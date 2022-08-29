using System;
using Splat;

namespace MemeManager.DependencyInjection;

/// <summary>
/// Copied from https://github.com/IngvarX/Camelot/blob/master/src/Camelot/DependencyInjection/ReadonlyDependencyResolverExtensions.cs
/// </summary>
public static class ReadonlyDependencyResolverExtensions
{
    public static TService GetRequiredService<TService>(this IReadonlyDependencyResolver resolver)
    {
        var service = resolver.GetService<TService>();
        if (service is null)
        {
            throw new InvalidOperationException($"Failed to resolve object of type {typeof(TService)}");
        }

        return service;
    }

    public static object GetRequiredService(this IReadonlyDependencyResolver resolver, Type type)
    {
        var service = resolver.GetService(type);
        if (service is null)
        {
            throw new InvalidOperationException($"Failed to resolve object of type {type}");
        }

        return service;
    }
}
