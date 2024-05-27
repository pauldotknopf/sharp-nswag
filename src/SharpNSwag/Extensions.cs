using Microsoft.Extensions.DependencyInjection;
using Pivotte.Impl;

namespace Pivotte;

public static class Extensions
{
    public static IServiceCollection AddSharpNSwagServices(this IServiceCollection services)
    {
        services.AddSingleton<ISharpNSwagBuilder, SharpNSwagBuilder>();
        services.AddSingleton<ISharpNSwagGenerator, SharpNSwagGenerator>();
        return services;
    }
}