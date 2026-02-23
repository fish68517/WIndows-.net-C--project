using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AnonymousEmotionDiary.Services
{
    /// <summary>
    /// Service for detecting and analyzing emotions in diary content.
    /// Combines keyword extraction with LLM API analysis to generate emotion indices.
    /// </summary>
    public class EmotionDetectionService : IEmotionDetectionService
    {
        private readonly LogService _logService;
        private readonly HttpClient _httpClient;

        // Emotion keywords mapping: keyword -> emotion intensity (0-100)
        private static readonly Dictionary<string, int> NegativeEmotionKeywords = new Dictionary<string, int>
        {
            // Sadness and depression
            { "难过", 75 },
            { "伤心", 75 },
            { "悲伤", 80 },
            { "抑郁", 85 },
            { "绝望", 90 },
            { "失望", 70 },
            { "沮丧", 75 },
            { "郁闷", 70 },
            
            // Anxiety and worry
            { "焦虑", 75 },
            { "担心", 60 },
            { "害怕", 75 },
            { "恐惧", 80 },
            { "紧张", 65 },
            { "不安", 70 },
            { "惶恐", 80 },
            
            // Anger and frustration
            { "生气", 75 },
            { "愤怒", 80 },
            { "恼怒", 75 },
            { "烦躁", 70 },
            { "气愤", 80 },
            { "怨恨", 85 },
            { "厌烦", 65 },
            
            // Loneliness and isolation
            { "孤独", 75 },
            { "寂寞", 70 },
            { "孤单", 70 },
            { "被遗弃", 85 },
            { "无人理解", 80 },
            
            // Stress and pressure
            { "压力", 70 },
            { "压抑", 75 },
            { "疲惫", 65 },
            { "疲劳", 60 },
            { "累", 55 },
            
            // Self-harm and suicidal ideation
            { "自杀", 95 },
            { "自伤", 90 },
            { "死亡", 85 },
            { "活着没意义", 90 },
            { "不想活", 90 },
            { "想死", 90 }
        };

        private static readonly Dictionary<string, int> PositiveEmotionKeywords = new Dictionary<string, int>
        {
            // Happiness and joy
            { "开心", 20 },
            { "高兴", 20 },
            { "快乐", 15 },
            { "喜悦", 15 },
            { "兴奋", 25 },
            { "欣喜", 20 },
            
            // Contentment and satisfaction
            { "满足", 25 },
            { "满意", 25 },
            { "舒适", 30 },
            { "放松", 30 },
            { "平静", 35 },
            { "安心", 30 },
            
            // Hope and optimism
            { "希望", 25 },
            { "乐观", 20 },
            { "期待", 25 },
            { "憧憬", 20 },
            
            // Love and connection
            { "爱", 20 },
            { "喜欢", 25 },
            { "感谢", 25 },
            { "感恩", 20 },
            { "温暖", 25 }
        };

        /// <summary>
        /// Initializes a new instance of the EmotionDetectionService class.
        /// </summary>
        public EmotionDetectionService()
        {
            _logService = new LogService();
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(ConfigurationHelper.LLMAPITimeout)
            };
        }

        /// <summary>
        /// Analyzes the emotion in the provided content and returns an emotion index.
        /// Combines keyword extraction with LLM API analysis.
        /// Falls back to keyword-only analysis if API fails.
        /// </summary>
        /// <param name="content">The diary content to analyze.</param>
        /// <returns>An emotion index value between 0 and 100.</returns>
        public int AnalyzeEmotion(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logService.LogDebug("Emotion analysis: content is empty, returning neutral emotion index 50");
                    return 50;
                }

                // Step 1: Extract keywords and get keyword-based emotion index
                int keywordEmotionIndex = ExtractKeywords(content);
                _logService.LogDebug($"Emotion analysis: keyword-based emotion index = {keywordEmotionIndex}");

                // Step 2: Try to call LLM API for deeper analysis
                int llmEmotionIndex = CallLLMAPI(content).Result;

                if (llmEmotionIndex >= 0)
                {
                    // Successfully got LLM result, combine with keyword analysis
                    // Weight: 40% keyword, 60% LLM
                    int combinedIndex = (int)Math.Round(keywordEmotionIndex * 0.4 + llmEmotionIndex * 0.6);
                    combinedIndex = Math.Max(0, Math.Min(100, combinedIndex)); // Clamp to 0-100
                    
                    _logService.LogEmotionAnalysis(
                        diaryId: 0,
                        emotionIndex: combinedIndex,
                        analysisTime: DateTime.Now,
                        modelVersion: "keyword+llm"
                    );
                    
                    return combinedIndex;
                }
                else
                {
                    // LLM API failed, use keyword-only analysis
                    _logService.LogDebug("Emotion analysis: LLM API failed, using keyword-only analysis");
                    _logService.LogEmotionAnalysis(
                        diaryId: 0,
                        emotionIndex: keywordEmotionIndex,
                        analysisTime: DateTime.Now,
                        modelVersion: "keyword-only"
                    );
                    
                    return keywordEmotionIndex;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError("Error during emotion analysis", ex);
                // Return neutral emotion index on error
                return 50;
            }
        }

        /// <summary>
        /// Extracts emotion keywords from the content and calculates emotion index based on keywords.
        /// </summary>
        /// <param name="content">The diary content to analyze.</param>
        /// <returns>An emotion index based on keyword analysis (0-100).</returns>
        public int ExtractKeywords(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return 50; // Neutral
                }

                string lowerContent = content.ToLower();
                List<int> foundEmotionValues = new List<int>();

                // Search for negative emotion keywords
                foreach (var keyword in NegativeEmotionKeywords.Keys)
                {
                    if (lowerContent.Contains(keyword.ToLower()))
                    {
                        foundEmotionValues.Add(NegativeEmotionKeywords[keyword]);
                    }
                }

                // Search for positive emotion keywords
                foreach (var keyword in PositiveEmotionKeywords.Keys)
                {
                    if (lowerContent.Contains(keyword.ToLower()))
                    {
                        foundEmotionValues.Add(PositiveEmotionKeywords[keyword]);
                    }
                }

                // Calculate average emotion index from found keywords
                if (foundEmotionValues.Count > 0)
                {
                    int averageIndex = (int)Math.Round(foundEmotionValues.Average());
                    _logService.LogDebug($"Keyword extraction: found {foundEmotionValues.Count} emotion keywords, average index = {averageIndex}");
                    return Math.Max(0, Math.Min(100, averageIndex));
                }

                // No keywords found, return neutral
                _logService.LogDebug("Keyword extraction: no emotion keywords found, returning neutral index 50");
                return 50;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error during keyword extraction", ex);
                return 50;
            }
        }

        /// <summary>
        /// Calls the LLM API to perform deep emotion analysis on the content.
        /// Returns -1 if the API call fails.
        /// </summary>
        /// <param name="content">The diary content to analyze.</param>
        /// <returns>An emotion index from LLM analysis (0-100), or -1 if API call fails.</returns>
        public async Task<int> CallLLMAPI(string content)
        {
            try
            {
                string apiEndpoint = ConfigurationHelper.LLMAPIEndpoint;
                string modelName = ConfigurationHelper.LLMAPIModel;

                // Prepare the prompt for emotion analysis
                string prompt = $@"Analyze the emotional tone of the following diary entry and provide an emotion index from 0 to 100, where 0 is very positive and 100 is very negative. 
Only respond with a single number between 0 and 100.

Diary entry:
{content}

Emotion index:";

                // Create request payload
                var requestPayload = new
                {
                    model = modelName,
                    prompt = prompt,
                    stream = false
                };

                string jsonPayload = JsonConvert.SerializeObject(requestPayload);
                var content_http = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _logService.LogDebug($"Calling LLM API at {apiEndpoint} with model {modelName}");

                // Make the API call
                HttpResponseMessage response = await _httpClient.PostAsync(apiEndpoint, content_http);

                if (!response.IsSuccessStatusCode)
                {
                    _logService.LogDebug($"LLM API call failed with status code: {response.StatusCode}");
                    return -1;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                _logService.LogDebug($"LLM API response received: {responseContent.Substring(0, Math.Min(100, responseContent.Length))}...");

                // Parse the response
                dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
                string responseText = responseObject.response;

                // Extract emotion index from response
                if (int.TryParse(responseText.Trim(), out int emotionIndex))
                {
                    emotionIndex = Math.Max(0, Math.Min(100, emotionIndex)); // Clamp to 0-100
                    _logService.LogDebug($"LLM API emotion index extracted: {emotionIndex}");
                    return emotionIndex;
                }

                _logService.LogDebug($"Failed to parse emotion index from LLM response: {responseText}");
                return -1;
            }
            catch (HttpRequestException ex)
            {
                _logService.LogDebug($"LLM API HTTP request failed: {ex.Message}");
                return -1;
            }
            catch (TaskCanceledException ex)
            {
                _logService.LogDebug($"LLM API request timeout: {ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error calling LLM API", ex);
                return -1;
            }
        }

        /// <summary>
        /// Determines if an emotion index indicates high-risk emotional state.
        /// </summary>
        /// <param name="emotionIndex">The emotion index to check.</param>
        /// <returns>True if emotion index exceeds the high-risk threshold, false otherwise.</returns>
        public bool IsHighRisk(int emotionIndex)
        {
            return emotionIndex > ConfigurationHelper.EmotionHighRiskThreshold;
        }

        /// <summary>
        /// Generates a risk warning message for high-risk emotions.
        /// Includes psychological support resources.
        /// </summary>
        /// <param name="emotionIndex">The emotion index that triggered the warning.</param>
        /// <returns>A warning message with support resources.</returns>
        public string GetRiskWarning(int emotionIndex)
        {
            string warning = $@"⚠️ 情绪预警提示

我们检测到您的日记内容反映出较为负面的情绪状态（情绪指数: {emotionIndex}/100）。

如果您正在经历困难或有任何心理健康方面的顾虑，请不要犹豫寻求帮助。以下是一些可用的心理援助资源：

📞 心理援助热线:
- 全国心理援助热线: 400-161-9995
- 生命热线: 400-821-1215

💻 在线心理咨询:
- 心理援助平台: https://www.xinli.com
- 心理咨询服务: https://www.xlzx.cn

🏥 专业医疗机构:
- 请联系当地医院心理科或精神卫生中心
- 校内心理咨询中心（如适用）

记住：寻求帮助是勇敢的表现，您不必独自承受。";

            return warning;
        }
    }
}
