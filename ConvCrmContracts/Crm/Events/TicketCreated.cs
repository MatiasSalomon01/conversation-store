namespace ConvCrmContracts.Crm.Events;

public class TicketCreated : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public int ticket_id { get; set; }
    public Guid conversation_id { get; set; }
}
