using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services
{
    public interface IServerAnalyzer
    {
        public Task<AnalyzedData> GetAnalyzedDataByKeyAsync(string key);
    }
}