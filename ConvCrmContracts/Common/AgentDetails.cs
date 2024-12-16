namespace ConvCrmContracts.Common;

public class AgentDetails
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;

    public AgentDetails() { }

    public AgentDetails(string id, string name)
    {
        this.id = id;
        this.name = name;
    }
}