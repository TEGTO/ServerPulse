using AuthenticationApi.Application.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace AuthenticationApi.Infrastructure.Services
{
    public class StringVerifierService : IStringVerifierService
    {
        private const string CODE_VERIFY_KEY = "CodeVerify";

        private readonly IDistributedCache distributedCache;
        private readonly int expirationTimeInMinutes;

        public StringVerifierService(IDistributedCache distributedCache, IConfiguration configuration)
        {
            this.distributedCache = distributedCache;
            expirationTimeInMinutes = int.Parse(
                configuration[ConfigurationKeys.CODE_VERIFY_EXPIRATION_TIME_IN_MINUTES] ?? "10");
        }

        public async Task<string> GetStringVerifierAsync(CancellationToken cancellationToken)
        {
            var codeVerifier = await distributedCache.GetStringAsync(CODE_VERIFY_KEY, cancellationToken);

            if (codeVerifier == null)
            {
                codeVerifier = Guid.NewGuid().ToString();

                var options = new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationTimeInMinutes)
                };
                await distributedCache.SetStringAsync(CODE_VERIFY_KEY, codeVerifier, options, cancellationToken);
            }
            return codeVerifier;
        }
    }
}
