using Microsoft.AspNetCore.Mvc;

namespace SharpNSwag.Tests;

[TestClass]
public class TypeScriptTests
{
    private IServiceProvider _sp;
    
    public TypeScriptTests()
    {
        _sp = new ServiceCollection().AddSharpNSwagServices().BuildServiceProvider();
    }
    
    public class SharedResponse
    {
        public string Message { get; set; }
    }

    public class NonSharedRequest
    {
        public string Name { get; set; }
    }
    
    public abstract class TestService1
    {
        [HttpGet]
        [Route("route1")]
        public virtual SharedResponse Route1([FromBody] NonSharedRequest request)
        {
            throw new NotImplementedException();
        }
    }
    
    public abstract class TestService2
    {
        [HttpGet]
        [Route("route2")]
        public virtual SharedResponse Route2()
        {
            throw new NotImplementedException();
        }
    }
    
    [TestMethod]
    public async Task CanGenerateTypeScriptFiles()
    {
        var generator = _sp.GetRequiredService<ISharpNSwagGenerator>();
        var ts = await generator.GenerateClientTypeScript(config =>
        {
            config.AddController<TestService2>();
            config.AddController<TestService1>();
        });

        ts.Should().NotBeNullOrEmpty();
        // test the output using this:
        // tsc {file} --lib es2015,dom
    }
}