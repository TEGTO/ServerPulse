namespace ApiGateway.Middlewares
{
    public class TokenFromQueryMiddleware
    {
        private readonly RequestDelegate next;

        public TokenFromQueryMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            if (request.Path.ToString().Contains("hub") &&
               request.Query.TryGetValue("access_token", out var accessToken))
            {
                request.Headers.Append("Authorization", $"Bearer {accessToken}");
            }
            await next(context);
        }
    }
}