namespace Pivotte;

[AttributeUsage(AttributeTargets.Interface)]
public class PivotteServiceAttribute : Attribute
{
    public PivotteServiceAttribute(string name)
    {
        Name = name;
    }
    
    public string Name { get; }
}