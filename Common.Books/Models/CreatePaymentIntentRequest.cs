namespace Common.Books.Models;

public class CreatePaymentIntentRequest
{
    public Guid BookRequestId { get; set; }
    public string Currency { get; set; }
}