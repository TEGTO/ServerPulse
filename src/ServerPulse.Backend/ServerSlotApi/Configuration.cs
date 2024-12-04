namespace ServerSlotApi
{
    public static class Configuration
    {
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string SERVER_SLOT_DATABASE_CONNECTION_STRING { get; } = "ServerSlotDb";
        public static string SERVER_SLOTS_PER_USER { get; } = "SlotsPerUser";
        public static string API_GATEWAY { get; } = "ApiGateway";
        public static string STATISTICS_DELETE_URL { get; } = "ServerMonitorApi:DeleteStatistics";
        public static string CACHE_GET_BY_EMAIL_SERVER_SLOT_EXPIRY_IN_SECONDS { get; } = "Cache:GetServerSlotByEmailExpiryInSeconds";
        public static string CACHE_CHECK_SERVER_SLOT_EXPIRY_IN_SECONDS { get; } = "Cache:ServerSlotCheckExpiryInSeconds";

    }
}
