namespace AuthenticationApi.Infrastructure
{
    public static class ConfigurationKeys
    {
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string AUTH_DATABASE_CONNECTION_STRING { get; } = "AuthenticationDb";
    }
}
