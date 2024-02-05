using Common.Books.Configs;
using Common.Books.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SumoLogic;
using Serilog.Sinks.SystemConsole.Themes;

namespace Common.Books.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterConfig<T>(this IServiceCollection services, IConfiguration configuration)
        where T : KvConfig
    {
        var type = configuration.Get<T>();
        services.AddSingleton(type ?? throw new InvalidOperationException("Could not load configuration"));

        return services;
    }
    
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaConfig = configuration.GetSection("KafkaConfig").Get<KafkaConfig>() ?? throw new InvalidOperationException("Could not load configuration");
        services.AddSingleton(kafkaConfig);
        return services;
    }    
    
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("RedisConfig").Get<RedisConfig>() ?? throw new InvalidOperationException("Could not load configuration");
        services.AddSingleton(redisConfig);
        return services;
    }

    public static IServiceCollection AddStragixLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var appConfig = configuration.Get<AppConfig>();
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.SumoLogic("https://endpoint4.collection.sumologic.com/receiver/v1/http/ZaVnC4dhaV2Zbq_TV1agf7ev3q-rXYMb_rPgDhsUujQJM7ww7gMkkyHoPCldgEbYdn7IzSSnSm8R2KnieGsn713aY4H2x1rhfDpw0O7YMyV1BvSy-6Txpg==", $"{appConfig.Env}-{appConfig.AppName}")
            //.WriteTo.BetterStack("Gk8QPbp35q5TMQfHRL6iUVGZ")
            .WriteTo.Console(theme: SystemConsoleTheme.Literate);

        Log.Logger = loggerConfig.CreateLogger();

        services.AddSingleton<ILoggerProvider>(sp => new SerilogLoggerProvider(Log.Logger, true));
        services.AddSingleton(Log.Logger);

        return services;
    }

    public static IServiceCollection AddBooksDatabase(this IServiceCollection services, IConfiguration configuration,
        string connectionName = "DefaultConnection", ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var connectionString = configuration.GetConnectionString(connectionName) ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Could not find connection string named {connectionName}");
        
        services.AddDbContext<BooksDbContext>(options => { options.UseSqlServer(connectionString); }, serviceLifetime);
        return services;
    }
}