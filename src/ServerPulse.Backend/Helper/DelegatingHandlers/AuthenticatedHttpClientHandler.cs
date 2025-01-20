namespace Helper.DelegatingHandlers
{
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private readonly Func<Task<string>> getAccessToken;

        public AuthenticatedHttpClientHandler(Func<Task<string>> getAccessToken)
        {
            this.getAccessToken = getAccessToken;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await getAccessToken();

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
