using Microsoft.AspNetCore.TestHost;
using ProtoPivot.Impl;
using ProtoPivot.NetClient;

namespace ProtoPivot.Tests;

[TestClass]
public class BaseTests
{
    public TestServer BuildTestServer<T>(string path, T impl) where T : class
    {
        return new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddProtoPivotServices();
                services.AddPivotNetClientServices();
                services.AddSingleton(impl);
            }).Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapProtoPivotService<T>(path);
                });
            }));
    }
}