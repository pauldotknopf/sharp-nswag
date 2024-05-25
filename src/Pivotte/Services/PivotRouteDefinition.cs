using System.Reflection;

namespace Pivotte.Services;

public class PivotRouteDefinition
{
    public Type ServiceType { get; set; }
    
    public MethodInfo MethodInfo { get; set; }
    
    public string Route { get; set; }
    
    public int? Order { get; set; }
    
    public string Verb { get; set; }
    
    public List<PivotRouteParameterDefinition> Parameters { get; set; }
}