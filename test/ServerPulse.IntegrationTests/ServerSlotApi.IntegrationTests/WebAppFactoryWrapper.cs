﻿using Consul;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ServerSlotApi.Data;
using ServerSlotApi.Services;
using System.Text;
using System.Text.Json;
using Testcontainers.Consul;
using Testcontainers.PostgreSql;

namespace ServerSlotApi.IntegrationTests
{
    public class WebAppFactoryWrapper : IAsyncDisposable
    {
        private PostgreSqlContainer dbContainer;
        private ConsulContainer consulContainer;

        public WebApplicationFactory<Program> WebApplicationFactory { get; private set; }

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
            }
            GC.SuppressFinalize(this);
        }

        private async Task InitializeContainersAsync()
        {
            dbContainer = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .WithDatabase("serverslot-db")
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
                      services.RemoveAll(typeof(IDbContextFactory<ServerDataDbContext>));

                      services.AddDbContextFactory<ServerDataDbContext>(options =>
                          options.UseNpgsql(dbContainer.GetConnectionString()));

                      services.RemoveAll(typeof(ISlotStatisticsService));

                      var mock = new Mock<ISlotStatisticsService>();
                      mock.Setup(x => x.DeleteSlotStatisticsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

                      services.AddSingleton<ISlotStatisticsService>(mock.Object);
                  });
              });
        }
        private async Task PopulateConsulAsync()
        {
            var consulClient = new ConsulClient(config => config.Address = new Uri(consulContainer.GetBaseAddress()));

            var kvPairs = new Dictionary<string, object>
            {
                { "ConnectionStrings:ServerSlotConnection", dbContainer.GetConnectionString() },
                { "SlotsPerUser", 5 },
                { "EFCreateDatabase", "true" },
                { "AuthSettings:Key", "q57+LXDr4HtynNQaYVs7t50HwzvTNrWM2E/OepoI/D4=" },
                { "AuthSettings:Issuer", "https://token.issuer.example.com" },
                { "AuthSettings:ExpiryInMinutes", "30" },
                { "AuthSettings:Audience", "https://api.example.com" },
            };

            var developmentJson = JsonSerializer.Serialize(kvPairs);
            var productionJson = JsonSerializer.Serialize(kvPairs);

            await consulClient.KV.Put(new KVPair("server-slot/appsettings.Development.json")
            {
                Value = Encoding.UTF8.GetBytes(developmentJson)
            });

            await consulClient.KV.Put(new KVPair("server-slot/appsettings.Production.json")
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
                { "Consul:ServiceName", "server-slot" },
                { "Consul:ServicePort", "80" }
            });
            return configurationBuilder.Build();
        }
    }
}