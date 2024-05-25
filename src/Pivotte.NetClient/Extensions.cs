using Microsoft.Extensions.DependencyInjection;
using Pivotte.NetClient.Impl;

namespace Pivotte.NetClient;

public static class Extensions
{
    public static void AddPivotNetClientServices(this IServiceCollection services)
    {
        services.AddSingleton<IPivotClientGenerator, PivotClientGenerator>();
        services.AddSingleton<IPivotClientInvoker, PivotClientInvoker>();
    }
}