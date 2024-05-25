using Microsoft.AspNetCore.Mvc;
using Pivotte.Services;
using Pivotte.Services.Impl;

namespace Pivotte.Tests;

[TestClass]
public class ServiceDefinitionBuilderTests : BaseTests
{
    IPivotteServiceDefinitionBuilder _pivotServiceDefinitionBuilder;
    
    public ServiceDefinitionBuilderTests()
    {
        _pivotServiceDefinitionBuilder = new PivotteServiceDefinitionBuilder();
    }
    
    [PivotteService("Test")]
    public interface ITestService
    {
        [HttpGet]
        [Route("route1")]
        [Consumes("application/json")]
        [Produces("application/json")]
        void Route1();

        [HttpPost]
        [Route("route2")]
        [Consumes("application/json")]
        [Produces("application/json")]
        void Route2();
    }
    
    [TestMethod]
    public void CanBuildDefinition()
    {
        var def = _pivotServiceDefinitionBuilder.BuildServiceDefinition<ITestService>();

        def.Routes.Should().HaveCount(2);
        
        var route1 = def.Routes.SingleOrDefault(x => x.Route == "route1");
        route1.Should().NotBeNull();
        route1.Verb.Should().Be("GET");
        
        var route2 = def.Routes.SingleOrDefault(x => x.Route == "route2");
        route2.Should().NotBeNull();
        route2.Verb.Should().Be("POST");
    }
}