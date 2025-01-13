using Caching;
using DatabaseControl;
using Microsoft.EntityFrameworkCore;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.CheckSlotKey;
using ServerSlotApi.Infrastructure;
using ServerSlotApi.Infrastructure.Configuration;
using ServerSlotApi.Infrastructure.Data;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi
{
    public static class HostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddDbContextFactory<ServerSlotDbContext>(
                builder.Configuration.GetConnectionString(ConfigurationKeys.SERVER_SLOT_DATABASE_CONNECTION_STRING)!,
                "ServerSlotApi"
            );

            #region Options

            var cacheSettings = builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION).Get<CacheSettings>();

            ArgumentNullException.ThrowIfNull(cacheSettings);

            builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION));

            #endregion

            builder.Services.AddSingleton<IServerSlotRepository, ServerSlotRepository>();

            #region Caching

            builder.Services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString(CacheSettings.REDIS_SERVER_CONNECTION_STRING);
            });

            builder.Services.AddOutputCache((options) =>
            {
                options.AddPolicy("BasePolicy", new OutputCachePolicy());

                var expiryTime = cacheSettings.GetServerSlotByEmailExpiryInSeconds;

                options.SetOutputCachePolicy("GetSlotsByEmailPolicy", duration: TimeSpan.FromSeconds(expiryTime), useAuthId: true);

                expiryTime = cacheSettings.ServerSlotCheckExpiryInSeconds;

                options.SetOutputCachePolicy("CheckSlotKeyPolicy", duration: TimeSpan.FromSeconds(expiryTime), types: typeof(CheckSlotKeyRequest));
            });

            #endregion

            builder.Services.AddRepositoryWithResilience<ServerSlotDbContext>(builder.Configuration);

            return builder;
        }
    }
}
