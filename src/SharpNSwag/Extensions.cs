using Microsoft.Extensions.DependencyInjection;
using SharpNSwag.Impl;

namespace SharpNSwag;

public static class Extensions
{
    public static IServiceCollection AddSharpNSwagServices(this IServiceCollection services)
    {
        services.AddSingleton<ISharpNSwagBuilder, SharpNSwagBuilder>();
        services.AddSingleton<ISharpNSwagGenerator, SharpNSwagGenerator>();
        return services;
    }
}