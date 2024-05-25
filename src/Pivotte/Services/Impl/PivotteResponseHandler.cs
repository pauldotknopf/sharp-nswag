using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Pivotte.Services.Impl;

public class PivotteResponseHandler : IPivotteResponseHandler
{
    public async Task HandleResponse(HttpContext ctx, PivotteRouteDefinition route)
    {
        var service = ctx.RequestServices.GetRequiredService(route.ServiceType);
        
        var del = RequestDelegateFactory.Create(route.MethodInfo,
            x => service,
            new RequestDelegateFactoryOptions());

        await del.RequestDelegate(ctx);
    }
}