using System.Collections.Generic;
using System.Threading.Tasks;

namespace TourismPlatform.Services.Weather
{
    public class DailyWeather
    {
        public string Date { get; set; }      // 日期
        public string Week { get; set; }      // 星期
        public string Icon { get; set; }      // 图标代码 (bi-icons)
        public string Text { get; set; }      // 天气状况 (晴/雨)
        public int TempMax { get; set; }      // 最高温
        public int TempMin { get; set; }      // 最低温
        public string Wind { get; set; }      // 风向
    }

    public interface IWeatherService
    {
        Task<List<DailyWeather>> GetJinanForecastAsync();
    }
}