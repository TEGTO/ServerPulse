using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServerSlotApi.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace ServerSlotApi.IntegrationTests
{
    public sealed class WebAppFactoryWrapper : IAsyncDisposable
    {
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
            DbContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17")
                .WithDatabase("serverslot-db")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

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
                { $"ConnectionStrings:{Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING}", DbContainer?.GetConnectionString() },
                { "SlotsPerUser", "5" },
                { "EFCreateDatabase", "true" },
                { "AuthSettings:Key", "q57+LXDr4HtynNQaYVs7t50HwzvTNrWM2E/OepoI/D4=" },
                { "AuthSettings:Issuer", "https://token.issuer.example.com" },
                { "AuthSettings:ExpiryInMinutes", "30" },
                { "AuthSettings:Audience", "https://api.example.com" },
                { "Cache__GetServerSlotByEmailExpiryInSeconds", "2" },
                { "Cache__ServerSlotCheckExpiryInSeconds", "2" },
            });

            return configurationBuilder.Build();
        }
    }
}