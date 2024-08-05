namespace ServerSlotApi
{
    public class Configuration
    {
        public static string CONSUL_HOST { get; } = "Consul:Host";
        public static string CONSUL_SERVICE_NAME { get; } = "Consul:ServiceName";
        public static string CONSUL_SERVICE_PORT { get; } = "Consul:ServicePort";
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string SERVER_SLOT_DATABASE_CONNECTION_STRING { get; } = "ServerSlotConnection";
        public static string SERVER_SLOTS_PER_USER { get; } = "SlotsPerUser";
    }
}
