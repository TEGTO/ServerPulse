using Consul;
using ConsulUtils.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace ConsulUtils.Extension
{
    public class ServiceDiscoveryHostedService : IHostedService
    {
        private readonly IConsulClient client;
        private readonly ConsulSettings configuration;
        private readonly IHostApplicationLifetime appLifetime;
        private string registrationId = default!;

        public ServiceDiscoveryHostedService(IConsulClient client,
            ConsulSettings configuration, IHostApplicationLifetime appLifetime)
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