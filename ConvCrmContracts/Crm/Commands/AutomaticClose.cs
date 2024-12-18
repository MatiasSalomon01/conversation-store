namespace ConvCrmContracts.Crm.Commands;

public class AutomaticClose
{
    public int Id { get; set; }
    public AutomaticCloseTypification Typification { get; set; } = default!;
}

public class AutomaticCloseTypification
{
    public TypificationData? Type { get; set; }
    public TypificationData? SubType { get; set; }
    public TypificationData? Typification { get; set; }
    public string? Description { get; set; }
}

public record TypificationData(int Value, string Label);