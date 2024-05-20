using Microsoft.AspNetCore.Http;

namespace Pivot.Services;

public interface IPivotResponseHandler
{
    Task HandleResponse(HttpContext ctx, PivotRouteDefinition route);
}