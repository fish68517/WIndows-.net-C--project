using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace TourismPlatform.Services.Ai
{
    public class OpenAiService : IAiService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OpenAiService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> GetAnswerAsync(string userQuestion)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            // 默认使用 OpenRouter 地址
            var apiUrl = _configuration["OpenAI:ApiUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
            // 默认使用免费的 Gemini 模型
            var modelName = _configuration["OpenAI:Model"] ?? "google/gemini-2.0-flash-exp:free";

            if (string.IsNullOrEmpty(apiKey))
            {
                return "请先在 appsettings.json 中配置 OpenRouter API Key。";
            }

            // 构造请求体 (OpenRouter 兼容 OpenAI 格式)
            var requestBody = new
            {
                model = modelName, 
                messages = new[]
                {
                    new { role = "system", content = "你是一个专业的济南旅游助手，只回答关于旅游、景点、酒店、美食和天气相关的问题。态度热情，回复简洁有用。" },
                    new { role = "user", content = userQuestion }
                },
                temperature = 0.7,
               // max_tokens = 100 // 可选：限制回复长度
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 添加 Headers
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            // OpenRouter 建议添加的额外头 (用于统计排名，可选)
            if (!_httpClient.DefaultRequestHeaders.Contains("HTTP-Referer"))
            {
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5000"); // 你的网站地址
                _httpClient.DefaultRequestHeaders.Add("X-Title", "Jinan Tourism App"); // 你的应用名称
            }

            try 
            {
                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode) 
                {
                    // 打印错误详情，方便调试
                    return $"AI 请求失败 ({response.StatusCode}): {responseString}";
                }

                dynamic result = JsonConvert.DeserializeObject(responseString);
                
                // OpenRouter 返回的结构和 OpenAI 完全一致
                string answer = result.choices[0].message.content;
                return answer;
            }
            catch (Exception ex)
            {
                return $"网络连接异常: {ex.Message}";
            }
        }
    }
}