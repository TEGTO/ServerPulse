
namespace ServerSlotApi.Services
{
    public interface ISlotStatisticsService
    {
        public Task DeleteSlotStatisticsAsync(string key, string token, CancellationToken cancellationToken);
    }
}