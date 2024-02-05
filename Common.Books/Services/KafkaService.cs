using Common.Books.Interfaces;
using Confluent.Kafka;
using Newtonsoft.Json;
using Polly;
using Serilog;

namespace Common.Books.Services;

public class KafkaService : IKafkaService
{
    private readonly ILogger _logger;
    private readonly ProducerConfig _producerConfig;

    public KafkaService(string bootstrapServers, ILogger logger)
    {
        _logger = logger;
        _producerConfig = new() {BootstrapServers = bootstrapServers};
    }

    private static IAsyncPolicy RetryPolicy =>
        Policy.Handle<Exception>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8)
            });

    public async Task ProduceAsync(string topic, string key, string message)
    {
        try
        {
            using var producer = new ProducerBuilder<string, string>(_producerConfig)
                .SetErrorHandler((producer, error) =>
                {
                    _logger.Error("Error while producing message for {ProducerName} {Reason}", producer.Name,
                        error.Reason);
                }).SetLogHandler((s, msg) =>
                {
                    _logger.Information("Log from Kafka: {Level} {Facility} {Name} {Message}", msg.Level, msg.Facility,
                        msg.Name, msg.Message);
                })
                .Build();
            await RetryPolicy.ExecuteAsync(() =>
                producer.ProduceAsync(topic, new Message<string, string> {Key = key, Value = message}));
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while producing message");
        }
    }

    public async Task ProduceAsync<T>(string topic, string key, T message) where T : class
    {
        try
        {
            using var producer = new ProducerBuilder<string, string>(_producerConfig)
                .SetErrorHandler((producer, error) =>
                {
                    _logger.Error("Error while producing message for {ProducerName} {Reason}", producer.Name,
                        error.Reason);
                }).SetLogHandler((s, msg) =>
                {
                    _logger.Information("Log from Kafka: {Level} {Facility} {Name} {Message}", msg.Level, msg.Facility,
                        msg.Name, msg.Message);
                })
                .Build();
            var json = JsonConvert.SerializeObject(message);
            await RetryPolicy.ExecuteAsync(() =>
                producer.ProduceAsync(topic, new Message<string, string> {Key = key, Value = json}));
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while producing message");
        }
    }

    public async Task ConsumeAsync<T>(ConsumerConfig consumerConfig, string topic, Func<T, Task> handleMessageAsync,
        CancellationToken cancellationToken = default) where T : class
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
            .SetErrorHandler((consumer, error) =>
            {
                _logger.Error("Error while consuming message for {ConsumerName} {Reason}", consumer.Name,
                    error.Reason);
            }).SetLogHandler((s, msg) =>
            {
                _logger.Information("Log from Kafka: {Level} {Facility} {Name} {Message}", msg.Level, msg.Facility,
                    msg.Name, msg.Message);
            })
            .Build();
        try
        {
            consumer.Subscribe(topic);

            while (true)
            {
                var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5)); // 5-second timeout
                if (consumeResult != null)
                {
                    // Process message
                    var message = JsonConvert.DeserializeObject<T>(consumeResult.Message.Value);
                    if (message == null) continue;

                    await RetryPolicy.ExecuteAsync(() => handleMessageAsync(message));
                    consumer.Commit(consumeResult);
                }
                else
                {
                    //logger.Information("No message received from topic {Topic}", topic);
                }
            }
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.Information(operationCanceledException, "Kafka consumer cancelled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error processing message: {Message}", e.Message);
        }
        finally
        {
            consumer.Close();
        }
    }

    public async Task ConsumeAsync(ConsumerConfig consumerConfig, string topic, Func<string, Task> handleMessageAsync)
    {
        _logger.Information("Starting Kafka Consumer for topic {Topic}", topic);

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).SetErrorHandler((consumer, error) =>
        {
            _logger.Error("Error while consuming message for {ConsumerName} {Reason}", consumer.Name,
                error.Reason);
        }).SetLogHandler((s, msg) =>
        {
            _logger.Information("Log from Kafka: {Level} {Facility} {Name} {Message}", msg.Level, msg.Facility,
                msg.Name, msg.Message);
        }).Build();


        try
        {
            consumer.Subscribe(topic);
            while (true)
            {
                var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5)); // 5-second timeout
                if (consumeResult != null)
                {
                    await RetryPolicy.ExecuteAsync(() => handleMessageAsync(consumeResult.Message.Value));
                    consumer.Commit(consumeResult);
                }
                else
                {
                    _logger.Information("No message received from topic {Topic}", topic);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Kafka consumer cancelled");
            consumer.Close();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error processing message: {Message}", e.Message);
        }
        finally
        {
            consumer.Close();
        }
    }
}