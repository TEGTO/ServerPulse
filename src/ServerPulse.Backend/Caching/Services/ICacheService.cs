namespace Caching.Services
{
    public interface ICacheService
    {
        public ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken);
        public ValueTask<bool> SetAsync(string key, string value, TimeSpan duration, CancellationToken cancellationToken);
        public ValueTask<bool> RemoveKeyAsync(string key, CancellationToken cancellationToken);
    }
}
