using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IStatisticsSender
    {
        public Task SendServerStatisticsAsync(string key, ServerStatistics serverStatistics, CancellationToken cancellationToken);
        public Task SendServerLoadStatisticsAsync(string key, ServerLoadStatistics statistics, CancellationToken cancellationToken);
        public Task SendServerCustomStatisticsAsync(string key, CustomEventStatistics statistics, CancellationToken cancellationToken);
    }
}