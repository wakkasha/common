using SendGrid.Helpers.Mail;

namespace Common.Books.Interfaces;

public interface ICommunicationService
{
    Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string content,
        EmailAddress? from = null,
        CancellationToken cancellationToken = default
    );

    Task SendSmsAsync(string toPhoneNumber, string message);
}