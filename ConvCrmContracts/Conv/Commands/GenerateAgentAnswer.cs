
namespace ConvCrmContracts.Conv.Commands;

public class GenerateAgentAnswer : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public int ticket_id { get; set; }
    public Guid conversation_id { get; set; }
    public ConversationMessages message { get; set; } = default!;

    public GenerateAgentAnswer() { }

    public GenerateAgentAnswer(int ticket_id, Guid conversation_id, ConversationMessages message, DateTime? timestamp = null)
    {
        this.uuid = Guid.NewGuid();
        this.timestamp = timestamp ?? DateTime.Now;
        this.ticket_id = ticket_id;
        this.conversation_id = conversation_id;
        this.message = message;
    }
}
