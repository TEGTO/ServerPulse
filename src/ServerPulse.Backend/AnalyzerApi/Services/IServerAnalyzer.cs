using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services
{
    public interface IServerAnalyzer
    {
        public Task<ServerStatistics> GetServerStatisticsByKeyAsync(string key, CancellationToken cancellationToken);
    }
}