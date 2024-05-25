using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Pivotte.Generators.Tests;

[TestClass]
public class TypeScriptTests
{
    private IServiceProvider _sp;
    
    public TypeScriptTests()
    {
        _sp = new ServiceCollection().AddPivotteServices().AddPivotGeneratorServices().BuildServiceProvider();
    }
    
    public class SharedResponse
    {
        public string Message { get; set; }
    }

    public class NonSharedRequest
    {
        public string Name { get; set; }
    }
    
    [PivotteService("Foo")]
    public interface ITestService1
    {
        [HttpGet]
        [Route("route1")]
        SharedResponse Route1([FromBody]NonSharedRequest request);
    }
    
    [PivotteService("Bar")]
    public interface ITestService2
    {
        [HttpGet]
        [Route("route2")]
        SharedResponse Route2();
    }
    
    [TestMethod]
    public async Task CanGenerateTypeScriptFiles()
    {
        var generator = _sp.GetRequiredService<IGenerator>();
        var ts = await generator.GenerateClientTypeScript(config =>
        {
            config.AddService<ITestService1>();
            config.AddService<ITestService2>();
        });

        ts.Should().NotBeNullOrEmpty();
        // test the output using this:
        // tsc {file} --lib es2015,dom
    }
}