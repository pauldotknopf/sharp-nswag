using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Pivotte.Services.Impl;

public class PivotteResponseHandler : IPivotteResponseHandler
{
    public async Task HandleResponse(HttpContext ctx, PivotteRouteDefinition route)
    {
        return;
    }
}