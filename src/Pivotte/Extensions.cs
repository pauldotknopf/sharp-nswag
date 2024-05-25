using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Pivotte.Services;
using Pivotte.Services.Impl;

namespace Pivotte;

public static class Extensions
{
    public static IServiceCollection AddPivotteServices(this IServiceCollection services)
    {
        services.AddSingleton<IPivotteServiceDefinitionBuilder, PivotteServiceDefinitionBuilder>();
        services.AddSingleton<IPivotteResponseHandler, PivotteResponseHandler>();
        return services;
    }
    
    public static void MapPivotteService<T>(this IEndpointRouteBuilder endpointsRouteBuilder, string path, Action<RouteHandlerBuilder, PivotteRouteDefinition> builder = null)
    {
        var serviceDefinition = endpointsRouteBuilder.ServiceProvider.GetRequiredService<IPivotteServiceDefinitionBuilder>().BuildServiceDefinition<T>();
        endpointsRouteBuilder.DataSources.Add(new PivotteServiceEndpointDataSource(serviceDefinition, path, builder));
    }
}