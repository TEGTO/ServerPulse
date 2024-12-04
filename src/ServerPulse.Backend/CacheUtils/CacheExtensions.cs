using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerMonitorApi.Services;
using StackExchange.Redis;

namespace CacheUtils
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(
                  ConnectionMultiplexer.Connect(configuration.GetConnectionString(Configuration.REDIS_CONNECTION_STRING)!));

            services.AddSingleton<ICacheService, RedisService>();

            return services;
        }
    }
}
