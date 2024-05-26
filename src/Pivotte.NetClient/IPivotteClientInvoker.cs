using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Pivotte.Services;

namespace Pivotte.NetClient;

public interface IPivotteClientInvoker
{
    Task<object> Invoke(PivotteServiceDefinition serviceDefinition, PivotteRouteDefinition routeDefinition, ApiDescription apiDescription, HttpClient client, object[] args);
}