namespace ConvCrmContracts.Common;

public class MinimalConversation
{
    public DateTime timestamp { get; set; }
    public string text { get; set; }
    public string sender { get; set; }

    public MinimalConversation(DateTime timestamp, string text, string sender)
    {
        this.timestamp = timestamp;
        this.text = text;
        this.sender = sender;
    }
}