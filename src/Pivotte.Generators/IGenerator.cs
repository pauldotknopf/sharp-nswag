using Microsoft.AspNetCore.Mvc.ApiExplorer;
using NSwag;

namespace Pivotte.Generators;

public interface IGenerator
{
    Task<string> GenerateClientTypeScript(Action<GenerateConfig> configure);
    
    Task<OpenApiDocument> GenerateOpenApiDoc(Type serviceType);
    
    Task<ApiDescriptionGroupCollection> GenerateApiDescriptionGroups(Type serviceType);
}