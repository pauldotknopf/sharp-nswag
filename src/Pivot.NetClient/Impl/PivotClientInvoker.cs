using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Patterns;
using Pivot.Services;

namespace Pivot.NetClient.Impl;

public class PivotClientInvoker : IPivotClientInvoker
{
    private readonly RoutePatternTransformer _routePatternTransformer;

    public PivotClientInvoker(RoutePatternTransformer routePatternTransformer)
    {
        _routePatternTransformer = routePatternTransformer;
    }
    
    public async Task<object> Invoke(PivotServiceDefinition serviceDefinition, PivotRouteDefinition routeDefinition, HttpClient client, object[] args)
    {
        var routeTemplate = routeDefinition.Route;
        var queryParameters = new Dictionary<string, string>();
        
        foreach (var parameter in routeDefinition.Parameters)
        {
            if (parameter.Source == BindingSource.Path)
            {
                routeTemplate = routeTemplate.Replace($"{{{parameter.Name}}}", args[parameter.Index].ToString());
            }
            else if (parameter.Source == BindingSource.Query)
            {
                queryParameters.Add(parameter.Name, args[parameter.Index].ToString());
            }
        }

        if(queryParameters.Count > 0)
        {
            routeTemplate += "?" + string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));
        }
        
        var request = new HttpRequestMessage(new HttpMethod(routeDefinition.Verb), routeTemplate);

        var bodyParameter = routeDefinition.Parameters.SingleOrDefault(x => x.Source == BindingSource.Body);
        if (bodyParameter != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(args[bodyParameter.Index],
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                Encoding.UTF8,
                "application/json");
        }
        
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var returnType = routeDefinition.MethodInfo.ReturnType;
        object result = null;

        if (routeDefinition.MethodInfo.ReturnType != typeof(void) && routeDefinition.MethodInfo.ReturnType != typeof(Task))
        {
            if (returnType.IsGenericType)
            {
                if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    returnType = returnType.GetGenericArguments()[0];
                }
                else if (returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    returnType = returnType.GetGenericArguments()[0];
                }
            }
            var responseBody = await response.Content.ReadAsStringAsync();
            result = JsonSerializer.Deserialize(responseBody, returnType,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        return result;
    }
}