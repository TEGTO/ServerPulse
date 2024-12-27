using Authentication.Token;
using AuthenticationApi.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace AuthenticationApi.IntegrationTests
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
                .WithDatabase("elibrary-db")
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
                      services.RemoveAll(typeof(IDbContextFactory<AuthIdentityDbContext>));

                      services.AddDbContextFactory<AuthIdentityDbContext>(options =>
                          options.UseNpgsql(DbContainer?.GetConnectionString()));
                  });
              });
        }

        private IConfigurationRoot GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { $"ConnectionStrings:{Configuration.AUTH_DATABASE_CONNECTION_STRING}", DbContainer?.GetConnectionString() },
                { JwtConfiguration.JWT_SETTINGS_PUBLIC_KEY, TestRsaKeys.PUBLIC_KEY },
                { JwtConfiguration.JWT_SETTINGS_PRIVATE_KEY, TestRsaKeys.PRIVATE_KEY },
                { JwtConfiguration.JWT_SETTINGS_ISSUER, "https://token.issuer.example.com" },
                { JwtConfiguration.JWT_SETTINGS_AUDIENCE, "https://api.example.com" },
                { JwtConfiguration.JWT_SETTINGS_EXPIRY_IN_MINUTES, "30" },
                { Configuration.AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS, "7" },
                { Configuration.EF_CREATE_DATABASE, "true" },
            });

            return configurationBuilder.Build();
        }
    }
}
