namespace ServerSlotApi.Infrastructure.Configuration
{
    public static class ConfigurationKeys
    {
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string SERVER_SLOT_DATABASE_CONNECTION_STRING { get; } = "ServerSlotDb";
        public static string SERVER_SLOTS_PER_USER { get; } = "SlotsPerUser";
    }
}
