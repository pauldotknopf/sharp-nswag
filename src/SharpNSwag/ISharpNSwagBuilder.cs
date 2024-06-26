using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.AspNetCore;

namespace SharpNSwag;

public interface ISharpNSwagBuilder
{
    Task<OpenApiDocument> BuildOpenApiDoc(Type controllerType, Action<AspNetCoreOpenApiDocumentGeneratorSettings> configureSettings = null, Action<IServiceCollection> configureServices = null);
}