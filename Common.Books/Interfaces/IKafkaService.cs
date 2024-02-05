using Confluent.Kafka;

namespace Common.Books.Interfaces;

public interface IKafkaService
{
    Task ProduceAsync(string topic, string key, string message);
    Task ConsumeAsync(ConsumerConfig consumerConfig, string topic, Func<string, Task> handleMessageAsync);

    Task ProduceAsync<T>(string topic, string key, T message) where T : class;

    Task ConsumeAsync<T>(ConsumerConfig consumerConfig, string topic, Func<T, Task> handleMessageAsync,
        CancellationToken cancellationToken = default) where T : class;
}