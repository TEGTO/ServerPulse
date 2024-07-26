
namespace ServerInteractionApi.Services
{
    public interface IRedisService
    {
        public Task<string> GetValueAsync(string key);
        public Task SetValueAsync(string key, string value, double expiryInMinutes);
        public Task<bool> RemoveKeyAsync(string key);
    }
}