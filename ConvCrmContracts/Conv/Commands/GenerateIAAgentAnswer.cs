namespace ConvCrmContracts.Conv.Commands;

public class GenerateIAAgentAnswer : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public Guid conversation_id { get; set; }
    public List<MinimalConversation> messages { get; set; }
    public List<string> tags { get; set; } = [];

    public GenerateIAAgentAnswer(Guid uuid, Guid conversation_id, DateTime timestamp, List<MinimalConversation> messages)
    {
        this.uuid = uuid;
        this.conversation_id = conversation_id;
        this.timestamp = timestamp;
        this.messages = messages;
    }
}