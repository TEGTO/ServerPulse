namespace AuthenticationApi
{
    public class Configuration
    {
        public static string CONSUL_HOST { get; } = "Consul:Host";
        public static string CONSUL_SERVICE_NAME { get; } = "Consul:ServiceName";
        public static string CONSUL_SERVICE_PORT { get; } = "Consul:ServicePort";
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS { get; } = "AuthSettings:RefreshExpiryInDays";
        public static string AUTH_DATABASE_CONNECTION_STRING { get; } = "AuthenticationConnection";
    }
}