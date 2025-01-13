using AuthenticationApi.Core.Models;

namespace AuthenticationApi.Application.Services
{
    public record OAuthAccessCodeParams(string Code, string CodeVerifier, string RedirectUrl);
    public record OAuthRequestUrlParams(string RedirectUrl, string CodeVerifier);

    public interface IOAuthService
    {
        public string GenerateOAuthRequestUrl(OAuthRequestUrlParams requestParams);
        public Task<ProviderLoginModel> GetProviderModelOnCodeAsync(OAuthAccessCodeParams requestParams, CancellationToken cancellationToken);
    }
}
