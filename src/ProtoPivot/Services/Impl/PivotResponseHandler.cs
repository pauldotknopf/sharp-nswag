using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProtoPivot.Services;

namespace ProtoPivot.Impl;

public class PivotResponseHandler : IPivotResponseHandler
{
    public async Task HandleResponse(HttpContext ctx, PivotRouteDefinition route)
    {
        if(ctx.Request.Method != route.Verb)
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.CompleteAsync();
            return;
        }

        var service = ctx.RequestServices.GetRequiredService(route.ServiceType);

        var del = RequestDelegateFactory.Create(route.MethodInfo,
            x => service,
            new RequestDelegateFactoryOptions
        {
            
        });

        await del.RequestDelegate(ctx);
    }
}