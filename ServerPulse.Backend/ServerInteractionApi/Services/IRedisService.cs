
namespace ServerInteractionApi.Services
{
    public interface IRedisService
    {
        public Task<string> GetValueAsync(string key);
        public Task SetValueAsync(string key, string value);
        public Task<bool> RemoveKeyAsync(string key);
    }
}