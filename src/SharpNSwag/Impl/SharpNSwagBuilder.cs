using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation;
using NSwag.Generation.AspNetCore;

namespace SharpNSwag.Impl;

public class SharpNSwagBuilder : ISharpNSwagBuilder
{
    public async Task<OpenApiDocument> BuildOpenApiDoc(Type type, Action<AspNetCoreOpenApiDocumentGeneratorSettings> configureSettings = null, Action<IServiceCollection> configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddOpenApiDocument((settings, sp) =>
        {
            configureSettings?.Invoke(settings);
        });
        services.AddLogging();
        services.AddSingleton<IHostEnvironment, FakeEnv>();
        configureServices?.Invoke(services);
        
        var mvcBuilder = services.AddControllers();
        var old = mvcBuilder.PartManager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().FirstOrDefault();
        mvcBuilder.PartManager.FeatureProviders.Remove(old);
        mvcBuilder.PartManager.FeatureProviders.Add(
            new ManualControllerFeatureProvider(f =>
            {
                f.Controllers.Add(type.GetTypeInfo());
            }));

        var sp = services.BuildServiceProvider();

        var doc = sp.GetRequiredService<OpenApiDocumentRegistration>();
        return await sp.GetRequiredService<IOpenApiDocumentGenerator>().GenerateAsync(doc.DocumentName);
    }
    
    class ManualControllerFeatureProvider(Action<ControllerFeature> action)
        : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            action(feature);
        }
    }

    class FakeEnv : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "SharpNSwag";
        public IFileProvider ContentRootFileProvider
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public string ContentRootPath { get; set; } = "contentroot";
        public string EnvironmentName { get; set; } = "environmentname";
    }
}