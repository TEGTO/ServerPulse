namespace Shared.Helpers
{
    public interface IHttpHelper
    {
        public Task<T?> SendGetRequestAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null, string? accessToken = null, CancellationToken cancellationToken = default);
        public Task<T?> SendPostRequestAsync<T>(string endpoint, Dictionary<string, string> bodyParams, string? accessToken = null, CancellationToken cancellationToken = default);
        public Task<T?> SendPostRequestAsync<T>(string endpoint, string json, string? accessToken = null, CancellationToken cancellationToken = default);
        public Task SendPostRequestAsync(string endpoint, string json, string? accessToken = null, CancellationToken cancellationToken = default);
        public Task SendDeleteRequestAsync(string endpoint, string? accessToken = null, CancellationToken cancellationToken = default);
        public Task SendPutRequestAsync(string endpoint, string jsonBody, Dictionary<string, string>? queryParams = null, string? accessToken = null, CancellationToken cancellationToken = default);
    }
}