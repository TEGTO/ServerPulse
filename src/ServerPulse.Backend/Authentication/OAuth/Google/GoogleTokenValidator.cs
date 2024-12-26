using Google.Apis.Auth;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Authentication.OAuth.Google
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        public async Task<Payload> ValidateAsync(string idToken, ValidationSettings settings)
        {
            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
    }
}
