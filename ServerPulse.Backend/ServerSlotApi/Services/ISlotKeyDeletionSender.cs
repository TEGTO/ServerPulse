namespace ServerSlotApi.Services
{
    public interface ISlotKeyDeletionSender
    {
        public Task SendDeleteSlotKeyEventAsync(string slotKey, CancellationToken cancellationToken);
    }
}