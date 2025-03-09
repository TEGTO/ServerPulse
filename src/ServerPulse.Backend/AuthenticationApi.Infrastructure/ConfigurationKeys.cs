namespace AuthenticationApi.Infrastructure
{
    public static class ConfigurationKeys
    {
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string AUTH_DATABASE_CONNECTION_STRING { get; } = "AuthenticationDb";
        public static string REDIS_SERVER_CONNECTION_STRING { get; } = "RedisServer";
        public static string CODE_VERIFY_EXPIRATION_TIME_IN_MINUTES { get; } = "CodeVerifyExpirationTimeInMinutes";
        public static string EMAIL_SERVICE_TYPE { get; } = "EmailServiceType";
    }
}
