using Microsoft.AspNetCore.Mvc.ApiExplorer;
using NSwag;

namespace SharpNSwag;

public interface ISharpNSwagBuilder
{
    ApiDescriptionGroup BuildApiDescriptions(Type controllerType);
    
    Task<OpenApiDocument> BuildOpenApiDoc(Type controllerType);
}