using Common.Books.Interfaces;
using Confluent.Kafka;
using Serilog;

namespace Common.Books.Helpers;

public static class WorkerHelper
{
    public static void InitializePlayWright(ILogger logger)
    {
        var playwrightCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cache", "ms-playwright");

        if (!Directory.Exists(playwrightCacheDir) || Directory.GetDirectories(playwrightCacheDir).Length == 0)
        {
            var exitCode = Microsoft.Playwright.Program.Main(new[] {"install"});
            // if (exitCode != 0)
            //     throw new Exception($"Playwright exited with code {exitCode}");
        }
        else
        {
            logger.Information("Playwright browsers are already installed");
        }
    }
    public static async Task RunWorkerAsync<T>(
        string workerName,
        IKafkaService kafkaService,
        ConsumerConfig config,
        string topic,
        Func<T, Task> handleMessageAsync,
        ILogger logger,
        CancellationToken stoppingToken,
        int delay = 5) where T : class
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await kafkaService.ConsumeAsync(config, topic, handleMessageAsync, stoppingToken);
                logger.Information("{WorkerName} Worker running at: {@Time}", workerName, DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromMinutes(delay), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when the token is cancelled, just break out of the loop
                break;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while consuming messages for worker {WorkerName}", workerName);
                // Delay before retrying
                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
        }
    }
}