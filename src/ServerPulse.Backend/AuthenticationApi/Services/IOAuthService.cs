using Authentication.Models;

namespace AuthenticationApi.Services
{
    public record GetAccessOnCodeParams(string Code, string CodeVerifier, string RedirectUrl);
    public record GenerateOAuthRequestUrlParams(string RedirectUrl, string CodeVerifier);

    public interface IOAuthService
    {
        public string GenerateOAuthRequestUrl(GenerateOAuthRequestUrlParams generateUrlParams);
        public Task<AccessTokenData> GetAccessOnCodeAsync(GetAccessOnCodeParams accessOnCodeParams, CancellationToken cancellationToken);
    }
}
