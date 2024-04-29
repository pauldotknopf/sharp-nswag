using Microsoft.AspNetCore.Http;

namespace ProtoPivot.Services;

public interface IPivotResponseHandler
{
    Task HandleResponse(HttpContext ctx, PivotRouteDefinition route);
}