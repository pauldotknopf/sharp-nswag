using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Pivotte.Services;

public class PivotteRouteParameterDefinition
{
    public int Index { get; set; }
    
    public string Name { get; set; }
    
    public BindingSource Source { get; set; }
    
    public Type Type { get; set; }
}