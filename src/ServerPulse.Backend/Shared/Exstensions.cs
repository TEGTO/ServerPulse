using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Shared
{
    public static class Extensions
    {
        private static readonly ILogger logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Extensions");

        public static bool TryToDeserialize<T>(this string? message, out T? obj)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    obj = default;
                    return false;
                }

                obj = JsonSerializer.Deserialize<T>(message);

                if (obj == null)
                {
                    return false;
                }

                return true;
            }
            catch (JsonException ex)
            {
                logger.LogInformation("Deserialization failed: " + ex.Message);
                obj = default;
                return false;
            }
        }
    }
}
