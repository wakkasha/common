using Common.Books.Data.Enums;

namespace Common.Books.Data.Entities;

public class Communication: BaseEntity
{
    public CommunicationType Type { get; set; }
    public string? ToEmail { get; set; }
    public string? FromEmail { get; set; }
    public string? ToPhone { get; set; }
    public string? FromPhone { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ReadDate { get; set; }
}