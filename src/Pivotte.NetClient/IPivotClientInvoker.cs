using Pivotte.Services;

namespace Pivotte.NetClient;

public interface IPivotClientInvoker
{
    Task<object> Invoke(PivotServiceDefinition serviceDefinition, PivotRouteDefinition routeDefinition, HttpClient client, object[] args);
}