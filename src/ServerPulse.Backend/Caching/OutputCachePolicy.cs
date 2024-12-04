using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.Json;

namespace Caching
{
    public sealed class OutputCachePolicy : IOutputCachePolicy
    {
        private readonly TimeSpan? duration;
        private readonly bool useAuthenticationId;
        private readonly string[] propertyNamesToCacheBy;

        public OutputCachePolicy(TimeSpan? duration = null, bool useAuthenticationId = false, params string[] propertyNamesToCacheBy)
        {
            this.duration = duration;
            this.useAuthenticationId = useAuthenticationId;
            this.propertyNamesToCacheBy = propertyNamesToCacheBy;
        }

        #region IOutputCachePolicy Members

        public ValueTask CacheRequestAsync(
            OutputCacheContext context,
            CancellationToken cancellation)
        {
            var attemptOutputCaching = AttemptOutputCaching(context);
            context.EnableOutputCaching = true;
            context.AllowCacheLookup = attemptOutputCaching;
            context.AllowCacheStorage = attemptOutputCaching;
            context.AllowLocking = true;

            context.CacheVaryByRules.QueryKeys = "*";

            if (context.HttpContext.Request.HasJsonContentType())
            {
                SetCacheVaryByValuesFromRequestBody(context);
            }

            if (useAuthenticationId)
            {
                var id = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (id != null)
                {
                    context.CacheVaryByRules.CacheKeyPrefix += id;
                }
            }

            if (duration != null)
            {
                context.ResponseExpirationTimeSpan = duration;
            }


            return ValueTask.CompletedTask;
        }
        public ValueTask ServeFromCacheAsync
            (OutputCacheContext context, CancellationToken cancellation)
        {
            return ValueTask.CompletedTask;
        }
        public ValueTask ServeResponseAsync
            (OutputCacheContext context, CancellationToken cancellation)
        {
            var response = context.HttpContext.Response;

            if (response.Headers != null && !StringValues.IsNullOrEmpty(response.Headers.SetCookie))
            {
                context.AllowCacheStorage = false;
                return ValueTask.CompletedTask;
            }

            if (response.StatusCode != StatusCodes.Status200OK &&
                response.StatusCode != StatusCodes.Status301MovedPermanently)
            {
                context.AllowCacheStorage = false;
                return ValueTask.CompletedTask;
            }

            return ValueTask.CompletedTask;
        }

        #endregion

        #region Private Helpers

        private static bool AttemptOutputCaching(OutputCacheContext context)
        {
            var request = context.HttpContext.Request;

            if (!HttpMethods.IsGet(request.Method) &&
                !HttpMethods.IsHead(request.Method) &&
                !HttpMethods.IsPut(request.Method) &&
                !HttpMethods.IsPost(request.Method))
            {
                return false;
            }

            return true;
        }
        private void SetCacheVaryByValuesFromRequestBody(OutputCacheContext context)
        {
            context.HttpContext.Request.EnableBuffering();

            using (var reader = new StreamReader(context.HttpContext.Request.Body, leaveOpen: true))
            {
                var requestBody = reader.ReadToEndAsync().Result;

                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    var options = new JsonSerializerOptions();
                    options.PropertyNameCaseInsensitive = false;

                    var requestObject = JsonSerializer.Deserialize<JsonElement>(requestBody, options);

                    foreach (var propName in propertyNamesToCacheBy)
                    {
                        var property = requestObject.EnumerateObject()
                               .FirstOrDefault(p => string.Equals(p.Name, propName, StringComparison.OrdinalIgnoreCase));

                        if (property.Value.ValueKind != JsonValueKind.Undefined)
                        {
                            context.CacheVaryByRules.VaryByValues.Add(propName, property.Value.ToString());
                        }
                    }
                }

                context.HttpContext.Request.Body.Position = 0;
            }
        }

        #endregion
    }
}
