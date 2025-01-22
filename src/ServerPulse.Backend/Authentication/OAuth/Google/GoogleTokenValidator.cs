using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Authentication.OAuth.Google
{
    public sealed class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly GoogleOAuthSettings oAuthSettings;

        public GoogleTokenValidator(IOptions<GoogleOAuthSettings> options)
        {
            oAuthSettings = options.Value;
        }

        public async Task<Payload> ValidateAsync(string idToken)
        {
            return await GoogleJsonWebSignature.ValidateAsync(idToken, new ValidationSettings
            {
                Audience = [oAuthSettings.ClientId],
            });
        }
    }
}
