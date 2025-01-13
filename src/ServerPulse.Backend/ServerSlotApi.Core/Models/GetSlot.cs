namespace ServerSlotApi.Core.Models
{
    public class GetSlot
    {
        public required string UserEmail { get; set; }
        public required string SlotId { get; set; }
    }
}
