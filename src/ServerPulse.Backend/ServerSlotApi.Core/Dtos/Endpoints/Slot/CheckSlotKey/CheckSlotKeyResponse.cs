namespace ServerSlotApi.Core.Dtos.Endpoints.Slot.CheckSlotKey
{
    public class CheckSlotKeyResponse
    {
        public string SlotKey { get; set; } = string.Empty;
        public bool IsExisting { get; set; }
    }
}
