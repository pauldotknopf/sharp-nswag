using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;

namespace Pivotte.Generators.Tests;

[TestClass]
public class SanityTests
{
    public class RequestType
    {
        public string Foo { get; set; }
    }

    public class ResponseType
    {
        public string Bar { get; set; }
    }
    
    [PivotteService("Test")]
    public interface ITestService
    {
        [HttpGet]
        [Route("test-route1")]
        ResponseType Route([FromBody]RequestType request);
    }
    
    [TestMethod]
    public async Task CreatedApiDefinitionIsSimilarToDefault()
    {
        var builder = WebApplication.CreateEmptyBuilder(new());
        // CreateEmptyBuilder doesn't register an IServer or Routing.
        builder.Services.AddRoutingCore();
        builder.Services.AddPivotteServices();
        builder.Services.AddEndpointsApiExplorer();
        builder.WebHost.UseKestrelCore();
        var app = builder.Build();
        app.UseRouting();
        app.MapGet("test-route2", ([FromBody]RequestType request) => new ResponseType { Bar = request.Foo });
        app.MapPivotteService<ITestService>(string.Empty);

        await app.StartAsync();
        
        var apiDescriptions = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

        var theirs = apiDescriptions.ApiDescriptionGroups.Items[0].Items[0];
        var ours = apiDescriptions.ApiDescriptionGroups.Items[0].Items[1];

        theirs.RelativePath.Should().Be("test-route2");
        ours.RelativePath.Should().Be("test-route1");

        theirs.HttpMethod.Should().Be(ours.HttpMethod);
        theirs.ParameterDescriptions[0].Name.Should().Be(ours.ParameterDescriptions[0].Name);
        theirs.ParameterDescriptions[0].Type.Should().Be(ours.ParameterDescriptions[0].Type);
        theirs.ParameterDescriptions[0].Source.Should().Be(ours.ParameterDescriptions[0].Source);
        theirs.SupportedRequestFormats.Count.Should().Be(ours.SupportedRequestFormats.Count);
        theirs.SupportedResponseTypes.Count.Should().Be(ours.SupportedResponseTypes.Count);
        theirs.SupportedRequestFormats[0].MediaType.Should().Be(ours.SupportedRequestFormats[0].MediaType);
        theirs.SupportedResponseTypes[0].Type.Should().Be(ours.SupportedResponseTypes[0].Type);
    }
}