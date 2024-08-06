using Consul;
using ConsulUtils.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Winton.Extensions.Configuration.Consul;

namespace ConsulUtils.Extension
{
    public static class ConsulExtension
    {
        public static ConsulSettings GetConsulSettings(IConfiguration configuration)
        {
            var consulSettings = new ConsulSettings
            {
                Host = configuration[ConsulConfiguration.CONSUL_HOST]!,
                ServiceName = configuration[ConsulConfiguration.CONSUL_SERVICE_NAME]!,
                ServicePort = int.Parse(configuration[ConsulConfiguration.CONSUL_SERVICE_PORT]!)
            };
            return consulSettings;
        }
        public static IServiceCollection AddConsulService(this IServiceCollection services, ConsulSettings consulSettings)
        {
            services.AddSingleton(consulSettings);
            services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var host = consulSettings.Host;
                consulConfig.Address = new Uri(host);
            }));

            return services;
        }
        public static IConfigurationBuilder ConfigureConsul(this IConfigurationBuilder builder, ConsulSettings consulSettings, string environmentName)
        {
            string sharedKey = $"shared/appsettings.{environmentName}.json";
            builder.AddConsul(sharedKey, options =>
            {
                options.ConsulConfigurationOptions = cco => { cco.Address = new Uri(consulSettings.Host); };
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

            string key = $"{consulSettings.ServiceName}/appsettings.{environmentName}.json";
            builder.AddConsul(
                key,
                options =>
                {
                    options.ConsulConfigurationOptions = cco => { cco.Address = new Uri(consulSettings.Host); };
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
}