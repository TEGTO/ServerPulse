using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Infrastructure.Hubs;

namespace AnalyzerApi
{
    public static class EndpointRouteBuilderExtenstions
    {
        public static IEndpointRouteBuilder MapHubs(this IEndpointRouteBuilder app)
        {
            app.MapHub<StatisticsHub<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>>("/lifecyclestatisticshub");
            app.MapHub<StatisticsHub<ServerLoadStatistics, ServerLoadStatisticsResponse>>("/loadstatisticshub");
            app.MapHub<StatisticsHub<ServerCustomStatistics, ServerCustomStatisticsResponse>>("/customstatisticshub");

            return app;
        }
    }
}
