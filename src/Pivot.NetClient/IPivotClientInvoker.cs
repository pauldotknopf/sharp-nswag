using Pivot.Services;

namespace Pivot.NetClient;

public interface IPivotClientInvoker
{
    Task<object> Invoke(PivotServiceDefinition serviceDefinition, PivotRouteDefinition routeDefinition, HttpClient client, object[] args);
}