using System.Text;
using Azure.Storage.Queues;
using Common.Books.Interfaces;

namespace Common.Books.Services;

public class QueueService: IQueueService
{
    public async Task EnqueueAsync(string connectionString, string queueName, string message)
    {
        await AddMessageToQueueAsync(connectionString, queueName, message);
    }

    private async Task AddMessageToQueueAsync(string connectionString, string queueName, string message)
    {
        var queueClient = new QueueClient(connectionString, queueName);
        await queueClient.CreateIfNotExistsAsync();

        await queueClient.SendMessageAsync(Base64Encode(message));
    }

    private string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
}