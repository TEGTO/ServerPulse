using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;

namespace Logging
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddLogging(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(new JsonFormatter(), "logs/applogs-.txt", rollingInterval: RollingInterval.Day);
            });

            return hostBuilder;
        }
    }
}
