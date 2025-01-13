using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Infrastructure.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AnalyzerApi.Infrastructure
{
    public static class EndpointRouteBuilderExtenstions
    {
        public static IEndpointRouteBuilder MapHubs(this IEndpointRouteBuilder app)
        {
            #region Hubs

            app.MapHub<StatisticsHub<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>>("/lifecyclestatisticshub");
            app.MapHub<StatisticsHub<ServerLoadStatistics, ServerLoadStatisticsResponse>>("/loadstatisticshub");
            app.MapHub<StatisticsHub<ServerCustomStatistics, ServerCustomStatisticsResponse>>("/customstatisticshub");

            #endregion

            return app;
        }
    }
}
