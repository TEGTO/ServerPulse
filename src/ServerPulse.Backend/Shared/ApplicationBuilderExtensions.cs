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

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder builder, string title)
        {
            builder.UseSwagger();
            builder.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", title);
            });

            return builder;
        }
    }
}