namespace Common.Books.Configs;

public class CommunicationConfig : KvConfig
{
    public string TwilioAccountSid { get; set; }
    public string TwilioAuthToken { get; set; }
    public string TwilioPhoneNumber { get; set; }
    public string SendGridApiKey { get; set; }
}