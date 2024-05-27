using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.AspNetCore;

namespace Pivotte.Impl;

public class SharpNSwagBuilder : ISharpNSwagBuilder
{
    public ApiDescriptionGroup BuildApiDescriptions(Type type)
    {
        var services = new ServiceCollection();
        services.AddOpenApiDocument();
        services.AddLogging();
        services.AddSingleton<IHostEnvironment, FakeEnv>();
        var mvcBuilder = services.AddControllers();

        var old = mvcBuilder.PartManager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().FirstOrDefault();
        mvcBuilder.PartManager.FeatureProviders.Remove(old);
        mvcBuilder.PartManager.FeatureProviders.Add(
            new ManualControllerFeatureProvider(f =>
            {
                f.Controllers.Add(type.GetTypeInfo());
            }));

        var sp = services.BuildServiceProvider();

        var apiDescriptionGroupCollectionProvider = sp.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
        return apiDescriptionGroupCollectionProvider.ApiDescriptionGroups.Items[0];
    }

    public async Task<OpenApiDocument> BuildOpenApiDoc(Type type)
    {
        var apiDescriptions = BuildApiDescriptions(type);
        var docgen = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings
        {
            Title = type.Name
        });
        return await docgen.GenerateAsync(new ApiDescriptionGroupCollection(
            new ReadOnlyCollection<ApiDescriptionGroup>(new List<ApiDescriptionGroup>{apiDescriptions}), 0));
    }
    
    class ManualControllerFeatureProvider
        : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly Action<ControllerFeature> _action;

        public ManualControllerFeatureProvider(Action<ControllerFeature> action)
        {
            _action = action;
        }
        
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            _action(feature);
        }
    }

    class FakeEnv : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "Pivot";
        public IFileProvider ContentRootFileProvider
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public string ContentRootPath { get; set; } = "contentroot";
        public string EnvironmentName { get; set; } = "environmentname";
    }
}