namespace ConvCrmContracts.Common;

public class AuthorizedBy
{
    public string agent_id { get; set; } = default!;
    public string agent_name { get; set; } = default!;

    public AuthorizedBy() { }

    public AuthorizedBy(string agentId, string agentName)
    {
        this.agent_id = agentId;
        this.agent_name = agentName;
    }
}
