namespace Pivotte.NetClient;

public interface IPivotClientGenerator
{
    T Generate<T>(HttpClient client);
}