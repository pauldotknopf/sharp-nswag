namespace ProtoPivot.NetClient;

public interface IPivotClientGenerator
{
    T Generate<T>(HttpClient client);
}