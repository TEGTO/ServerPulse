using Authentication.OAuth.GitHub;
using Authentication.OAuth.Google;
using Authentication.Token;
using AuthenticationApi.Application;
using AuthenticationApi.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using ApplicationKeys = AuthenticationApi.Application.ConfigurationKeys;
using InfrastructureKeys = AuthenticationApi.Infrastructure.ConfigurationKeys;

namespace AuthenticationApi.IntegrationTests
{
    internal sealed class WebAppFactoryWrapper : IAsyncDisposable
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
                .WithDatabase($"elibrary-db")
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
                      services.RemoveAll(typeof(IDbContextFactory<AuthIdentityDbContext>));

                      services.AddDbContextFactory<AuthIdentityDbContext>(options =>
                          options.UseNpgsql(DbContainer?.GetConnectionString()));
                  });
              });
        }

        private IConfigurationRoot GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();

            var configuration = new Dictionary<string, string?>
            {
                { $"ConnectionStrings:{InfrastructureKeys.AUTH_DATABASE_CONNECTION_STRING}", DbContainer?.GetConnectionString() },
                { $"ConnectionStrings:{InfrastructureKeys.REDIS_SERVER_CONNECTION_STRING}",  RedisContainer?.GetConnectionString() },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.PrivateKey)}", TestRsaKeys.PRIVATE_KEY },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.PublicKey)}", TestRsaKeys.PUBLIC_KEY },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.Audience)}", "https://api.example.com" },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.Issuer)}", "https://token.issuer.example.com" },
                { $"{GitHubOAuthSettings.SETTINGS_SECTION}:{nameof(GitHubOAuthSettings.GitHubOAuthApiUrl)}", "https://api.example.com" },
                { $"{GitHubOAuthSettings.SETTINGS_SECTION}:{nameof(GitHubOAuthSettings.GitHubApiUrl)}", "https://api.example.com" },
                { $"{GoogleOAuthSettings.SETTINGS_SECTION}:{nameof(GoogleOAuthSettings.GoogleOAuthUrl)}", "https://api.example.com" },
                { $"{GoogleOAuthSettings.SETTINGS_SECTION}:{nameof(GoogleOAuthSettings.GoogleOAuthTokenUrl)}", "https://api.example.com" },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.ExpiryInMinutes)}", "30" },
                { ApplicationKeys.AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS, "7" },
                { InfrastructureKeys.EF_CREATE_DATABASE, "true" },
                {$"FeatureManagement:{Features.EMAIL_CONFIRMATION}", "true" },
                {$"FeatureManagement:{Features.OAUTH}", "true" },
            };

            configurationBuilder.AddInMemoryCollection(configuration);

            return configurationBuilder.Build();
        }
    }
}
