namespace SharpNSwag;

public class GenerateConfig
{
    public List<Type> ControllerTypes { get; set; } = new List<Type>();

    public void AddController(Type service)
    {
        if (!ControllerTypes.Contains(service))
        {
            ControllerTypes.Add(service);
        }
    }

    public void AddController<T>()
    {
        AddController(typeof(T));
    }
}