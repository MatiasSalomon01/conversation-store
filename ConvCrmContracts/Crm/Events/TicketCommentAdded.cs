namespace ConvCrmContracts.Crm.Events;

public class TicketCommentAdded : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public int ticket_id { get; set; }
    public Guid conversation_id { get; set; }
    public string comment { get; set; } = default!;
    public AuthorizedBy authorized_by { get; set; } = new();
}
