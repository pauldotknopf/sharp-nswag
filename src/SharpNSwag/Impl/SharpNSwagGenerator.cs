using System.Text;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.TypeScript;

namespace SharpNSwag.Impl;

public class SharpNSwagGenerator : ISharpNSwagGenerator
{
    private readonly ISharpNSwagBuilder _builder;

    public SharpNSwagGenerator(ISharpNSwagBuilder builder)
    {
        _builder = builder;
    }
    
    public async Task<string> GenerateClientTypeScript(Action<GenerateConfig> configure)
    {
        var config = new GenerateConfig();
        configure(config);
        
        var openApiDocs = new List<OpenApiDocument>();

        foreach (var controllerType in config.ControllerTypes)
        {
            openApiDocs.Add(await _builder.BuildOpenApiDoc(controllerType));
        }

        var sb = new StringBuilder();
        var sw = new TypeScriptStringWriter(sb, new TypeScriptClientGeneratorSettings().ExceptionClass);
        
        // let's generate all the type definitions first.
        {
            var doc = new OpenApiDocument();
            foreach (var serviceDoc in openApiDocs)
            {
                foreach (var definition in serviceDoc.Definitions)
                {
                    doc.Definitions[definition.Key] = definition.Value;
                }
            }

            var generator = new TypeScriptClientGenerator(doc, new TypeScriptClientGeneratorSettings());
            foreach (var line in generator.GenerateFile(ClientGeneratorOutputType.Contracts).Split(Environment.NewLine))
            {
                await sw.WriteLineAsync(line);
            }
        }

        var index = 0;
        foreach (var serviceDoc in openApiDocs)
        {
            var settings = new TypeScriptClientGeneratorSettings
            {
                ClassName = serviceDoc.Info.Title
            };
            var generator = new TypeScriptClientGenerator(serviceDoc, settings);
            foreach (var line in generator.GenerateFile(ClientGeneratorOutputType.Implementation).Split(Environment.NewLine))
            {
                await sw.WriteLineAsync(line);
            }
        }
        await sw.DisposeAsync();
        return sb.ToString();
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
    
    class TypeScriptStringWriter : CustomStringWriter
    {
        private readonly StringBuilder _sb;
        private readonly string _exceptionClassName;

        public TypeScriptStringWriter(StringBuilder sb, string exceptionClassName): base(sb)
        {
            _sb = sb;
            _exceptionClassName = exceptionClassName;
        }
        
        private bool _alreadyRenderedException;
        private bool _rendering;
        private int _closingBracketCount = 0;

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
    
    class DelDisposable : IDisposable
    {
        private readonly Action _del;

        public DelDisposable(Action del)
        {
            _del = del;
        }
        
        public static IDisposable Run(Action action)
        {
            return new DelDisposable(action);
        }
        
        public void Dispose()
        {
            _del();
        }
    }
}