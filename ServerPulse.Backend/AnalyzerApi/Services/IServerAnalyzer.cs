using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services
{
    public interface IServerAnalyzer
    {
        public Task<ServerStatus> GetCurrentServerStatusByKeyAsync(string key, CancellationToken cancellationToken);
    }
}