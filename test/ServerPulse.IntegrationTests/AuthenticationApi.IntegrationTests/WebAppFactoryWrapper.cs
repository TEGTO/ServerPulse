using AuthenticationApi.Data;
using Consul;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;
using System.Text.Json;
using Testcontainers.Consul;
using Testcontainers.PostgreSql;

namespace AuthenticationApi.IntegrationTests
{
    public sealed class WebAppFactoryWrapper : IAsyncDisposable
    {
        private PostgreSqlContainer dbContainer;
        private ConsulContainer consulContainer;

        protected WebApplicationFactory<Program> WebApplicationFactory { get; private set; }

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
            if (WebApplicationFactory != null)
            {
                await dbContainer.StopAsync();
                await consulContainer.StopAsync();

                await dbContainer.DisposeAsync();
                await consulContainer.DisposeAsync();

                await WebApplicationFactory.DisposeAsync();
                WebApplicationFactory = null;
            }
        }

        private async Task InitializeContainersAsync()
        {
            dbContainer = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .WithDatabase("authentication-db")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            consulContainer = new ConsulBuilder()
                .WithImage("hashicorp/consul:latest")
                .Build();

            await dbContainer.StartAsync();
            await consulContainer.StartAsync();

            await PopulateConsulAsync();
        }
        private WebApplicationFactory<Program> InitializeFactory()
        {
            return new WebApplicationFactory<Program>()
              .WithWebHostBuilder(builder =>
              {
                  builder.UseConfiguration(GetConfiguration());

                  builder.ConfigureTestServices(services =>
                  {
                      services.RemoveAll(typeof(IDbContextFactory<AuthIdentityDbContext>));

                      services.AddDbContextFactory<AuthIdentityDbContext>(options =>
                          options.UseNpgsql(dbContainer.GetConnectionString()));
                  });
              });
        }
        private async Task PopulateConsulAsync()
        {
            var consulClient = new ConsulClient(config => config.Address = new Uri(consulContainer.GetBaseAddress()));

            var kvPairs = new Dictionary<string, object>
            {
                { "ConnectionStrings:AuthenticationConnection", dbContainer.GetConnectionString() },
                { "EFCreateDatabase", "true" },
                { "AuthSettings:Key", "q57+LXDr4HtynNQaYVs7t50HwzvTNrWM2E/OepoI/D4=" },
                { "AuthSettings:Issuer", "https://token.issuer.example.com" },
                { "AuthSettings:ExpiryInMinutes", "30" },
                { "AuthSettings:Audience", "https://api.example.com" },
                { "AuthSettings:RefreshExpiryInDays", "5" },
            };

            var developmentJson = JsonSerializer.Serialize(kvPairs);
            var productionJson = JsonSerializer.Serialize(kvPairs);

            await consulClient.KV.Put(new KVPair("authentication/appsettings.Development.json")
            {
                Value = Encoding.UTF8.GetBytes(developmentJson)
            });

            await consulClient.KV.Put(new KVPair("authentication/appsettings.Production.json")
            {
                Value = Encoding.UTF8.GetBytes(productionJson)
            });
        }
        private IConfigurationRoot GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var s = consulContainer.GetBaseAddress();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Consul:Host", consulContainer.GetBaseAddress() },
                { "Consul:ServiceName", "authentication" },
                { "Consul:ServicePort", "80" }
            });
            return configurationBuilder.Build();
        }
    }
}
