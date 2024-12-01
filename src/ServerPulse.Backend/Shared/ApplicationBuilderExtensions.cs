using ExceptionHandling;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Shared.Middlewares;

namespace Shared
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder builder)
        {
            builder.UseExceptionMiddleware();
            builder.UseSerilogRequestLogging();
            builder.UseMiddleware<AccessTokenMiddleware>();

            return builder;
        }
    }
}