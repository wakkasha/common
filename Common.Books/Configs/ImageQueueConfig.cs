namespace Common.Books.Configs;

public class ImageQueueConfig : KvConfig
{
    public string ConnectionString { get; set; }
    public string QueueName { get; set; }
}