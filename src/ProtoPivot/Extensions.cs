using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using ProtoPivot.Services;

namespace ProtoPivot;

public static class Extensions
{
    public static IServiceCollection AddProtoPivotServices(this IServiceCollection services)
    {
        services.AddSingleton<IPivotServiceDefinitionBuilder, Impl.PivotServiceDefinitionBuilder>();
        services.AddSingleton<IPivotResponseHandler, Impl.PivotResponseHandler>();
        return services;
    }
    
    public static void MapProtoPivotService<T>(this IEndpointRouteBuilder endpointsRouteBuilder, string path)
    {
        var serviceDefinition = endpointsRouteBuilder.ServiceProvider.GetRequiredService<IPivotServiceDefinitionBuilder>().BuildServiceDefinition<T>();
        endpointsRouteBuilder.DataSources.Add(new PivotServiceEndpointDataSource(serviceDefinition, path));
    }
}