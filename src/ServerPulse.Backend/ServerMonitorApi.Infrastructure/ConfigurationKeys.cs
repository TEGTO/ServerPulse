namespace ServerMonitorApi.Infrastructure
{
    public static class ConfigurationKeys
    {
        public static string SERVER_SLOT_URL { get; } = "ServerSlotApi:Url";
        public static string SERVER_SLOT_ALIVE_CHECKER { get; } = "ServerSlotApi:Check";
    }
}