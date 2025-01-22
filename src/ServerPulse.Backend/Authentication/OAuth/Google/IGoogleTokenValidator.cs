using Google.Apis.Auth;

namespace Authentication.OAuth.Google
{
    public interface IGoogleTokenValidator
    {
        public Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken);
    }
}