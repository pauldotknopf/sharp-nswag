using Microsoft.AspNetCore.Mvc.ApiExplorer;
using NSwag;

namespace Pivotte;

public interface ISharpNSwagBuilder
{
    ApiDescriptionGroup BuildApiDescriptions(Type controllerType);
    
    Task<OpenApiDocument> BuildOpenApiDoc(Type controllerType);
}