using Shared.Helpers;

namespace ServerSlotApi.Services
{
    public class SlotStatisticsService : ISlotStatisticsService
    {
        private readonly IHttpHelper httpHelper;
        private readonly string deleteStatisticsUrl;

        public SlotStatisticsService(IHttpHelper httpHelper, IConfiguration configuration)
        {
            this.httpHelper = httpHelper;
            deleteStatisticsUrl = $"{configuration[Configuration.API_GATEWAY]}{configuration[Configuration.STATISTICS_DELETE_URL]}";
        }

        public async Task DeleteSlotStatisticsAsync(string key, string token, CancellationToken cancellationToken)
        {
            var requestUrl = deleteStatisticsUrl + key;
            await httpHelper.SendDeleteRequestAsync(requestUrl, token, cancellationToken);
        }
    }
}
