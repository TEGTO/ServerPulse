namespace ServerSlotApi.Dtos
{
    public class CheckSlotKeyResponse
    {
        public string SlotKey { get; set; } = string.Empty;
        public bool IsExisting { get; set; }
    }
}