namespace ConvCrmContracts.Crm.Events;

public class TicketStateChanged : IBaseEvent
{
    public string type { get; set; } = default!;
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public Guid conversation_id { get; set; }
    public int ticket_id { get; set; }
    public string new_state { get; set; } = default!;
    public AuthorizedBy authorized_by { get; set; } = new();
}
