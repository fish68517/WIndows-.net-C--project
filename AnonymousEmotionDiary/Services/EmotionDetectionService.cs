using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AnonymousEmotionDiary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnonymousEmotionDiary.Services
{
    /// <summary>
    /// Emotion analysis service with keyword scoring and optional remote LLM enhancement.
    /// </summary>
    public class EmotionDetectionService : IEmotionDetectionService
    {
        private readonly LogService _logService;
        private readonly HttpClient _httpClient;

        private static readonly Dictionary<string, int> NegativeEmotionKeywords = new Dictionary<string, int>
        {
            { "难过", 78 }, { "伤心", 80 }, { "悲伤", 82 }, { "抑郁", 90 }, { "绝望", 95 },
            { "失望", 70 }, { "崩溃", 92 }, { "糟糕", 78 }, { "糟糕透顶", 92 }, { "心情差", 82 },
            { "心情太差", 88 }, { "痛苦", 88 }, { "压抑", 82 }, { "烦", 66 }, { "烦躁", 74 },
            { "郁闷", 72 }, { "焦虑", 80 }, { "担心", 68 }, { "害怕", 76 }, { "恐惧", 86 },
            { "紧张", 65 }, { "不安", 74 }, { "委屈", 72 }, { "生气", 76 }, { "愤怒", 84 },
            { "孤独", 80 }, { "寂寞", 74 }, { "压力", 72 }, { "累", 60 }, { "疲惫", 66 },
            { "失眠", 72 }, { "自残", 98 }, { "自杀", 100 }, { "想死", 100 }, { "不想活", 100 },
            { "活着没意义", 100 }, { "sad", 74 }, { "upset", 72 }, { "angry", 80 }, { "hopeless", 94 }
        };

        private static readonly Dictionary<string, int> PositiveEmotionKeywords = new Dictionary<string, int>
        {
            { "开心", 18 }, { "高兴", 18 }, { "快乐", 16 }, { "愉快", 18 }, { "喜悦", 16 },
            { "幸福", 14 }, { "满足", 24 }, { "满意", 24 }, { "轻松", 28 }, { "放松", 28 },
            { "平静", 30 }, { "安心", 28 }, { "温暖", 22 }, { "感恩", 20 }, { "感谢", 22 },
            { "希望", 24 }, { "期待", 24 }, { "很好", 24 }, { "不错", 28 }, { "顺利", 24 },
            { "天气很好", 20 }, { "郊游", 18 }, { "游玩", 18 }, { "旅行", 20 }, { "散步", 26 },
            { "happy", 16 }, { "calm", 28 }, { "great", 18 }, { "good", 24 }, { "relaxed", 26 }
        };

        public EmotionDetectionService()
        {
            _logService = new LogService();
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(ConfigurationHelper.LLMAPITimeout)
            };

            if (!string.IsNullOrWhiteSpace(ConfigurationHelper.LLMApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", ConfigurationHelper.LLMApiKey);
            }
        }

        public int AnalyzeEmotion(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return 50;
                }

                KeywordAnalysisResult keywordResult = AnalyzeKeywords(content);
                int llmEmotionIndex = CallLLMForSingleEntryAsync(content).GetAwaiter().GetResult();

                int finalIndex = llmEmotionIndex >= 0
                    ? Clamp((int)Math.Round(keywordResult.EmotionIndex * 0.4 + llmEmotionIndex * 0.6))
                    : keywordResult.EmotionIndex;

                _logService.LogDebug(
                    $"Emotion analysis result: keyword={keywordResult.EmotionIndex}, llm={llmEmotionIndex}, final={finalIndex}, matched=[{string.Join(", ", keywordResult.MatchedKeywords)}]");
                _logService.LogEmotionAnalysis(0, finalIndex, DateTime.Now, llmEmotionIndex >= 0 ? "keyword+llm" : "keyword-only");

                return finalIndex;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error during single diary emotion analysis.", ex);
                return 50;
            }
        }

        public UserEmotionProfile AnalyzeUserEmotionProfile(User user, IEnumerable<Diary> diaries)
        {
            List<Diary> diaryList = diaries?.OrderByDescending(d => d.CreatedAt).ToList() ?? new List<Diary>();
            UserEmotionProfile profile = new UserEmotionProfile
            {
                UserId = user?.UserId ?? 0,
                Username = user?.Username ?? "未知用户",
                ContactInfo = user?.ContactInfo ?? string.Empty,
                DiaryCount = diaryList.Count,
                LastDiaryAt = diaryList.FirstOrDefault()?.CreatedAt
            };

            if (diaryList.Count == 0)
            {
                profile.AverageEmotionIndex = 50;
                profile.LatestEmotionIndex = 50;
                profile.OverallEmotionIndex = 50;
                profile.RiskLevel = "低";
                profile.Summary = "该用户暂未写入日记，暂无可分析的情绪数据。";
                profile.SuggestedAction = "建议提醒用户持续记录，以便形成趋势分析。";
                return profile;
            }

            profile.HighRiskCount = diaryList.Count(d => d.IsHighRisk);
            profile.LatestEmotionIndex = diaryList.First().EmotionIndex;
            profile.AverageEmotionIndex = Clamp((int)Math.Round(diaryList.Average(d => d.EmotionIndex)));

            int heuristicIndex = BuildHeuristicProfileIndex(diaryList);
            LlmUserProfileResult llmResult = CallLLMForUserProfileAsync(user, diaryList).GetAwaiter().GetResult();

            profile.OverallEmotionIndex = llmResult != null ? Clamp(llmResult.OverallEmotionIndex) : heuristicIndex;
            profile.RiskLevel = !string.IsNullOrWhiteSpace(llmResult?.RiskLevel)
                ? llmResult.RiskLevel
                : ResolveRiskLevel(profile.OverallEmotionIndex, profile.HighRiskCount);
            profile.Summary = !string.IsNullOrWhiteSpace(llmResult?.Summary)
                ? llmResult.Summary
                : BuildFallbackSummary(profile);
            profile.SuggestedAction = !string.IsNullOrWhiteSpace(llmResult?.SuggestedAction)
                ? llmResult.SuggestedAction
                : BuildFallbackSuggestion(profile);

            _logService.LogEmotionAnalysis(0, profile.OverallEmotionIndex, DateTime.Now, llmResult != null ? "user-profile-llm" : "user-profile-heuristic");
            return profile;
        }

        public int ExtractKeywords(string content)
        {
            return AnalyzeKeywords(content).EmotionIndex;
        }

        public bool IsHighRisk(int emotionIndex)
        {
            return emotionIndex >= ConfigurationHelper.EmotionHighRiskThreshold;
        }

        public string GetRiskWarning(int emotionIndex)
        {
            return
                $"情绪预警\n\n系统检测到当前情绪指数为 {emotionIndex}/100，已达到高风险阈值。\n\n" +
                "如果你正在经历持续的压抑、绝望、自伤或轻生想法，请尽快联系家人、老师、辅导员或专业心理咨询机构。\n\n" +
                "建议资源：\n" +
                "1. 校园心理咨询中心或辅导员\n" +
                "2. 当地医院心理科 / 精神卫生中心\n" +
                "3. 可信赖的亲友，优先进行线下陪伴与陪诊";
        }

        private KeywordAnalysisResult AnalyzeKeywords(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new KeywordAnalysisResult(50, Array.Empty<string>());
            }

            string normalized = content.ToLowerInvariant();
            List<int> scores = new List<int>();
            List<string> matched = new List<string>();

            CollectMatches(normalized, NegativeEmotionKeywords, scores, matched);
            CollectMatches(normalized, PositiveEmotionKeywords, scores, matched);

            int emotionIndex = scores.Count == 0 ? 50 : Clamp((int)Math.Round(scores.Average()));
            return new KeywordAnalysisResult(emotionIndex, matched);
        }

        private static void CollectMatches(string normalizedContent, Dictionary<string, int> source, List<int> scores, List<string> matched)
        {
            foreach (KeyValuePair<string, int> item in source)
            {
                if (normalizedContent.Contains(item.Key.ToLowerInvariant()))
                {
                    scores.Add(item.Value);
                    matched.Add(item.Key);
                }
            }
        }

        private async Task<int> CallLLMForSingleEntryAsync(string content)
        {
            try
            {
                string prompt =
                    "Please rate the emotional tone of the diary entry on a 0-100 scale, where 0 is very positive and 100 is very negative. " +
                    "Return only one integer.\n\n" +
                    $"Diary Entry:\n{content}\n\nEmotion Index:";

                string responseText = await SendPromptAsync(prompt);
                if (int.TryParse(responseText.Trim(), out int emotionIndex))
                {
                    return Clamp(emotionIndex);
                }

                _logService.LogDebug($"LLM single-entry analysis returned non-integer content: {responseText}");
            }
            catch (Exception ex)
            {
                _logService.LogDebug($"LLM single-entry analysis failed: {ex.Message}");
            }

            return -1;
        }

        private async Task<LlmUserProfileResult> CallLLMForUserProfileAsync(User user, List<Diary> diaries)
        {
            try
            {
                string history = BuildUserHistoryPrompt(diaries);
                string prompt =
                    "You are an emotion profiling assistant. Analyze the user's diary history and output strict JSON only.\n" +
                    "Schema:\n" +
                    "{\"overallEmotionIndex\":0,\"riskLevel\":\"low|medium|high|critical\",\"summary\":\"...\",\"suggestedAction\":\"...\"}\n" +
                    "Rules: overallEmotionIndex must be 0-100; summary under 80 Chinese characters or 160 English chars; suggestedAction under 60 Chinese chars or 120 English chars.\n\n" +
                    $"Username: {user?.Username}\n" +
                    $"Contact: {(string.IsNullOrWhiteSpace(user?.ContactInfo) ? "N/A" : user.ContactInfo)}\n" +
                    $"Diary History:\n{history}";

                string responseText = await SendPromptAsync(prompt);
                string json = ExtractJsonObject(responseText);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logService.LogDebug($"LLM user-profile analysis returned invalid JSON: {responseText}");
                    return null;
                }

                JObject parsed = JObject.Parse(json);
                return new LlmUserProfileResult
                {
                    OverallEmotionIndex = Clamp(parsed.Value<int?>("overallEmotionIndex") ?? 50),
                    RiskLevel = NormalizeRiskLevel(parsed.Value<string>("riskLevel")),
                    Summary = parsed.Value<string>("summary") ?? string.Empty,
                    SuggestedAction = parsed.Value<string>("suggestedAction") ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logService.LogDebug($"LLM user-profile analysis failed: {ex.Message}");
                return null;
            }
        }

        private async Task<string> SendPromptAsync(string prompt)
        {
            if (IsSiliconFlow())
            {
                return await SendSiliconFlowPromptAsync(prompt);
            }

            var requestPayload = new
            {
                model = ConfigurationHelper.LLMAPIModel,
                prompt,
                stream = false
            };

            string payload = JsonConvert.SerializeObject(requestPayload);
            using StringContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _httpClient.PostAsync(ConfigurationHelper.LLMAPIEndpoint, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"LLM API returned status {(int)response.StatusCode}.");
            }

            string rawResponse = await response.Content.ReadAsStringAsync();
            JObject parsed = JObject.Parse(rawResponse);
            return parsed.Value<string>("response") ?? string.Empty;
        }

        private async Task<string> SendSiliconFlowPromptAsync(string prompt)
        {
            string endpoint = string.IsNullOrWhiteSpace(ConfigurationHelper.LLMAPIEndpoint)
                ? ConfigurationHelper.LLMBaseUrl.TrimEnd('/') + "/chat/completions"
                : ConfigurationHelper.LLMAPIEndpoint;

            var requestPayload = new
            {
                model = ConfigurationHelper.LLMAPIModel,
                stream = false,
                temperature = 0.2,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a precise assistant that must follow output format strictly."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                }
            };

            string payload = JsonConvert.SerializeObject(requestPayload);
            using StringContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _httpClient.PostAsync(endpoint, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                string errorText = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"SiliconFlow API returned status {(int)response.StatusCode}: {errorText}");
            }

            string rawResponse = await response.Content.ReadAsStringAsync();
            JObject parsed = JObject.Parse(rawResponse);
            return parsed["choices"]?.FirstOrDefault()?["message"]?["content"]?.ToString() ?? string.Empty;
        }

        private static bool IsSiliconFlow()
        {
            return string.Equals(ConfigurationHelper.LLMProvider, "SiliconFlow", StringComparison.OrdinalIgnoreCase)
                || ConfigurationHelper.LLMAPIEndpoint.Contains("siliconflow.cn", StringComparison.OrdinalIgnoreCase)
                || ConfigurationHelper.LLMBaseUrl.Contains("siliconflow.cn", StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractJsonObject(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            int start = text.IndexOf('{');
            int end = text.LastIndexOf('}');

            if (start < 0 || end <= start)
            {
                return string.Empty;
            }

            return text.Substring(start, end - start + 1);
        }

        private static int BuildHeuristicProfileIndex(List<Diary> diaries)
        {
            int latest = diaries.First().EmotionIndex;
            int average = Clamp((int)Math.Round(diaries.Average(d => d.EmotionIndex)));
            int highRiskBoost = Math.Min(15, diaries.Count(d => d.IsHighRisk) * 4);
            return Clamp((int)Math.Round(average * 0.55 + latest * 0.35 + highRiskBoost));
        }

        private static string BuildUserHistoryPrompt(List<Diary> diaries)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Total Diaries: {diaries.Count}");
            builder.AppendLine($"Average Emotion Index: {Clamp((int)Math.Round(diaries.Average(d => d.EmotionIndex)))}");
            builder.AppendLine($"High Risk Diaries: {diaries.Count(d => d.IsHighRisk)}");

            int totalLength = builder.Length;
            foreach (Diary diary in diaries)
            {
                string preview = diary.Content.Length > 240 ? diary.Content.Substring(0, 240) + "..." : diary.Content;
                string line = $"[{diary.CreatedAt:yyyy-MM-dd HH:mm}] emotion={diary.EmotionIndex}, highRisk={diary.IsHighRisk}, content={preview}";

                if (totalLength + line.Length > ConfigurationHelper.LLMUserProfileMaxChars)
                {
                    builder.AppendLine("...remaining diary samples omitted due to prompt length limit.");
                    break;
                }

                builder.AppendLine(line);
                totalLength += line.Length;
            }

            return builder.ToString();
        }

        private static string NormalizeRiskLevel(string riskLevel)
        {
            if (string.IsNullOrWhiteSpace(riskLevel))
            {
                return string.Empty;
            }

            string normalized = riskLevel.Trim().ToLowerInvariant();
            return normalized switch
            {
                "low" => "低",
                "medium" => "中",
                "high" => "高",
                "critical" => "极高",
                "低" => "低",
                "中" => "中",
                "高" => "高",
                "极高" => "极高",
                _ => riskLevel
            };
        }

        private static string ResolveRiskLevel(int overallEmotionIndex, int highRiskCount)
        {
            if (overallEmotionIndex >= 85 || highRiskCount >= 3)
            {
                return "极高";
            }

            if (overallEmotionIndex >= 70 || highRiskCount >= 1)
            {
                return "高";
            }

            if (overallEmotionIndex >= 55)
            {
                return "中";
            }

            return "低";
        }

        private static string BuildFallbackSummary(UserEmotionProfile profile)
        {
            return $"共分析 {profile.DiaryCount} 篇日记，平均情绪指数 {profile.AverageEmotionIndex}，最近一次为 {profile.LatestEmotionIndex}，综合风险等级为 {profile.RiskLevel}。";
        }

        private static string BuildFallbackSuggestion(UserEmotionProfile profile)
        {
            if (profile.RiskLevel == "极高" || profile.RiskLevel == "高")
            {
                return "建议管理员优先联系并确认用户当前状态，必要时联动线下支持资源。";
            }

            if (profile.RiskLevel == "中")
            {
                return "建议持续观察近期日记趋势，并鼓励用户保持稳定记录。";
            }

            return "当前总体情绪较稳定，可继续通过日记追踪变化。";
        }

        private static int Clamp(int value)
        {
            return Math.Max(ConfigurationHelper.EmotionIndexMin, Math.Min(ConfigurationHelper.EmotionIndexMax, value));
        }

        private sealed class KeywordAnalysisResult
        {
            public KeywordAnalysisResult(int emotionIndex, IReadOnlyList<string> matchedKeywords)
            {
                EmotionIndex = emotionIndex;
                MatchedKeywords = matchedKeywords;
            }

            public int EmotionIndex { get; }

            public IReadOnlyList<string> MatchedKeywords { get; }
        }

        private sealed class LlmUserProfileResult
        {
            public int OverallEmotionIndex { get; set; }

            public string RiskLevel { get; set; }

            public string Summary { get; set; }

            public string SuggestedAction { get; set; }
        }
    }
}
