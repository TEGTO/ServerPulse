using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CacheUtilsTests")]
namespace CacheUtils
{
    internal class Configuration
    {
        public static string REDIS_CONNECTION_STRING { get; } = "RedisServer";
    }
}
