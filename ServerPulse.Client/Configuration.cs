namespace ServerPulse.Client
{
    public class Configuration
    {
        public required string SlotKey { get; set; }
        public required string EventController { get; set; } = default!;
        public double AliveEventSendInterval { get; set; } = 5d;
        public double MessagePerInterval { get; set; } = 0.2d;
    }
}
