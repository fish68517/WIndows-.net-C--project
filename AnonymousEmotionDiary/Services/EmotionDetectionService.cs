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
    /// 用于检测并分析日记内容情绪的服务。
    /// 结合关键词提取与 LLM API 分析生成情绪指数。
    /// </summary>
    public class EmotionDetectionService : IEmotionDetectionService
    {
        private readonly LogService _logService;
        private readonly HttpClient _httpClient;

         // 情绪关键词映射：关键词 -> 情绪强度（0-100）
        private static readonly Dictionary<string, int> NegativeEmotionKeywords = new Dictionary<string, int>
        {
            // 悲伤与抑郁
            { "难过", 75 },
            { "伤心", 75 },
            { "悲伤", 80 },
            { "抑郁", 85 },
            { "绝望", 90 },
            { "失望", 70 },
            { "沮丧", 75 },
            { "郁闷", 70 },
            
            // 焦虑与担忧
            { "焦虑", 75 },
            { "担心", 60 },
            { "害怕", 75 },
            { "恐惧", 80 },
            { "紧张", 65 },
            { "不安", 70 },
            { "惶恐", 80 },
            
            // 愤怒与挫败
            { "生气", 75 },
            { "愤怒", 80 },
            { "恼怒", 75 },
            { "烦躁", 70 },
            { "气愤", 80 },
            { "怨恨", 85 },
            { "厌烦", 65 },
            
            // 孤独与疏离
            { "孤独", 75 },
            { "寂寞", 70 },
            { "孤单", 70 },
            { "被遗弃", 85 },
            { "无人理解", 80 },
            
            // 压力与紧张
            { "压力", 70 },
            { "压抑", 75 },
            { "疲惫", 65 },
            { "疲劳", 60 },
            { "累", 55 },
            
            // 自伤与自杀意念
            { "自杀", 95 },
            { "自伤", 90 },
            { "死亡", 85 },
            { "活着没意义", 90 },
            { "不想活", 90 },
            { "想死", 90 }
        };

        private static readonly Dictionary<string, int> PositiveEmotionKeywords = new Dictionary<string, int>
        {
            // 快乐与喜悦
            { "开心", 20 },
            { "高兴", 20 },
            { "快乐", 15 },
            { "喜悦", 15 },
            { "兴奋", 25 },
            { "欣喜", 20 },

            // 满足与满意
            { "满足", 25 },
            { "满意", 25 },
            { "舒适", 30 },
            { "放松", 30 },
            { "平静", 35 },
            { "安心", 30 },
            
            // 希望与乐观
            { "希望", 25 },
            { "乐观", 20 },
            { "期待", 25 },
            { "憧憬", 20 },
            
            // 爱与连接感
            { "爱", 20 },
            { "喜欢", 25 },
            { "感谢", 25 },
            { "感恩", 20 },
            { "温暖", 25 }
        };

         /// <summary>
        /// 初始化 EmotionDetectionService 类的新实例。
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
        /// 分析给定内容中的情绪并返回情绪指数。
        /// 结合关键词提取与 LLM API 分析。
        /// 当 API 调用失败时回退为仅基于关键词的分析。
        /// </summary>
        /// <param name="content">要分析的日记内容。</param>
        /// <returns>返回 0 到 100 之间的情绪指数值。</returns>
        public int AnalyzeEmotion(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logService.LogDebug("Emotion analysis: content is empty, returning neutral emotion index 50");
                    return 50;
                }

                // 步骤 1：提取关键词并计算基于关键词的情绪指数
                int keywordEmotionIndex = ExtractKeywords(content);
                _logService.LogDebug($"Emotion analysis: keyword-based emotion index = {keywordEmotionIndex}");

                // 步骤 2：尝试调用 LLM API 进行更深入的分析
                int llmEmotionIndex = CallLLMAPI(content).Result;

                if (llmEmotionIndex >= 0)
                {
                    // 成功获取 LLM 结果，与关键词分析进行融合
                    // 权重：关键词 40%，LLM 60%
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
                    // LLM API 调用失败，使用仅关键词分析
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
        /// 从内容中提取情绪关键词，并基于关键词计算情绪指数。
        /// </summary>
        /// <param name="content">要分析的日记内容。</param>
        /// <returns>基于关键词分析得到的情绪指数（0-100）。</returns>
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
        /// 调用 LLM API 对内容进行深度情绪分析。
        /// 若 API 调用失败则返回 -1。
        /// </summary>
        /// <param name="content">要分析的日记内容。</param>
        /// <returns>LLM 分析得到的情绪指数（0-100）；若 API 调用失败则返回 -1。</returns>
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
        /// 判断情绪指数是否指示高风险情绪状态。
        /// </summary>
        /// <param name="emotionIndex">要检查的情绪指数。</param>
        /// <returns>若情绪指数超过高风险阈值则返回 True，否则返回 False。</returns>
        public bool IsHighRisk(int emotionIndex)
        {
            return emotionIndex > ConfigurationHelper.EmotionHighRiskThreshold;
        }

             /// <summary>
        /// 为高风险情绪生成风险提示信息。
        /// 包含心理支持资源信息。
        /// </summary>
        /// <param name="emotionIndex">触发提示的情绪指数。</param>
        /// <returns>包含支持资源的提示信息。</returns>
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
