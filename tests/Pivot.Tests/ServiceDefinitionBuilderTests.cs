using Microsoft.AspNetCore.Mvc;
using Pivot.Services;
using Pivot.Services.Impl;

namespace Pivot.Tests;

[TestClass]
public class ServiceDefinitionBuilderTests : BaseTests
{
    IPivotServiceDefinitionBuilder _pivotServiceDefinitionBuilder;
    
    public ServiceDefinitionBuilderTests()
    {
        _pivotServiceDefinitionBuilder = new PivotServiceDefinitionBuilder();
    }
    
    public interface ITestService
    {
        [HttpGet]
        [Route("route1")]
        void Route1();

        [HttpPost]
        [Route("route2")]
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