using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Pivotte.Services.Impl;

public class PivotteServiceDefinitionBuilder : IPivotteServiceDefinitionBuilder
{
    public PivotteServiceDefinition BuildServiceDefinition<T>()
    {
        return BuildServiceDefinition(typeof(T));
    }

    public PivotteServiceDefinition BuildServiceDefinition(Type type)
    {
        var name = "";
        var routes = new List<PivotteRouteDefinition>();

        var serviceAttributes = type.GetCustomAttributes(inherit: true).OfType<PivotteServiceAttribute>().ToList();
        
        if(serviceAttributes.Count > 1) throw new NotSupportedException($"you can only have one PivotServiceDefinition attribute on {type.Name}");
        if(serviceAttributes.Count == 0) throw new NotSupportedException($"you must provide a PivotServiceDefinition attribute on {type.Name}");

        name = serviceAttributes[0].Name;

        if (string.IsNullOrEmpty(name)) throw new NotSupportedException($"you must provide a name for {type.Name}");
        
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var attributes = method.GetCustomAttributes(inherit: true);
            
            var routeModel = GetRouteModel(attributes.OfType<IRouteTemplateProvider>());
            
            if (routeModel == null || string.IsNullOrEmpty(routeModel.Template)) throw new NotSupportedException($"you must provide a route for {method.Name}");
            
            if (routeModel.IsAbsoluteTemplate)
            {
                throw new Exception($"method {method.Name} has an absolute route of {routeModel.Template}, this is not supported");
            }
            
            var verbs = GetHttpMethods(attributes.OfType<IActionHttpMethodProvider>());
            
            if (verbs.Count == 0) throw new NotSupportedException($"you must provide an HTTP verb for {method.Name}");
            
            if (verbs.Count > 1) throw new NotSupportedException($"you can only provide one HTTP verb for {method.Name}");

            routes.Add(new PivotteRouteDefinition
            {
                ServiceType = type,
                MethodInfo = method,
                Route = routeModel.Template,
                Order = routeModel.Order,
                Verb = verbs[0]
            });
        }

        return new PivotteServiceDefinition
        {
            Name = name,
            Routes = routes
        };
    }

    public List<ApiDescription> BuildApiDescriptions(Type type)
    {
        var services = new ServiceCollection();
        services.AddEndpointsApiExplorer();
        services.AddRoutingCore();
        services.AddLogging();
        services.AddSingleton<IHostEnvironment, FakeEnv>();

        var serviceDefinition = BuildServiceDefinition(type);
        var endpointDataSource = new PivotteServiceEndpointDataSource(null, serviceDefinition,string.Empty);
        services.AddSingleton<EndpointDataSource>(endpointDataSource);
        
        var sp = services.BuildServiceProvider();

        var apiDescriptionGroupCollectionProvider = sp.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
        return apiDescriptionGroupCollectionProvider.ApiDescriptionGroups.Items[0].Items.ToList();
    }

    private List<string> GetHttpMethods(IEnumerable<IActionHttpMethodProvider> attributes)
    {
        var result = new List<string>();
        
        foreach (var attribute in attributes)
        {
            foreach (var value in attribute.HttpMethods.Select(x => x.ToUpper()).ToList())
            {
                if (!result.Contains(value))
                {
                    result.Add(value);
                }
            }
        }

        return result;
    }
    
    private AttributeRouteModel GetRouteModel(IEnumerable<IRouteTemplateProvider> attributes)
    {
        AttributeRouteModel current = new AttributeRouteModel();
        
        foreach (var routeTemplateProvider in attributes)
        {
            current = AttributeRouteModel.CombineAttributeRouteModel(current, new AttributeRouteModel(routeTemplateProvider));
        }

        return current;
    }
    
    class FakeEnv : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "Pivot";

        public IFileProvider ContentRootFileProvider
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public string ContentRootPath { get; set; } = "contentroot";
        public string EnvironmentName { get; set; } = "environmentname";
    }
}