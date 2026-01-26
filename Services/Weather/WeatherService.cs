using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
// 1. 引入日志命名空间
using Microsoft.Extensions.Logging; 

namespace TourismPlatform.Services.Weather
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        // 2. 定义 Logger
        private readonly ILogger<WeatherService> _logger;

        // 3. 在构造函数中注入 Logger
        public WeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<DailyWeather>> GetJinanForecastAsync()
        {
            var apiUrl = _configuration["Weather:ApiUrl"] ?? "https://eolink.o.apispace.com/456456/weather/v001/day";
            var token = _configuration["Weather:Token"] ?? "5eiyf77i1th15ucp1syz63x1c6vz8bff";
            var areaCode = _configuration["Weather:AreaCode"] ?? "101120101"; 

            var requestUrl = $"{apiUrl}?days=7&areacode={areaCode}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("X-APISpace-Token", token);
            request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

            try
            {
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("【天气API报错】状态码: {StatusCode}", response.StatusCode);
                    return GetFallbackData($"API请求失败: {response.StatusCode}");
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                // ================================================================
                // 4. 【关键修改】打印获取到的原始 JSON 数据到控制台/日志
                // ================================================================
                _logger.LogInformation("【天气API原始响应】: \n{Json}", jsonString);
                // ================================================================

                var apiResponse = JsonConvert.DeserializeObject<WeatherResponse>(jsonString);

                if (apiResponse == null || apiResponse.Result == null || apiResponse.Result.DailyForecasts == null)
                {
                    _logger.LogWarning("【天气API警告】返回数据为空或格式不匹配");
                    return GetFallbackData("API返回数据为空");
                }

                var weatherList = apiResponse.Result.DailyForecasts.Select(item => new DailyWeather
                {
                    Date = ParseDate(item.Date),      
                    Week = item.Week,                 
                    Text = item.TextDay,              
                    Icon = GetIconByCode(item.CodeDay), 
                    TempMax = item.High,
                    TempMin = item.Low,
                    Wind = $"{item.WindDirDay} {item.WindClassDay}" 
                }).ToList();

                return weatherList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【天气API异常】发生错误");
                return GetFallbackData($"网络异常: {ex.Message}");
            }
        }

        // --- 辅助方法保持不变 ---

        private string GetIconByCode(string code)
        {
            return code switch
            {
                "00" => "bi-sun",              
                "01" => "bi-cloud-sun",        
                "02" => "bi-cloud",            
                "03" => "bi-cloud-rain",       
                "04" => "bi-cloud-lightning",  
                "07" => "bi-cloud-drizzle",    
                "08" => "bi-cloud-rain-heavy", 
                "21" => "bi-cloud-rain-heavy", 
                "06" => "bi-cloud-snow",       
                "14" => "bi-snow",             
                "18" => "bi-cloud-fog",        
                _ => "bi-cloud"                
            };
        }

        private string ParseDate(string dateStr)
        {
            if (DateTime.TryParse(dateStr, out var date))
            {
                return date.ToString("MM-dd");
            }
            return dateStr;
        }

        private List<DailyWeather> GetFallbackData(string errorMessage)
        {
            return new List<DailyWeather>
            {
                new DailyWeather { Date = "Error", Week = "错误", Text = "获取失败", Icon = "bi-exclamation-triangle", TempMax = 0, TempMin = 0, Wind = errorMessage }
            };
        }

        // --- DTO 类保持不变 ---
        public class WeatherResponse
        {
            [JsonProperty("status")]
            public int Status { get; set; }

            [JsonProperty("result")]
            public WeatherResult Result { get; set; }
        }

        public class WeatherResult
        {
            [JsonProperty("location")]
            public Location Location { get; set; }

            [JsonProperty("daily_fcsts")]
            public List<DailyForecastItem> DailyForecasts { get; set; }
        }

        public class Location
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class DailyForecastItem
        {
            [JsonProperty("text_day")]
            public string TextDay { get; set; } 

            [JsonProperty("code_day")]
            public string CodeDay { get; set; } 

            [JsonProperty("high")]
            public int High { get; set; }

            [JsonProperty("low")]
            public int Low { get; set; }

            [JsonProperty("wc_day")]
            public string WindClassDay { get; set; } 

            [JsonProperty("wd_day")]
            public string WindDirDay { get; set; }   

            [JsonProperty("date")]
            public string Date { get; set; }     

            [JsonProperty("week")]
            public string Week { get; set; }     
        }
    }
}