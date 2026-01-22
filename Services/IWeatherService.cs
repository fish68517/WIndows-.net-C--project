namespace TourismPlatform.Services
{
    public interface IWeatherService
    {
        Task<WeatherForecast> GetForecastAsync();
    }

    public class WeatherForecast
    {
        public string City { get; set; }
        public List<DailyForecast> Forecasts { get; set; }
    }

    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public string Weather { get; set; }
        public int HighTemp { get; set; }
        public int LowTemp { get; set; }
        public string Wind { get; set; }
    }
}
