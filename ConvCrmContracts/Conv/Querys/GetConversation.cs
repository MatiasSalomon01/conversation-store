namespace ConvCrmContracts.Conv.Querys;

public class GetConversation : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public int ticket_id { get; set; }
    public Guid conversation_id { get; set; }
    public DateTime? start_timestamp { get; set; }
    public DateTime? end_timestamp { get; set; }

    public GetConversation() { }

    public GetConversation(int ticket_id, Guid conversation_id)
    {
        this.uuid = Guid.NewGuid();
        this.timestamp = timestamp;
        this.ticket_id = ticket_id;
        this.conversation_id = conversation_id;
    }
}