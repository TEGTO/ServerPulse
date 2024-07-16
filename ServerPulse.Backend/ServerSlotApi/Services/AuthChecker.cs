using Shared.Dtos;
using System.Text;
using System.Text.Json;

namespace ServerSlotApi.Services
{
    public class AuthChecker : IAuthChecker
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;

        public AuthChecker(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<bool> CheckAuthDataAsync(string login, string password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email and password must be provided");
            }
            var checkAuthDataRequest = new CheckAuthDataRequest()
            {
                Login = login,
                Password = password
            };
            var authApi = configuration["AuthApi"];
            var requestUrl = $"{authApi}/auth/check";
            var httpClient = httpClientFactory.CreateClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(checkAuthDataRequest),
                Encoding.UTF8,
                "application/json"
            );
            var httpResponseMessage = await httpClient.PostAsync(requestUrl, jsonContent, cancellationToken);
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeAsync<CheckAuthDataResponse>(contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return response.IsCorrect;
        }
    }
}