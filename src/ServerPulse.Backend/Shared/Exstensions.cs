using Serilog;
using System.Text.Json;

namespace Shared
{
    public static class Extensions
    {
        private static readonly ILogger logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        public static bool TryToDeserialize<T>(this string? message, out T? obj, JsonSerializerOptions? options = default)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    obj = default;
                    return false;
                }

                obj = JsonSerializer.Deserialize<T>(message, options);

                if (object.Equals(obj, default(T)))
                {
                    return false;
                }

                return true;
            }
            catch (JsonException ex)
            {
                logger.Error(ex, "Deserialization failed");
                obj = default;
                return false;
            }
        }
    }
}
