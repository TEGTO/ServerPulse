
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace ClientUseCase.Middlewares
{
    public class ResponseError
    {
        public string? StatusCode { get; set; }
        public string[]? Messages { get; set; }

        public override string ToString()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }
    }

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleware> logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors
                .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                .ToArray();
                await SetError(httpContext, HttpStatusCode.BadRequest, ex, errors);
            }
            catch (UnauthorizedAccessException ex)
            {
                await SetError(httpContext, HttpStatusCode.Unauthorized, ex, [ex.Message]);
            }
            catch (Exception ex)
            {
                await SetError(httpContext, HttpStatusCode.InternalServerError, ex, [ex.Message]);
            }
        }
        private async Task SetError(HttpContext httpContext, HttpStatusCode httpStatusCode, Exception ex, string[] messages)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)httpStatusCode;
            var responseError = new ResponseError
            {
                StatusCode = httpContext.Response.StatusCode.ToString(),
                Messages = messages
            };
            logger.LogError(ex, responseError.ToString());
            await httpContext.Response.WriteAsync(responseError.ToString());
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}