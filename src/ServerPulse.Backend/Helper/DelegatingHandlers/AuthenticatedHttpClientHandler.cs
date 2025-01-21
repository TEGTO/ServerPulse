using System.Net.Http.Headers;

namespace Helper.DelegatingHandlers
{
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization != null && !request.Headers.Authorization.Scheme.Contains("Bearer"))
            {
                var token = request.Headers.Authorization.Scheme;
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
