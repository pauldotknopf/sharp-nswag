using Microsoft.AspNetCore.TestHost;
using Pivotte;
using Pivotte.NetClient;

namespace Pivotte.Tests;

[TestClass]
public class BaseTests
{
    public TestServer BuildTestServer<T>(string path, T impl) where T : class
    {
        return new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPivotteServices();
                services.AddPivotteNetClientServices();
                services.AddSingleton(impl);
            }).Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapPivotteService<T>(path);
                });
            }));
    }
}