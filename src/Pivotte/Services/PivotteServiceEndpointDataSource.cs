using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Pivotte.Services;

internal class PivotteServiceEndpointDataSource : EndpointDataSource
{
    
    
    public PivotteServiceEndpointDataSource(PivotteServiceDefinition serviceDefinition, string path, Action<RouteHandlerBuilder, PivotteRouteDefinition> builder = null)
    {
        var endpoints = new List<Endpoint>();

        foreach (var route in serviceDefinition.Routes)
        {
            RouteEndpointBuilder endpointBuilder = null;
            
            endpointBuilder = new RouteEndpointBuilder(async context =>
                {
                    var service = context.RequestServices.GetRequiredService(route.ServiceType);
        
                    var del = RequestDelegateFactory.Create(route.MethodInfo,
                        x => service,
                        new RequestDelegateFactoryOptions
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            EndpointBuilder = endpointBuilder,
                            ServiceProvider = context.RequestServices
                        });
                    
                    await del.RequestDelegate(context);
                },
                RoutePatternFactory.Parse(Path.Combine(path, route.Route)), route.Order ?? 0);
            endpointBuilder.Metadata.Add(new HttpMethodMetadata(new []{route.Verb}));
            endpointBuilder.Metadata.Add(route.MethodInfo);

            if (builder != null)
            {
                var conventionBuilder = new PivotteServiceConventionBuilder();
                var handler = new RouteHandlerBuilder(new IEndpointConventionBuilder[] { conventionBuilder });
                
                builder(handler, route);
                
                foreach (var e in conventionBuilder.Conventions)
                {
                    e(endpointBuilder);
                }
            }

            endpoints.Add(endpointBuilder.Build());
        }
        
        Endpoints = endpoints;
    }

    class PivotteServiceConventionBuilder : IEndpointConventionBuilder
    {
        public List<Action<EndpointBuilder>> Conventions { get; } = new();
        
        public void Add(Action<EndpointBuilder> convention)
        {
            Conventions.Add(convention);
        }
    }
    
    public override IChangeToken GetChangeToken()
    {
        return NullChangeToken.Singleton;
    }

    public override IReadOnlyList<Endpoint> Endpoints { get; }
}