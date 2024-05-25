using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Generators.Impl;

namespace Pivot.Generators.Tests;

[TestClass]
public class TypeScriptTests
{
    private IServiceProvider _sp;
    
    public TypeScriptTests()
    {
        _sp = new ServiceCollection().AddPivotServices().AddPivotGeneratorServices().BuildServiceProvider();
    }
    
    public class SharedResponse
    {
        public string Message { get; set; }
    }

    public class NonSharedRequest
    {
        public string Name { get; set; }
    }
    
    [PivotService("Foo")]
    public interface ITestService1
    {
        [HttpGet]
        [Route("route1")]
        SharedResponse Route1([FromBody]NonSharedRequest request);
    }
    
    [PivotService("Bar")]
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