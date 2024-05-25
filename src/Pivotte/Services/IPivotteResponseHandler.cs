using Microsoft.AspNetCore.Http;

namespace Pivotte.Services;

public interface IPivotteResponseHandler
{
    Task HandleResponse(HttpContext ctx, PivotteRouteDefinition route);
}