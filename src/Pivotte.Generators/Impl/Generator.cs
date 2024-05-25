using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.TypeScript;
using NSwag.Generation.AspNetCore;
using Pivotte.Services;

namespace Pivotte.Generators.Impl;

// ReSharper disable MethodHasAsyncOverload

public class Generator : IGenerator
{
    private readonly IPivotteServiceDefinitionBuilder _pivotteServiceDefinition;
    private readonly IServiceProvider _applicationServices;

    public Generator(IPivotteServiceDefinitionBuilder pivotteServiceDefinition,
        IServiceProvider applicationServices)
    {
        _pivotteServiceDefinition = pivotteServiceDefinition;
        _applicationServices = applicationServices;
    }
    
    class FakeEnv : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "Pivot.Generators";

        public IFileProvider ContentRootFileProvider
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public string ContentRootPath { get; set; } = "contentroot";
        public string EnvironmentName { get; set; } = "environmentname";
    }

    private List<EndpointDataSource> GetEndpointDataSources(IEnumerable<Type> services, Action<RouteHandlerBuilder, PivotteRouteDefinition> builder = null)
    {
        var result = new List<EndpointDataSource>();
        
        foreach (var service in services)
        {
            var serviceDefinition = _pivotteServiceDefinition.BuildServiceDefinition(service);
            result.Add(new PivotteServiceEndpointDataSource(_applicationServices, serviceDefinition, "/test", builder));
        }

        return result;
    }
    
    public async Task<string> GenerateClientTypeScript(Action<GenerateConfig> configure)
    {
        var config = new GenerateConfig();
        configure(config);
        
        var openApiDocs = new List<(OpenApiDocument doc, PivotteServiceDefinition def)>();

        foreach (var service in config.Services)
        {
            openApiDocs.Add((await GenerateOpenApiDoc(service), _pivotteServiceDefinition.BuildServiceDefinition(service)));
        }

        var sb = new StringBuilder();
        var sw = new TypeScriptStringWriter(sb, new TypeScriptClientGeneratorSettings().ExceptionClass);
        
        // let's generate all the type definitions first.
        {
            var doc = new OpenApiDocument();
            foreach (var (serviceDoc, _) in openApiDocs)
            {
                foreach (var definition in serviceDoc.Definitions)
                {
                    doc.Definitions[definition.Key] = definition.Value;
                }
            }

            var generator = new TypeScriptClientGenerator(doc, new TypeScriptClientGeneratorSettings());
            foreach (var line in generator.GenerateFile(ClientGeneratorOutputType.Contracts).Split(Environment.NewLine))
            {
                sw.WriteLine(line);
            }
        }

        var index = 0;
        foreach (var (doc, def) in openApiDocs)
        {
            var settings = new TypeScriptClientGeneratorSettings
            {
                ClassName = def.Name
            };
            var generator = new TypeScriptClientGenerator(doc, settings);
            foreach (var line in generator.GenerateFile(ClientGeneratorOutputType.Implementation).Split(Environment.NewLine))
            {
                sw.WriteLine(line);
            }
        }
        sw.Dispose();
        return sb.ToString();
    }
    
    public async Task<OpenApiDocument> GenerateOpenApiDoc(Type serviceType)
    {
        var docgen = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
        return await docgen.GenerateAsync(await GenerateApiDescriptionGroups(serviceType));
    }

    public Task<ApiDescriptionGroupCollection> GenerateApiDescriptionGroups(Type serviceType)
    {
        var endpointDataSources = GetEndpointDataSources(new []{serviceType});
        
        var services = new ServiceCollection();
        services.AddEndpointsApiExplorer();
        services.AddRoutingCore();
        services.AddLogging();
        services.AddSingleton<IHostEnvironment, FakeEnv>();
        
        foreach (var endpointDataSource in endpointDataSources)
        {
            services.AddSingleton(endpointDataSource);
        }
        
        var sp = services.BuildServiceProvider();

        var apiDescriptionGroupCollectionProvider = sp.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
        return Task.FromResult(apiDescriptionGroupCollectionProvider.ApiDescriptionGroups);
    }
    
    class DelDisposable : IDisposable
    {
        private readonly Action _action;

        public DelDisposable(Action action)
        {
            _action = action;
        }

        public static IDisposable Run(Action action)
        {
            return new DelDisposable(action);
        }
        
        public void Dispose()
        {
            _action();
        }
    }
    
    class TypeScriptStringWriter : CustomStringWriter
    {
        private readonly string _exceptionClassName;
        private bool _alreadyRenderedException;
        private bool _rendering;
        private int _closingBracketCount = 0;
        
        public TypeScriptStringWriter(StringBuilder sb, string exceptionClassName) : base(sb)
        {
            _exceptionClassName = exceptionClassName;
        }

        public bool SkipDuplicateExceptionTypes(string value)
        {
            if (value == $"export class {_exceptionClassName} extends Error {{")
            {
                _rendering = true;
            }
            
            using var _ = DelDisposable.Run(() =>
            {
                if (_rendering)
                {
                    if (value == "}")
                    {
                        _closingBracketCount++;
                    }

                    if (_closingBracketCount == 2)
                    {
                        _closingBracketCount = 0;
                        _rendering = false;
                        _alreadyRenderedException = true;
                    }
                }
            });
            
            if(_rendering && _alreadyRenderedException)
            {
                return true;
            }

            return false;
        }
        
        public override bool Skip(string value)
        {
            if (SkipDuplicateExceptionTypes(value))
            {
                return true;
            }

            return false;
        }
    }
    
    class CustomStringWriter : StringWriter
    {
        public CustomStringWriter(StringBuilder sb) : base(sb)
        {
            
        }

        public virtual bool Skip(string value)
        {
            return false;
        }

        public override void Write(string value)
        {
            if (Skip(value))
            {
                return;
            }
            base.Write(value);
        }
    }
}