namespace ConvCrmContracts.Crm.Events;

public class GetTicketsPreviewsDocument
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public List<TicketsPreviews>? previews { get; set; } = [];

    public GetTicketsPreviewsDocument(Guid uuid, DateTime timestamp)
    {
        this.uuid = uuid;
        this.timestamp = timestamp;
    }

    public void SetPreviews(List<TicketsPreviews> previews)
    {
        this.previews = previews;
    }
}