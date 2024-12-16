namespace WebApi.Schemas;

public class AgentSender(string displayName, string? agentId = null)
{
    public string display_name { get; set; } = displayName;
    public string? agent_id { get; set; } = agentId;
}