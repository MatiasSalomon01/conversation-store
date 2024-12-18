namespace ConvCrmContracts.Conv.Commands;

public class NewActivityCountdown : IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
    public Guid conversation_id { get; set; }
    public int? ticket_id { get; set; }
}
