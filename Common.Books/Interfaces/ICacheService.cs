namespace Common.Books.Interfaces;

public interface ICacheService
{
    void Set<T>(string key, T value)
        where T : class;
    void Set(string key, string value);
    T? Get<T>(string key)
        where T : class;
    string? Get(string key);
    void Remove(string key);
    void Clear(string pattern = "*");
    Dictionary<string, string> GetAll();
}