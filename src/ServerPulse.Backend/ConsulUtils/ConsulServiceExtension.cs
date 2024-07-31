using Consul;
using ConsulUtils.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using Winton.Extensions.Configuration.Consul;

namespace ConsulUtils.Extension
{
    public static class ConsulServiceExtension
    {
        public static IServiceCollection AddConsulService(this IServiceCollection services, ConsulConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var host = configuration.Host;
                consulConfig.Address = new Uri(host);
            }));

            return services;
        }
        public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, ConsulConfiguration configuration, string environmentName)
        {
            string sharedKey = $"shared/appsettings.{environmentName}.json";
            builder.AddConsul(sharedKey, options =>
            {
                options.ConsulConfigurationOptions = cco => { cco.Address = new Uri(configuration.Host); };
                options.Optional = true;
                options.PollWaitTime = TimeSpan.FromSeconds(5);
                options.ReloadOnChange = true;
                options.OnLoadException = (consulLoadExceptionContext) =>
                {
                    Console.WriteLine($"Error onLoadException {consulLoadExceptionContext.Exception.Message} and stacktrace {consulLoadExceptionContext.Exception.StackTrace}");
                    throw consulLoadExceptionContext.Exception;
                };
                options.OnWatchException = (consulWatchExceptionContext) =>
                {
                    Console.WriteLine($"Unable to watchChanges in Consul due to {consulWatchExceptionContext.Exception.Message}");
                    return TimeSpan.FromSeconds(2);
                };
            });

            string key = $"{configuration.ServiceName}/appsettings.{environmentName}.json";
            builder.AddConsul(
                key,
                options =>
                {
                    options.ConsulConfigurationOptions = cco => { cco.Address = new Uri(configuration.Host); };
                    options.Optional = true;
                    options.PollWaitTime = TimeSpan.FromSeconds(5);
                    options.ReloadOnChange = true;
                    options.OnLoadException = (consulLoadExceptionContext) =>
                    {
                        Console.WriteLine($"Error onLoadException {consulLoadExceptionContext.Exception.Message} and stacktrace {consulLoadExceptionContext.Exception.StackTrace}");
                        throw consulLoadExceptionContext.Exception;
                    };
                    options.OnWatchException = (consulWatchExceptionContext) =>
                    {
                        Console.WriteLine($"Unable to watchChanges in Consul due to {consulWatchExceptionContext.Exception.Message}");
                        return TimeSpan.FromSeconds(2);
                    };
                });
            return builder;
        }
    }

    public class ServiceDiscoveryHostedService : IHostedService
    {
        private readonly IConsulClient client;
        private readonly ConsulConfiguration configuration;
        private readonly IHostApplicationLifetime appLifetime;
        private string registrationId = default!;

        public ServiceDiscoveryHostedService(IConsulClient client,
            ConsulConfiguration configuration, IHostApplicationLifetime appLifetime)
        {
            this.client = client;
            this.configuration = configuration;
            this.appLifetime = appLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var servicePort = configuration.ServicePort;
            var serviceIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            var serviceName = configuration.ServiceName;
            registrationId = $"{serviceName}-{serviceIp.ToString()}:{servicePort}";

            var registration = new AgentServiceRegistration
            {
                ID = registrationId,
                Name = serviceName,
                Address = serviceIp.ToString(),
                Port = servicePort,
                Check = new AgentCheckRegistration()
                {
                    HTTP = $"http://{serviceIp}:{servicePort}/health",
                    Interval = TimeSpan.FromSeconds(10),
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                }
            };

            await client.Agent.ServiceDeregister(registration.ID, cancellationToken);
            await client.Agent.ServiceRegister(registration, cancellationToken);
            appLifetime.ApplicationStopping.Register(OnStopping);
        }

        private void OnStopping()
        {
            client.Agent.ServiceDeregister(registrationId).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await client.Agent.ServiceDeregister(registrationId, cancellationToken);
        }
    }
}
