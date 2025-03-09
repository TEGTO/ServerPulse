using Microsoft.AspNetCore.OutputCaching;
using System.Reflection;

namespace Caching
{
    public static class CacheExtensions
    {
        public static void SetOutputCachePolicy(this OutputCacheOptions options,
            string name, TimeSpan? duration = null, bool useAuthId = false, params Type[] types)
        {
            if (types != null && types.Any())
            {
                var properties = new List<PropertyInfo>();

                foreach (var type in types)
                {
                    properties.AddRange(type.GetProperties());
                }

                options.AddPolicy(name,
                    new OutputCachePolicy(
                        duration,
                        useAuthId,
                        properties.Select(x => x.Name).ToArray()
                        )
                    );
            }
            else
            {
                options.AddPolicy(name,
                    new OutputCachePolicy(
                        duration,
                        useAuthId
                        )
                    );
            }
        }
    }
}
