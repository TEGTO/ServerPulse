using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AuthenticationTests")]
namespace Authentication.OAuth
{
    internal static class OAuthConfiguration
    {
        public const string GOOGLE_OAUTH_CLIENT_ID = "AuthSettings:GoogleOAuth:ClientId";
        public const string GOOGLE_OAUTH_CLIENT_SECRET = "AuthSettings:GoogleOAuth:ClientSecret";
        public const string GOOGLE_OAUTH_SCOPE = "AuthSettings:GoogleOAuth:Scope";
    }
}
