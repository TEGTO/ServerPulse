using ExceptionHandling;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Shared
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder builder)
        {
            builder.UseExceptionMiddleware();
            builder.UseSerilogRequestLogging();

            return builder;
        }
    }
}