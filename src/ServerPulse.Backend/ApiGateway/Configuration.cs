namespace ApiGateway
{
    public class Configuration
    {
        public static string CONSUL_HOST { get; } = "Consul:Host";
        public static string CONSUL_SERVICE_NAME { get; } = "Consul:ServiceName";
        public static string CONSUL_SERVICE_PORT { get; } = "Consul:ServicePort";
        public static string ALLOWED_CORS_ORIGINS { get; } = "AllowedCORSOrigins";
        public static string JWT_SETTINGS_KEY { get; } = "AuthSettings:Key";
        public static string JWT_SETTINGS_AUDIENCE { get; } = "AuthSettings:Audience";
        public static string JWT_SETTINGS_ISSUER { get; } = "AuthSettings:Issuer";
        public static string JWT_SETTINGS_EXPIRY_IN_MINUTES { get; } = "AuthSettings:ExpiryInMinutes";

    }
}
