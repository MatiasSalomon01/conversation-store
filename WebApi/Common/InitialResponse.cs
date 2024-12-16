using WebApi.Constants;

namespace WebApi.Common;

public class InitialResponse
{
    public string Name { get; set; } = default!;
    public bool IsRAG() => Name.Equals(Constant.RAG, StringComparison.OrdinalIgnoreCase);
    public bool IsLlama() => Name.Equals(Constant.Llama, StringComparison.OrdinalIgnoreCase);
    public bool IsAgent() => Name.Equals(Constant.Agent, StringComparison.OrdinalIgnoreCase);
}
