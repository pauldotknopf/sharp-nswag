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
        Task RouteWithRouteValue([FromRoute]int id);
        
        [Route("routewithjsonbody")]
        [HttpPost]
        Task RouteWithJsonBody([FromBody]JsonBody body);
        
        [Route("methodwithresult")]
        [HttpPost]
        Task<JsonBody> MethodWithResult();
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
}