using Common.Books.Configs;
using Common.Books.Interfaces;
using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;

namespace Common.Books.Services;

public class CacheService : ICacheService
{
    private readonly ILogger _logger;
    private static IDatabase? _db;
    private static ConnectionMultiplexer? _redis;

    public CacheService(ILogger logger, RedisConfig redisConfig)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
        _db = _redis.GetDatabase();
        if (_db == null)
            throw new Exception("Failed to connect to redis");
    }

    public void Set<T>(string key, T value)
        where T : class
    {
        var serialized = JsonConvert.SerializeObject(value);
        _logger.Information("Setting {Key} to {@Value}", key, serialized);
        _db!.StringSet(key, serialized);
    }

    public void Set(string key, string value)
    {
        _logger.Information("Setting {Key} to {Value}", key, value);
        _db!.StringSet(key, value);
    }

    public T? Get<T>(string key)
        where T : class
    {
        var result = _db!.StringGet(key);
        if (result.IsNull)
            return null;

        var deserialized = JsonConvert.DeserializeObject<T>(result!);
        _logger.Information("Getting {Key} with value {@Value}", key, deserialized);
        return deserialized;
    }

    public string? Get(string key)
    {
        var result = _db!.StringGet(key);
        if (result.IsNull)
            return null;

        _logger.Information("Getting {Key} with value {Value}", key, result);
        return result;
    }

    public void Remove(string key)
    {
        _logger.Information("Removing {Key}", key);
        _db!.KeyDelete(key);
    }

    public void Clear(string pattern = "*")
    {
        _logger.Information("Clearing cache");
        _db!.KeyDelete(pattern);
        _logger.Information("Cache cleared");
    }

    public Dictionary<string, string> GetAll()
    {
        var keys = _redis.GetServer("localhost", 6379).Keys();

        string[] keysArr = keys.Select(key => key.ToString()).ToArray();

        Dictionary<string, string> values = new();
        foreach (string key in keysArr)
        {
            values.Add(key, _db.StringGet(key));
            Console.WriteLine(_db.StringGet(key));
        }

        return values;
    }
}