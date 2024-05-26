using Microsoft.AspNetCore.Mvc;
using Pivotte.Services;

namespace Pivotte.Tests;

[TestClass]
public class ApiGroupDescriptionTests
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
        [Route("test-route1/{test}")]
        ResponseType Route([FromBody]RequestType request);
    }

    [TestMethod]
    public void CanGenerateApiDescriptions()
    {
        var sp = new ServiceCollection().AddPivotteServices().BuildServiceProvider()
            .GetRequiredService<IPivotteServiceDefinitionBuilder>();

        var apiDescriptions = sp.BuildApiDescriptions(typeof(ITestService));
    }
}