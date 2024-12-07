using AnalyzerApi.Infrastructure.Models;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IStatisticsSender
    {
        public Task SendStatisticsAsync<TStatistics>(string key, TStatistics statistics, CancellationToken cancellationToken) where TStatistics : BaseStatistics;
    }
}