namespace AuthenticationApi
{
    public static class ConfigurationKeys
    {
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS { get; } = "AuthSettings:RefreshExpiryInDays";
        public static string AUTH_DATABASE_CONNECTION_STRING { get; } = "AuthenticationDb";
        public static string REQUIRE_EMAIL_CONFIRMATION { get; } = "RequireEmailConfirmation";
        public static string DELETE_UNCONRFIRMED_USERS_AFTER_MINUTES { get; } = "DeleteUnconfirmedUserAfterMinutes";
    }
}