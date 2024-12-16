using WebApi.Enums;

namespace WebApi.Schemas;

public class Conversation
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid id { get; private set; }
    public string? codigo_cliente { get; private set; }

    [BsonRepresentation(BsonType.String)]
    public Channel active_channel { get; private set; }
    public string source_id { get; private set; }

    [BsonRepresentation(BsonType.String)]
    public ManageBy manage_by { get; set; } = ManageBy.AIAgent;
    public UserDetails? user_details { get; private set; }
    public int? current_ticket_id { get; set; }
    public List<Log> logs { get; set; } = [];

    public Conversation(string sourceId, Log log, Channel activeChannel = Channel.WhatsApp, string? codigoCliente = null, UserDetails? userDetails = null)
    {
        id = Guid.NewGuid();
        codigo_cliente = codigoCliente;
        active_channel = activeChannel;
        source_id = sourceId;
        user_details = userDetails;
        logs.Add(log);
    }
}