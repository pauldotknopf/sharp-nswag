using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pivotte.Tests;

namespace Pivotte.NetClient.Tests;

[TestClass]
public class ClientTests : BaseTests
{
    public class JsonBody
    {
        public string Name { get; set; }
        
        public int YearOfBirth { get; set; }
    }
    
    [PivotteService("Test")]
    public interface ITestService
    {
        [HttpGet]
        [Route("myroute")]
        Task MyRoute();

        [Route("routevalue/{id}")]
        [HttpPost]
        Task RouteWithRouteValue(int id);
        
        [Route("routewithjsonbody")]
        [HttpPost]
        Task RouteWithJsonBody(JsonBody body);
        
        [Route("methodwithresult")]
        [HttpPost]
        Task<JsonBody> MethodWithResult();
        
        [Route("methodwithmultipleparams/{id}")]
        [HttpPost]
        Task<JsonBody> MethodWithMultipleParameters(int id, JsonBody body);
    }
    
    [TestMethod]
    public async Task CanInvokeEndpointWithRouteData()
    {
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.RouteWithRouteValue(3));
        var server = BuildTestServer("", impl.Object);

        var httpClient = server.CreateClient();
        var clientGenerator = server.Services.GetRequiredService<IPivotteClientGenerator>();
        var client = clientGenerator.Generate<ITestService>(httpClient);
        await client.RouteWithRouteValue(3);
        
        impl.Verify(x => x.RouteWithRouteValue(3), Times.Once);
    }
    
    [TestMethod]
    public async Task CanInvokeEndpointWithJsonBody()
    {
        JsonBody jsonBody = null;
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.RouteWithJsonBody(It.IsAny<JsonBody>()))
            .Callback(new Action<JsonBody>(x =>
            {
                jsonBody = x;
            }));
        
        var testServer = BuildTestServer("", impl.Object);
        var httpClient = testServer.CreateClient();
        var clientGenerator = testServer.Services.GetRequiredService<IPivotteClientGenerator>();
        var client = clientGenerator.Generate<ITestService>(httpClient);
        await client.RouteWithJsonBody(new JsonBody
        {
            Name = "Paul",
            YearOfBirth = 1988
        });
        
        impl.Verify(x => x.RouteWithJsonBody(It.IsAny<JsonBody>()), Times.Once);
        jsonBody.Should().NotBeNull();
        jsonBody.Name.Should().Be("Paul");
        jsonBody.YearOfBirth.Should().Be(1988);
    }

    [TestMethod]
    public async Task CanInvokeEndpointWithJsonResponse()
    {
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.MethodWithResult())
            .Returns(Task.FromResult(new JsonBody
            {
                Name = "Paul",
                YearOfBirth = 1988
            }));
        
        var testServer = BuildTestServer("", impl.Object);
        var httpClient = testServer.CreateClient();
        var clientGenerator = testServer.Services.GetRequiredService<IPivotteClientGenerator>();
        var client = clientGenerator.Generate<ITestService>(httpClient);
        var response = await client.MethodWithResult();
        
        response.Name.Should().Be("Paul");
        response.YearOfBirth.Should().Be(1988);
    }
    
    [TestMethod]
    public async Task CanInvokeEndpointWithMultipleParameters()
    {
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.MethodWithMultipleParameters(It.IsAny<int>(), It.IsAny<JsonBody>()))
            .Returns(new Func<int, JsonBody, Task<JsonBody>>((routeValue, body) =>
            {
                return Task.FromResult(new JsonBody
                {
                    Name = body.Name + " " + routeValue,
                    YearOfBirth = body.YearOfBirth
                });
            }));
        
        var testServer = BuildTestServer("", impl.Object);
        var httpClient = testServer.CreateClient();
        var clientGenerator = testServer.Services.GetRequiredService<IPivotteClientGenerator>();
        var client = clientGenerator.Generate<ITestService>(httpClient);
        var response = await client.MethodWithMultipleParameters(2, new JsonBody
        {
            Name = "Paul",
            YearOfBirth = 1988
        });
        
        response.Name.Should().Be("Paul 2");
        response.YearOfBirth.Should().Be(1988);
    }
}