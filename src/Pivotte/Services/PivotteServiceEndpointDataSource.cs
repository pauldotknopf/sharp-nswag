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
    public PivotteServiceEndpointDataSource(IServiceProvider applicationServices, PivotteServiceDefinition serviceDefinition, string path, Action<RouteHandlerBuilder, PivotteRouteDefinition> builder = null)
    {
        var endpoints = new List<Endpoint>();

        foreach (var route in serviceDefinition.Routes)
        {
            var endpointBuilder = new RouteEndpointBuilder(null,
                RoutePatternFactory.Parse(Path.Combine(path, route.Route)), route.Order ?? 0);
            endpointBuilder.Metadata.Add(new HttpMethodMetadata(new []{route.Verb}));
            endpointBuilder.Metadata.Add(route.MethodInfo);

            var conventionBuilder = new PivotteServiceConventionBuilder();
            var handler = new RouteHandlerBuilder(new IEndpointConventionBuilder[] { conventionBuilder });
                
            builder?.Invoke(handler, route);
            handler.WithTags(serviceDefinition.Name);
            
            foreach (var e in conventionBuilder.Conventions)
            {
                e(endpointBuilder);
            }

            RequestDelegateFactory.Create(route.MethodInfo,
                x => x.RequestServices.GetRequiredService(route.ServiceType),
                new RequestDelegateFactoryOptions
                {
                    EndpointBuilder = endpointBuilder,
                    ServiceProvider = applicationServices
                },
                RequestDelegateFactory.InferMetadata(route.MethodInfo, new RequestDelegateFactoryOptions
                {
                    EndpointBuilder = endpointBuilder,
                    ServiceProvider = applicationServices
                }));
            
            

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