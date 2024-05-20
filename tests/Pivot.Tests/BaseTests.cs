using Microsoft.AspNetCore.TestHost;
using Pivot.NetClient;

namespace Pivot.Tests;

[TestClass]
public class BaseTests
{
    public TestServer BuildTestServer<T>(string path, T impl) where T : class
    {
        return new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPivotServices();
                services.AddPivotNetClientServices();
                services.AddSingleton(impl);
            }).Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapPivotService<T>(path);
                });
            }));
    }
}