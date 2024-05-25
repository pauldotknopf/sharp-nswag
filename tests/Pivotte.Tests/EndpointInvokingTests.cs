using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Pivotte.Tests;

[TestClass]
public class EndpointInvokingTests : BaseTests
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
        
        [Route("MethodWithResultAsync")]
        [HttpPost]
        ValueTask<JsonBody> MethodWithResultAsync();
    }
    
    [TestMethod]
    public async Task CanInvokeEndpoint()
    {
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.MyRoute());
        
        var testServer = BuildTestServer("test", impl.Object);
       
        var client = testServer.CreateClient();
        var responseContent = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test/myroute"));
        
        responseContent.StatusCode.Should().Be(HttpStatusCode.OK);
        impl.Verify(x => x.MyRoute(), Times.Once);
    }

    [TestMethod]
    public async Task CanGet405IfInvalidVerb()
    {
        var testServer = BuildTestServer("test", new Mock<ITestService>().Object);
       
        var client = testServer.CreateClient();
        var responseContent = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/test/myroute"));
        
        responseContent.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }
    
    [TestMethod]
    public async Task CanInvokeRouteWithRouteValue()
    {
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.RouteWithRouteValue(3));
        
        var testServer = BuildTestServer("test", impl.Object);
       
        var client = testServer.CreateClient();
        var responseContent = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/test/routevalue/3"));
        
        responseContent.StatusCode.Should().Be(HttpStatusCode.OK);
        impl.Verify(x => x.RouteWithRouteValue(3), Times.Once);
    }
    
    [TestMethod]
    public async Task CanInvokeRouteWithJsonBody()
    {
        JsonBody jsonBody = null;
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.RouteWithJsonBody(It.IsAny<JsonBody>()))
            .Callback(new Action<JsonBody>(x =>
            {
                jsonBody = x;
            }));
        
        var testServer = BuildTestServer("test", impl.Object);
       
        var client = testServer.CreateClient();
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/test/routewithjsonbody");
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(new JsonBody
        {
            Name = "Paul",
            YearOfBirth = 1988
        }), Encoding.UTF8, "application/json");
        var responseContent = await client.SendAsync(requestMessage);
        
        responseContent.StatusCode.Should().Be(HttpStatusCode.OK);
        impl.Verify(x => x.RouteWithJsonBody(It.IsAny<JsonBody>()), Times.Once);
        jsonBody.Should().NotBeNull();
        jsonBody.Name.Should().Be("Paul");
        jsonBody.YearOfBirth.Should().Be(1988);
    }
    
    [TestMethod]
    public async Task CanInvokeMethodWithResult()
    {
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.MethodWithResult())
            .Returns(Task.FromResult(new JsonBody
            {
                Name = "Paul",
                YearOfBirth = 1988
            }));
        
        var testServer = BuildTestServer("test", impl.Object);
       
        var client = testServer.CreateClient();
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/test/methodwithresult");
        var responseContent = await client.SendAsync(requestMessage);
        
        responseContent.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await responseContent.Content.ReadAsStringAsync();
        var responseDeserialized = JsonSerializer.Deserialize<JsonBody>(responseBody,new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        });
        responseDeserialized.Should().NotBeNull();
        responseDeserialized.Name.Should().Be("Paul");
        responseDeserialized.YearOfBirth.Should().Be(1988);
    }
    
    [TestMethod]
    public async Task CanInvokeMethodWithResultAsync()
    {
        var impl = new Mock<ITestService>();
        impl.Setup(x => x.MethodWithResultAsync())
            .Returns(ValueTask.FromResult(new JsonBody
            {
                Name = "Paul",
                YearOfBirth = 1988
            }));
        
        var testServer = BuildTestServer("test", impl.Object);
       
        var client = testServer.CreateClient();
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/test/methodwithresultasync");
        var responseContent = await client.SendAsync(requestMessage);
        
        responseContent.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await responseContent.Content.ReadAsStringAsync();
        var responseDeserialized = JsonSerializer.Deserialize<JsonBody>(responseBody,new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        });
        responseDeserialized.Should().NotBeNull();
        responseDeserialized.Name.Should().Be("Paul");
        responseDeserialized.YearOfBirth.Should().Be(1988);
    }
}