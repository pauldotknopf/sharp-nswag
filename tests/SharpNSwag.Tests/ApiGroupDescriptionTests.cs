using Microsoft.AspNetCore.Mvc;

namespace SharpNSwag.Tests;

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
    
    public class TestServiceController
    {
        [HttpGet]
        [Route("test-route1/{test}")]
        public virtual ResponseType Route([FromBody] RequestType request)
        {
            throw new InvalidOperationException();
        }
    }

    [TestMethod]
    public void CanGenerateApiDescriptions()
    {
        var builder = new ServiceCollection().AddSharpNSwagServices().BuildServiceProvider()
            .GetRequiredService<ISharpNSwagBuilder>();

        var apiDescriptions = builder.BuildApiDescriptions(typeof(TestServiceController));
        apiDescriptions.Items.Should().HaveCount(1);
        apiDescriptions.Items[0].RelativePath.Should().Be("test-route1/{test}");
    }
}