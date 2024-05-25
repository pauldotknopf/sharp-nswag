using NSwag;

namespace Pivot.Generators;

public interface IGenerator
{
    Task<string> GenerateClientTypeScript(Action<GenerateConfig> configure);
    
    Task<OpenApiDocument> GenerateOpenApiDoc(Type serviceType);
}