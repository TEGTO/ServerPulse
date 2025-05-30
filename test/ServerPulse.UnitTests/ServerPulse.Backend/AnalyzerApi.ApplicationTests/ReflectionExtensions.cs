﻿using System.Reflection;

namespace AnalyzerApi.ApplicationTests
{
    internal static class ReflectionExtensions
    {
        public static T? GetFieldValue<T>(this object obj, string name)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T?)field?.GetValue(obj);
        }
    }
}
