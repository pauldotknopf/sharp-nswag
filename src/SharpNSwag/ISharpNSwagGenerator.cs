namespace SharpNSwag;

public interface ISharpNSwagGenerator
{
    Task<string> GenerateClientTypeScript(Action<GenerateConfig> configure);
}