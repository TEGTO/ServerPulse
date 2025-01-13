using AnalyzerApi.Application.Application.Services;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AnalyzerApi.Infrastructure.Services
{
    internal class SignalRStatisticsNotifier<T, Y> : IStatisticsNotifier<T, Y> where T : BaseStatistics where Y : BaseStatisticsResponse
    {
        private readonly IHubContext<StatisticsHub<T, Y>, IStatisticsHubClient<Y>> hubStatistics;

        public SignalRStatisticsNotifier(IHubContext<StatisticsHub<T, Y>, IStatisticsHubClient<Y>> hubStatistics)
        {
            this.hubStatistics = hubStatistics;
        }

        public async Task NotifyGroupAsync(string groupKey, Y data)
        {
            await hubStatistics.Clients.Group(groupKey).ReceiveStatistics(groupKey, data);
        }
    }
}
