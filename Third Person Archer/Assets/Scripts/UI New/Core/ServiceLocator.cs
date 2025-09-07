#nullable enable
using System;
using System.Collections.Generic;

namespace UI.Core
{
    /// <summary>Minimal, testable locator. Keep registrations at bootstrap.</summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T instance) where T : notnull => _services[typeof(T)] = instance!;
        public static void Unregister<T>() => _services.Remove(typeof(T));

        public static T Resolve<T>() where T : notnull
        {
            if (_services.TryGetValue(typeof(T), out var s)) return (T)s;
            throw new InvalidOperationException($"Service not found: {typeof(T).Name}");
        }

        public static bool TryResolve<T>(out T value) where T : notnull
        {
            if (_services.TryGetValue(typeof(T), out var s)) { value = (T)s; return true; }
            value = default!;
            return false;
        }
    }
}
