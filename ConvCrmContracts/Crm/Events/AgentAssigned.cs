namespace ConvCrmContracts.Crm.Events;

public class AgentAssigned : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public int ticket_id { get; set; }
    public Guid conversation_id { get; set; }
    public AuthorizedBy agent_assigned { get; set; } = new();
    public AuthorizedBy authorized_by { get; set; } = new();
}
