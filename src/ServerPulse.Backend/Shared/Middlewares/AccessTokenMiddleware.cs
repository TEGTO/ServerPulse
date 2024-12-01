using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Shared.Middlewares
{
    public class AccessTokenMiddleware
    {
        private readonly RequestDelegate next;

        public AccessTokenMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            httpContext.Items["AccessToken"] = accessToken;
            await next(httpContext);
        }
    }
}
