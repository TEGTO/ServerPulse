using EntityFramework.Exceptions.Common;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace ExceptionHandling
{
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
            catch (SecurityTokenMalformedException ex)
            {
                await SetError(httpContext, HttpStatusCode.Conflict, ex, ["Access Token Exception"]);
            }
            catch (InvalidDataException ex)
            {
                await SetError(httpContext, HttpStatusCode.BadRequest, ex, new[] { ex.Message });
            }
            catch (ArgumentException ex)
            {
                await SetError(httpContext, HttpStatusCode.BadRequest, ex, new[] { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                await SetError(httpContext, HttpStatusCode.Conflict, ex, [ex.Message]);
            }
            catch (AuthorizationException ex)
            {
                var errors = ex.Errors.ToArray();
                await SetError(httpContext, HttpStatusCode.Conflict, ex, errors);
            }
            catch (UniqueConstraintException ex)
            {
                await SetError(httpContext, HttpStatusCode.Conflict, ex, [$"{ex.Message}: '{ex.Entries.FirstOrDefault()?.Entity.GetType().Name ?? ""}'"]);
            }
            catch (UnauthorizedAccessException ex)
            {
                await SetError(httpContext, HttpStatusCode.Unauthorized, ex, [ex.Message]);
            }
            catch (Exception ex)
            {
                await SetError(httpContext, HttpStatusCode.InternalServerError, ex, ["Internal server error occurred."]);
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