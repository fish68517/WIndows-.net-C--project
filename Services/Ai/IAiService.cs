using System.Threading.Tasks;

namespace TourismPlatform.Services.Ai
{
    public interface IAiService
    {
        Task<string> GetAnswerAsync(string userQuestion);
    }
}