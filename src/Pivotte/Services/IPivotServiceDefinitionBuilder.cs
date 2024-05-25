namespace Pivotte.Services;

public interface IPivotServiceDefinitionBuilder
{
    PivotServiceDefinition BuildServiceDefinition<T>();
    
    PivotServiceDefinition BuildServiceDefinition(Type type);
}