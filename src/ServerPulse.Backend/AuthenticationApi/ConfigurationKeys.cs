namespace AuthenticationApi
{
    public static class ConfigurationKeys
    {
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS { get; } = "AuthSettings:RefreshExpiryInDays";
        public static string AUTH_DATABASE_CONNECTION_STRING { get; } = "AuthenticationDb";
        public static string USE_USER_UNCONFIRMED_CLEANUP { get; } = "BackgroundServices:UseUserUnconfirmedCleanup";
        public static string UNCONRFIRMED_USERS_CLEANUP_IN_MINUTES { get; } = "BackgroundServices:UnconfirmedUsersCleanUpInMinutes";
    }
}