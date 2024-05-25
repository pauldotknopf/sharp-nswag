using Microsoft.Extensions.DependencyInjection;

namespace Pivot.Generators;

public static class Extensions
{
    public static IServiceCollection AddPivotGeneratorServices(this IServiceCollection services)
    {
        services.AddSingleton<IGenerator, Impl.Generator>();
        return services;
    }
}