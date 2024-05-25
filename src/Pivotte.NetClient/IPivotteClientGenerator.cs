namespace Pivotte.NetClient;

public interface IPivotteClientGenerator
{
    T Generate<T>(HttpClient client);
}