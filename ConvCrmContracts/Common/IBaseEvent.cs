namespace ConvCrmContracts.Common;

public interface IBaseEvent
{
    public Guid uuid { get; set; }
    public DateTime timestamp { get; set; }
}
