using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Pivotte.Services;

public interface IPivotteServiceDefinitionBuilder
{
    PivotteServiceDefinition BuildServiceDefinition<T>();
    
    PivotteServiceDefinition BuildServiceDefinition(Type type);
    
    List<ApiDescription> BuildApiDescriptions(Type type);
}