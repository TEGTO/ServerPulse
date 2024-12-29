using Authentication.Token;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServerSlotApi.Infrastructure.Configuration;
using ServerSlotApi.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ServerSlotApi.IntegrationTests
{
    public sealed class WebAppFactoryWrapper : IAsyncDisposable
    {
        private RedisContainer? RedisContainer { get; set; }
        private PostgreSqlContainer? DbContainer { get; set; }
        private WebApplicationFactory<Program>? WebApplicationFactory { get; set; }

        public async Task<WebApplicationFactory<Program>> GetFactoryAsync()
        {
            if (WebApplicationFactory == null)
            {
                await InitializeContainersAsync();
                WebApplicationFactory = InitializeFactory();
            }
            return WebApplicationFactory;
        }

        public async ValueTask DisposeAsync()
        {
            if (RedisContainer != null)
            {
                await RedisContainer.StopAsync();
                await RedisContainer.DisposeAsync();
            }

            if (DbContainer != null)
            {
                await DbContainer.StopAsync();
                await DbContainer.DisposeAsync();
            }

            if (WebApplicationFactory != null)
            {
                await WebApplicationFactory.DisposeAsync();
                WebApplicationFactory = null;
            }
        }

        private async Task InitializeContainersAsync()
        {
            RedisContainer = new RedisBuilder()
                .WithImage("redis:7.4")
                .Build();

            DbContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17")
                .WithDatabase("serverslot-db")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await RedisContainer.StartAsync();
            await DbContainer.StartAsync();
        }

        private WebApplicationFactory<Program> InitializeFactory()
        {
            return new WebApplicationFactory<Program>()
              .WithWebHostBuilder(builder =>
              {
                  builder.UseConfiguration(GetConfiguration());

                  builder.ConfigureTestServices(services =>
                  {
                      services.RemoveAll(typeof(IDbContextFactory<ServerSlotDbContext>));

                      services.AddDbContextFactory<ServerSlotDbContext>(options =>
                          options.UseNpgsql(DbContainer?.GetConnectionString()));
                  });
              });
        }

        private IConfigurationRoot GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { $"ConnectionStrings:{CacheSettings.REDIS_SERVER_CONNECTION_STRING}",  RedisContainer?.GetConnectionString()},
                { $"ConnectionStrings:{ConfigurationKeys.SERVER_SLOT_DATABASE_CONNECTION_STRING}", DbContainer?.GetConnectionString() },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.PublicKey)}", TestRsaKeys.PUBLIC_KEY },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.PrivateKey)}", TestRsaKeys.PRIVATE_KEY },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.Audience)}", "https://api.example.com" },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.Issuer)}", "https://token.issuer.example.com" },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.ExpiryInMinutes)}", "30" },
                { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetServerSlotByEmailExpiryInSeconds)}", "2" },
                { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.ServerSlotCheckExpiryInSeconds)}", "2" },
                { ConfigurationKeys.EF_CREATE_DATABASE, "true" },
                { ConfigurationKeys.SERVER_SLOTS_PER_USER, "5" },
            });

            return configurationBuilder.Build();
        }
    }
}