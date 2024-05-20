using Microsoft.Extensions.DependencyInjection;
using Pivot.NetClient.Impl;

namespace Pivot.NetClient;

public static class Extensions
{
    public static void AddPivotNetClientServices(this IServiceCollection services)
    {
        services.AddSingleton<IPivotClientGenerator, PivotClientGenerator>();
        services.AddSingleton<IPivotClientInvoker, PivotClientInvoker>();
    }
}