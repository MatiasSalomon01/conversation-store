
namespace ConvCrmContracts.Conv.Commands;

public class SendWABATextMessage : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public string sender { get; set; }
    public string receiver { get; set; }
    public string body { get; set; }

    public SendWABATextMessage(Guid uuid, DateTime timestamp, string sender, string receiver, string body)
    {
        this.uuid = uuid;
        this.timestamp = timestamp;
        this.sender = sender;
        this.receiver = receiver;
        this.body = body;
    }
}