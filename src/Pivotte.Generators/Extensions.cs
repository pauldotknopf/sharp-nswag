using Microsoft.Extensions.DependencyInjection;
using Pivotte.Generators.Impl;

namespace Pivotte.Generators;

public static class Extensions
{
    public static IServiceCollection AddPivotGeneratorServices(this IServiceCollection services)
    {
        services.AddSingleton<IGenerator, Generator>();
        return services;
    }
}