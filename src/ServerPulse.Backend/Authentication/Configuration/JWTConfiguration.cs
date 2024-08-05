namespace Authentication.Configuration
{
    public class JWTConfiguration
    {
        public static string JWT_SETTINGS_KEY { get; } = "AuthSettings:Key";
        public static string JWT_SETTINGS_AUDIENCE { get; } = "AuthSettings:Audience";
        public static string JWT_SETTINGS_ISSUER { get; } = "AuthSettings:Issuer";
        public static string JWT_SETTINGS_EXPIRY_IN_MINUTES { get; } = "AuthSettings:ExpiryInMinutes";
    }
}
