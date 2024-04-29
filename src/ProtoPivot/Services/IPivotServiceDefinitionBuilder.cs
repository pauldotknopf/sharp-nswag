namespace ProtoPivot.Services;

public interface IPivotServiceDefinitionBuilder
{
    PivotServiceDefinition BuildServiceDefinition<T>();
}