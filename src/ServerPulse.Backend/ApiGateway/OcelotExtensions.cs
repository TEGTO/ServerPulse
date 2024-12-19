using Ocelot.Authentication.Middleware;
using Ocelot.Authorization.Middleware;
using Ocelot.Claims.Middleware;
using Ocelot.DownstreamPathManipulation.Middleware;
using Ocelot.DownstreamRouteFinder.Middleware;
using Ocelot.DownstreamUrlCreator.Middleware;
using Ocelot.Errors.Middleware;
using Ocelot.Headers.Middleware;
using Ocelot.LoadBalancer.Middleware;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using Ocelot.QueryStrings.Middleware;
using Ocelot.Request.Middleware;
using Ocelot.Responder.Middleware;
using Ocelot.WebSockets;

namespace ApiGateway
{
    public static class OcelotExtensions
    {
        public static IApplicationBuilder UseOcelotWebSockets(this IApplicationBuilder appBuilder)
        {
            appBuilder.UseWebSockets();

            appBuilder.MapWhen(httpContext => httpContext.WebSockets.IsWebSocketRequest,
                wenSocketsApp =>
                {
                    wenSocketsApp.UseDownstreamContextMiddleware();
                    wenSocketsApp.UseExceptionHandlerMiddleware();
                    wenSocketsApp.UseResponderMiddleware();
                    wenSocketsApp.UseDownstreamRouteFinderMiddleware();
                    wenSocketsApp.UseMultiplexingMiddleware();
                    wenSocketsApp.UseHttpHeadersTransformationMiddleware();
                    wenSocketsApp.UseDownstreamRequestInitialiser();
                    wenSocketsApp.UseAuthenticationMiddleware();
                    wenSocketsApp.UseClaimsToClaimsMiddleware();
                    wenSocketsApp.UseAuthorizationMiddleware();
                    wenSocketsApp.UseClaimsToHeadersMiddleware();
                    wenSocketsApp.UseClaimsToQueryStringMiddleware();
                    wenSocketsApp.UseClaimsToDownstreamPathMiddleware();
                    wenSocketsApp.UseLoadBalancingMiddleware();
                    wenSocketsApp.UseDownstreamUrlCreatorMiddleware();
                    wenSocketsApp.UseWebSocketsProxyMiddleware();
                });
            return appBuilder;
        }
    }
}