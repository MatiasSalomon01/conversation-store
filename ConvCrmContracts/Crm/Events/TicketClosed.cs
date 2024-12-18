namespace ConvCrmContracts.Crm.Events;

public class TicketClosed : TicketOpened
{
    public bool? was_automatic_changed { get; set; }
    public string? automatic_changed_id { get; set; }
}
