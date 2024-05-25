using Microsoft.AspNetCore.Http;

namespace Pivotte.Services;

public interface IPivotResponseHandler
{
    Task HandleResponse(HttpContext ctx, PivotRouteDefinition route);
}