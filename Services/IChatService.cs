namespace TourismPlatform.Services
{
    public interface IChatService
    {
        Task<string> GetAnswerAsync(string question);
    }
}
