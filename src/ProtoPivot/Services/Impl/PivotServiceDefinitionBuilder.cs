using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using ProtoPivot.Services;

namespace ProtoPivot.Impl;

public class PivotServiceDefinitionBuilder : IPivotServiceDefinitionBuilder
{
    public PivotServiceDefinition BuildServiceDefinition<T>()
    {
        var routes = new List<PivotRouteDefinition>();

        var serviceAttributes = typeof(T).GetCustomAttributes(inherit: true);

        foreach (var method in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            Console.WriteLine(method.Name);
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

            var parameters = new List<PivotRouteParameterDefinition>();
            
            foreach(var param in method.GetParameters())
            {
                var paramAttributes = param.GetCustomAttributes(inherit: true)
                    .OfType<IBindingSourceMetadata>()
                    .ToList();

                if (paramAttributes.Count == 0)
                {
                    throw new NotSupportedException($"you must provide a parameter source for {param.Name}");
                }

                if (paramAttributes.Count > 1)
                {
                    throw new NotSupportedException($"you can only provide one parameter source for {param.Name}");
                }
                
                parameters.Add(new PivotRouteParameterDefinition
                {
                    Name = param.Name,
                    Type = param.ParameterType,
                    Source = paramAttributes[0].BindingSource
                });
            }

            if (parameters.Count(x => x.Source.IsFromRequest) > 1)
            {
                throw new NotSupportedException($"method {method.Name} has multiple request body parameters");
            }
   
            routes.Add(new PivotRouteDefinition
            {
                ServiceType = typeof(T),
                MethodInfo = method,
                Route = routeModel.Template,
                Order = routeModel.Order,
                Verb = verbs[0]
            });
        }

        return new PivotServiceDefinition
        {
            Routes = routes
        };
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
}