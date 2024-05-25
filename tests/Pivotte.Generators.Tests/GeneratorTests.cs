using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Pivotte.Generators.Tests;

[TestClass]
public class GeneratorTests
{
    private IServiceProvider _sp;
    
    public GeneratorTests()
    {
        _sp = new ServiceCollection().AddPivotteServices().AddPivotGeneratorServices().BuildServiceProvider();
    }
    
    [PivotteService("Test1")]
    public interface ITestService1
    {
        [HttpGet]
        [Route("route1")]
        TypeScriptTests.SharedResponse Route1();
    }
    
    [PivotteService("Test2")]
    public interface ITestService2
    {
        [HttpGet]
        [Route("route2")]
        TypeScriptTests.SharedResponse Route2();
    }
    
    [TestMethod]
    public async Task CanGenerateOpenApiForService()
    {
        var generator = _sp.GetRequiredService<IGenerator>();
        var openApiDoc = await generator.GenerateOpenApiDoc(typeof(ITestService1));
        openApiDoc.Operations.Should().HaveCount(1);
    }
}