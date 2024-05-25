namespace Pivotte;

[AttributeUsage(AttributeTargets.Interface)]
public class PivotServiceAttribute : Attribute
{
    public PivotServiceAttribute(string name)
    {
        Name = name;
    }
    
    public string Name { get; }
}