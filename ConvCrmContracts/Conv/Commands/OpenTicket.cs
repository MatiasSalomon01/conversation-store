namespace ConvCrmContracts.Conv.Commands;

public class OpenTicket : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public Guid conversation_id { get; set; }
    public string source_id { get; set; } = default!;
    public string channel { get; set; } = default!;
    public UserDetails? user_details { get; set; }
    public string last_message { get; set; } = default!;

    public OpenTicket() { }

    public OpenTicket(Guid conversationId, string sourceId, string channel, UserDetails? userDetails, string lastMessage)
    {
        uuid = Guid.NewGuid();
        timestamp = DateTime.Now;
        conversation_id = conversationId;
        source_id = sourceId;
        this.channel = channel;
        user_details = userDetails;
        last_message = lastMessage;
    }
}
