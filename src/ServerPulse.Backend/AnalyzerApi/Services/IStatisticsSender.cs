using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services
{
    public interface IStatisticsSender
    {
        Task SendStatisticsAsync(string key, ServerStatistics serverStatistics);
    }
}