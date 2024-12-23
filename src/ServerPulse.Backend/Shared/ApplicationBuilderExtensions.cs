﻿using ExceptionHandling;
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