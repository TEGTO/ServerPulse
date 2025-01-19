using AuthenticationApi.Core.Models;

namespace AuthenticationApi.Application.Services
{
    public interface IOAuthService
    {
        public Task<string> GenerateOAuthRequestUrlAsync(string redirectUrl, CancellationToken cancellationToken);
        public Task<ProviderLoginModel> GetProviderModelOnCodeAsync(string code, string redirectUrl, CancellationToken cancellationToken);
    }
}
