using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ProtoPivot.Services;

public class PivotRouteParameterDefinition
{
    public string Name { get; set; }
    
    public BindingSource Source { get; set; }
    
    public Type Type { get; set; }
}