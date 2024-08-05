namespace ApiGateway
{
    public class Configuration
    {
        public static string CONSUL_HOST { get; } = "Consul:Host";
        public static string CONSUL_SERVICE_NAME { get; } = "Consul:ServiceName";
        public static string CONSUL_SERVICE_PORT { get; } = "Consul:ServicePort";
        public static string ALLOWED_CORS_ORIGINS { get; } = "AllowedCORSOrigins";
    }
}
