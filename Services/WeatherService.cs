using Microsoft.Extensions.Caching.Memory; // <--- 关键是加上这一行
using TourismPlatform.Models;
using TourismPlatform.Services;


namespace TourismPlatform.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "weather_forecast";
        private const int CacheDurationMinutes = 180; // 3 hours

        public WeatherService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<WeatherForecast> GetForecastAsync()
        {
            // Try to get from cache
            if (_cache.TryGetValue(CacheKey, out WeatherForecast cachedForecast))
            {
                return cachedForecast;
            }

            // Default forecast if API call fails
            var forecast = new WeatherForecast
            {
                City = "济南",
                Forecasts = new List<DailyForecast>
                {
                    new DailyForecast { Date = DateTime.Now, Weather = "晴", HighTemp = 25, LowTemp = 15, Wind = "东风3级" },
                    new DailyForecast { Date = DateTime.Now.AddDays(1), Weather = "多云", HighTemp = 24, LowTemp = 14, Wind = "东风2级" },
                    new DailyForecast { Date = DateTime.Now.AddDays(2), Weather = "晴", HighTemp = 26, LowTemp = 16, Wind = "南风2级" },
                    new DailyForecast { Date = DateTime.Now.AddDays(3), Weather = "阴", HighTemp = 22, LowTemp = 12, Wind = "北风3级" },
                    new DailyForecast { Date = DateTime.Now.AddDays(4), Weather = "小雨", HighTemp = 20, LowTemp = 10, Wind = "北风4级" }
                }
            };

            // Cache the forecast
            _cache.Set(CacheKey, forecast, TimeSpan.FromMinutes(CacheDurationMinutes));

            return await Task.FromResult(forecast);
        }
    }
}
