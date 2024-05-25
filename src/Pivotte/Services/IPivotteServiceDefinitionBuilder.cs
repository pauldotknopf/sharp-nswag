namespace Pivotte.Services;

public interface IPivotteServiceDefinitionBuilder
{
    PivotteServiceDefinition BuildServiceDefinition<T>();
    
    PivotteServiceDefinition BuildServiceDefinition(Type type);
}