using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Pivotte.Services.Impl;

public class PivotResponseHandler : IPivotResponseHandler
{
    public async Task HandleResponse(HttpContext ctx, PivotRouteDefinition route)
    {
        var service = ctx.RequestServices.GetRequiredService(route.ServiceType);
        
        var del = RequestDelegateFactory.Create(route.MethodInfo,
            x => service,
            new RequestDelegateFactoryOptions());

        await del.RequestDelegate(ctx);
    }
}