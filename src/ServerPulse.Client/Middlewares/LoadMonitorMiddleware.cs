using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ServerPulse.Client.Services.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client.Middlewares
{
    public class LoadMonitorMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IQueueMessageSender<LoadEvent> serverLoadSender;
        private readonly string key;

        public LoadMonitorMiddleware(RequestDelegate next, IQueueMessageSender<LoadEvent> serverLoadSender, EventSendingSettings configuration)
        {
            this.serverLoadSender = serverLoadSender;
            this.next = next;
            key = configuration.Key;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var startTime = DateTime.UtcNow;

            await next(httpContext);

            var endTime = DateTime.UtcNow;

            var loadEvent = new LoadEvent
            (
                Key: key,
                Endpoint: httpContext.Request.Path,
                Method: httpContext.Request.Method,
                StatusCode: httpContext.Response.StatusCode,
                Duration: endTime - startTime,
                TimestampUTC: startTime
           );

            serverLoadSender.SendEvent(loadEvent);
        }
    }
    public static class LoadMonitorMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoadMonitor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoadMonitorMiddleware>();
        }
    }
}