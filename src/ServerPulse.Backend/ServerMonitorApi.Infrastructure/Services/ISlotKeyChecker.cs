namespace ServerMonitorApi.Infrastructure.Services
{
    public interface ISlotKeyChecker
    {
        public Task<bool> CheckSlotKeyAsync(string key, CancellationToken cancellationToken);
    }
}