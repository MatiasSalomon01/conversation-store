namespace ConvCrmContracts.Crm.Events;

public class TicketOpened : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public int ticket_id { get; set; }
    public Guid conversation_id { get; set; }
    public AuthorizedBy authorized_by { get; set; }

    public TicketOpened() { }

    public TicketOpened(int ticketId, Guid conversationId, AuthorizedBy authorizedBy)
    {
        this.ticket_id = ticketId;
        this.conversation_id = conversationId;
        this.authorized_by = authorizedBy;
    }
}
