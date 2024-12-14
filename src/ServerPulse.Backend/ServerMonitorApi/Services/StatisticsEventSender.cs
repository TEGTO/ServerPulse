using EventCommunication;
using Shared.Helpers;
using System.Text.Json;

namespace ServerMonitorApi.Services
{
    public class StatisticsEventSender : IStatisticsEventSender
    {
        private readonly IHttpHelper httpHelper;
        private readonly string loadAnalyzeUri;

        public StatisticsEventSender(IHttpHelper httpHelper, IConfiguration configuration)
        {
            this.httpHelper = httpHelper;

            loadAnalyzeUri = $"{configuration[Configuration.API_GATEWAY]}{configuration[Configuration.ANALYZER_LOAD_EVENT]}";
        }

        public async Task SendLoadEventForStatistics(LoadEvent ev, CancellationToken cancellationToken)
        {
            var serializedEvents = JsonSerializer.Serialize(ev);

            await httpHelper.SendPostRequestAsync(loadAnalyzeUri, serializedEvents, null, cancellationToken);
        }
    }
}
