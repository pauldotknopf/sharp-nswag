namespace Pivot.Services;

public interface IPivotServiceDefinitionBuilder
{
    PivotServiceDefinition BuildServiceDefinition<T>();
}