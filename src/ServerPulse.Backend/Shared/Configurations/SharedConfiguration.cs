namespace Shared.Configurations
{
    public static class SharedConfiguration
    {
        public static string ALLOWED_CORS_ORIGINS { get; } = "AllowedCORSOrigins";
        public static string HTTP_CLIENT_RESILIENCE_PIPELINE { get; } = "HttpClientResiliencePipeline";
    }
}