using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TourismPlatform.Services.Ai
{
    public class SiliconFlowService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SiliconFlowService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            // 1. 读取配置
            var config = _configuration.GetSection("SiliconFlow");
            var baseUrl = config["BaseUrl"]?.ToString().TrimEnd('/');
            var apiKey = config["ApiKey"];
            var model = config["Model"];

            // 2. 构建请求 URL
            var url = $"{baseUrl}/chat/completions";

            // 3. 构建请求体 (OpenAI 兼容格式)
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "你是一个专业的济南旅游助手，请用热情、简洁的中文回答用户关于旅游的问题。" },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.7,
                max_tokens = 2000,
                stream = false
            };

            // 4. 发送请求
            var requestMsg = new HttpRequestMessage(HttpMethod.Post, url);
            requestMsg.Headers.Add("Authorization", $"Bearer {apiKey}");
            requestMsg.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            try 
            {
                var response = await _httpClient.SendAsync(requestMsg);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // 记录错误日志以便调试
                    Console.WriteLine($"[SiliconFlow Error] Status: {response.StatusCode}, Body: {responseString}");
                    return $"抱歉，AI 服务暂时不可用 (错误代码: {response.StatusCode})。";
                }

                // 5. 解析返回结果
                using var doc = JsonDocument.Parse(responseString);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return content ?? "AI 没有返回内容。";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] {ex.Message}");
                return "连接 AI 服务器超时，请稍后再试。";
            }
        }
    }
}