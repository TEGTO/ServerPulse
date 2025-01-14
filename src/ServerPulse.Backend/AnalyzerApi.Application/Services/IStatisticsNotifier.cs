using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;

namespace AnalyzerApi.Application.Application.Services
{
    public interface IStatisticsNotifier<T, Y> where T : BaseStatistics where Y : BaseStatisticsResponse
    {
        public Task NotifyGroupAsync(string groupKey, Y data);
    }
}
