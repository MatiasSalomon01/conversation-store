namespace ConvCrmContracts.Crm.Events;

public class TicketsPreviews
{
    public int ticket_id { get; set; }
    public string text { get; set; } = string.Empty;
    public string? display_name { get; set; }
    public DateTime timestamp { get; set; }
    public int unseen_messages_amount { get; set; }

    public TicketsPreviews() { }

    public TicketsPreviews(int ticket_id, string text, string? display_name, DateTime timestamp, int unseen_messages_amount)
    {
        this.ticket_id = ticket_id;
        this.text = text;
        this.display_name = display_name;
        this.timestamp = timestamp;
        this.unseen_messages_amount = unseen_messages_amount;
    }
}