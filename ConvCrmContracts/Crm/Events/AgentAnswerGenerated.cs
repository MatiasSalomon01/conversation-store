namespace ConvCrmContracts.Crm.Events;

public class AgentAnswerGenerated : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public string agent_id { get; set; } = default!;
    public AgentDetails agent_details { get; set; } = new();
    public int ticket_id { get; set; }
    public Guid conversation_id { get; set; }
    public string body { get; set; } = default!;

    public AgentAnswerGenerated() { }

    public AgentAnswerGenerated(string agent_id, AgentDetails agent_details, int ticket_id, Guid conversation_id, string body)
    {
        this.uuid = Guid.NewGuid();
        this.timestamp = DateTime.Now;
        this.agent_id = agent_id;
        this.agent_details = agent_details;
        this.ticket_id = ticket_id;
        this.conversation_id = conversation_id;
        this.body = body;
    }
}
