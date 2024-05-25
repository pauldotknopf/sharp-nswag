using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

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

            var parameters = new List<PivotteRouteParameterDefinition>();

            int parameterIndex = 0;
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
                
                parameters.Add(new PivotteRouteParameterDefinition
                {
                    Index = parameterIndex,
                    Name = param.Name,
                    Type = param.ParameterType,
                    Source = paramAttributes[0].BindingSource
                });

                parameterIndex++;
            }

            if (parameters.Count(x => x.Source == BindingSource.Body) > 1)
            {
                throw new NotSupportedException($"method {method.Name} has multiple request body parameters");
            }

            var routePattern = RoutePatternFactory.Parse(routeModel.Template);

            foreach (var pattern in routePattern.Parameters)
            {
                if (pattern.IsOptional)
                {
                    throw new Exception($"method {method.Name} has an optional route value {pattern.Name}, this is not supported");
                }
            }
            
            foreach (var routeParameter in parameters.Where(x => x.Source == BindingSource.Path))
            {
                if (routePattern.Parameters.All(x => x.Name != routeParameter.Name))
                {
                    throw new NotSupportedException(
                        $"method {method.Name} calls for route value {routeParameter.Name} that wasn't defined in the route");
                }
            }

            foreach (var requiredRouteValue in routePattern.RequiredValues.Keys)
            {
                if (parameters.All(x => x.Name != requiredRouteValue))
                {
                    throw new NotSupportedException($"method {method.Name} doesn't reference required route value {requiredRouteValue}");
                }
            }
            
            routes.Add(new PivotteRouteDefinition
            {
                ServiceType = type,
                MethodInfo = method,
                Route = routeModel.Template,
                Order = routeModel.Order,
                Verb = verbs[0],
                Parameters = parameters
            });
        }

        return new PivotteServiceDefinition
        {
            Name = name,
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