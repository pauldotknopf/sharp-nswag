using Microsoft.Extensions.DependencyInjection;

namespace ProtoPivot.NetClient;

public static class Extensions
{
    public static void AddPivotNetClientServices(this IServiceCollection services)
    {
        services.AddSingleton<IPivotClientGenerator, Impl.PivotClientGenerator>();
        services.AddSingleton<IPivotClientInvoker, Impl.PivotClientInvoker>();
    }
}