using Pivotte.Services;

namespace Pivotte.NetClient;

public interface IPivotteClientInvoker
{
    Task<object> Invoke(PivotteServiceDefinition serviceDefinition, PivotteRouteDefinition routeDefinition, HttpClient client, object[] args);
}