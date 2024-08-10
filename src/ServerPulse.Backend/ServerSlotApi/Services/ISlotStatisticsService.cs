
namespace ServerSlotApi.Services
{
    public interface ISlotStatisticsService
    {
        public Task<bool> DeleteSlotStatisticsAsync(string key, string token, CancellationToken cancellationToken);
    }
}