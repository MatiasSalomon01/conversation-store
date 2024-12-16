namespace ConvCrmContracts.Conv.Querys;

public class GetTicketsPreviews : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public List<int> tickets_ids { get; set; } = [];
}