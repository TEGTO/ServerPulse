
namespace ServerMonitorApi.Services
{
    public interface IStatisticsControlService
    {
        public Task DeleteStatisticsByKeyAsync(string key);
    }
}