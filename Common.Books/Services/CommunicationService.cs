using Common.Books.Configs;
using Common.Books.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Common.Books.Services;

public class CommunicationService : ICommunicationService
{
    private readonly ILogger _logger;
    private readonly CommunicationConfig _communicationConfig;
    private readonly string _apiKey;
    private readonly EmailAddress _from;
    private readonly string _defaultFromEmail = "swalker@stragixinnovations.com";
    private readonly string _defaultFromName = "Shawn Walker";

    public CommunicationService(CommunicationConfig communicationConfig, ILogger logger)
    {
        _logger = logger;
        _communicationConfig = communicationConfig;
        _apiKey = communicationConfig.SendGridApiKey;
        _from = new EmailAddress(_defaultFromEmail, _defaultFromName);
    }

    public async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string content,
        EmailAddress? from = null,
        CancellationToken cancellationToken = default
    )
    {
        var useDefaults = from == null;
        if (useDefaults)
            from = _from;

        var client = new SendGridClient(_apiKey);
        var to = new EmailAddress(toEmail, toName);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
        var response = await client.SendEmailAsync(msg, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("Failed to send email to {ToEmail}", toEmail);
            throw new Exception($"Failed to send email to {toEmail}");
        }
    }

    public async Task SendSmsAsync(string toPhoneNumber, string message)
    {
        var accountSid = _communicationConfig.TwilioAccountSid;
        var authToken = _communicationConfig.TwilioAuthToken;
        TwilioClient.Init(accountSid, authToken);

        var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
        {
            From = new PhoneNumber(_communicationConfig.TwilioPhoneNumber),
            Body = message
        };

        var response = await MessageResource.CreateAsync(messageOptions);

        if (response.ErrorCode != null)
        {
            _logger.Error("Failed to send SMS to {ToPhoneNumber}", toPhoneNumber);
            throw new Exception($"Failed to send SMS to {toPhoneNumber}");
        }
    }
}