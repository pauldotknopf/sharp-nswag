namespace Pivot.NetClient;

public interface IPivotClientGenerator
{
    T Generate<T>(HttpClient client);
}