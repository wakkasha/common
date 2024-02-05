namespace Common.Books.Interfaces;

public interface IQueueService
{
    Task EnqueueAsync(string connectionString, string queueName, string message);
}