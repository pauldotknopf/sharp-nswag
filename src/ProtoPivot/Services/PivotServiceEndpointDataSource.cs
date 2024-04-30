using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace ProtoPivot.Services;

internal class PivotServiceEndpointDataSource : EndpointDataSource
{
    public PivotServiceEndpointDataSource(PivotServiceDefinition serviceDefinition, string path)
    {
        var endpoints = new List<Endpoint>();
        
        foreach (var route in serviceDefinition.Routes)
        {
            var builder = new RouteEndpointBuilder(async context =>
                {
                    await context.RequestServices.GetRequiredService<IPivotResponseHandler>().HandleResponse(context, route);
                },
                RoutePatternFactory.Parse(Path.Combine(path, route.Route)), route.Order ?? 0);
            builder.Metadata.Add(new HttpMethodMetadata(new []{route.Verb}));
            builder.Metadata.Add(route.MethodInfo);
            endpoints.Add(builder.Build());
        }
        
        Endpoints = endpoints;
    }
    
    public override IChangeToken GetChangeToken()
    {
        return NullChangeToken.Singleton;
    }

    public override IReadOnlyList<Endpoint> Endpoints { get; }
}