using Microsoft.AspNetCore.TestHost;
using ProtoPivot.Impl;

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
                services.AddSingleton(impl);
            }).Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapProtoPivotService<EndpointInvokingTests.ITestService>(path);
                });
            }));
    }
}