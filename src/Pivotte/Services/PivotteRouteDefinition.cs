using System.Reflection;

namespace Pivotte.Services;

public class PivotteRouteDefinition
{
    public Type ServiceType { get; set; }
    
    public MethodInfo MethodInfo { get; set; }
    
    public string Route { get; set; }
    
    public int? Order { get; set; }
    
    public string Verb { get; set; }
    
    public List<PivotteRouteParameterDefinition> Parameters { get; set; }
}