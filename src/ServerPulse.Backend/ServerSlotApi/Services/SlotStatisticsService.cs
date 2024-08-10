using System.Net.Http.Headers;

namespace ServerSlotApi.Services
{
    public class SlotStatisticsService : ISlotStatisticsService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string deleteStatisticsUrl;

        public SlotStatisticsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            deleteStatisticsUrl = $"{configuration[Configuration.API_GATEWAY]}{configuration[Configuration.STATISTICS_DELETE_URL]}";
        }

        public async Task<bool> DeleteSlotStatisticsAsync(string key, string token, CancellationToken cancellationToken)
        {
            var requestUrl = deleteStatisticsUrl + key;
            var httpClient = httpClientFactory.CreateClient();

            using (var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.SendAsync(request, cancellationToken);
                return response.IsSuccessStatusCode;
            }
        }
    }
}