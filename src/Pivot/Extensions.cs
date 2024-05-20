using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Services;
using Pivot.Services.Impl;

namespace Pivot;

public static class Extensions
{
    public static IServiceCollection AddPivotServices(this IServiceCollection services)
    {
        services.AddSingleton<IPivotServiceDefinitionBuilder, PivotServiceDefinitionBuilder>();
        services.AddSingleton<IPivotResponseHandler, PivotResponseHandler>();
        return services;
    }
    
    public static void MapPivotService<T>(this IEndpointRouteBuilder endpointsRouteBuilder, string path)
    {
        var serviceDefinition = endpointsRouteBuilder.ServiceProvider.GetRequiredService<IPivotServiceDefinitionBuilder>().BuildServiceDefinition<T>();
        endpointsRouteBuilder.DataSources.Add(new PivotServiceEndpointDataSource(serviceDefinition, path));
    }
}