using Microsoft.Extensions.DependencyInjection;
using Pivotte.NetClient.Impl;

namespace Pivotte.NetClient;

public static class Extensions
{
    public static void AddPivotteNetClientServices(this IServiceCollection services)
    {
        services.AddSingleton<IPivotteClientGenerator, PivotteClientGenerator>();
        services.AddSingleton<IPivotteClientInvoker, PivotteClientInvoker>();
    }
}