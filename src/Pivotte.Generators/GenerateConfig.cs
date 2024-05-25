namespace Pivotte.Generators;

public class GenerateConfig
{
    public List<Type> Services { get; set; } = new List<Type>();

    public void AddService(Type service)
    {
        if (!Services.Contains(service))
        {
            Services.Add(service);
        }
    }

    public void AddService<T>()
    {
        AddService(typeof(T));
    }
}