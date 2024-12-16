using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace ConvCrmContracts.Crm.Events;

public class ConversationDocument
{
    public Guid id { get; set; }
    public int? ticket_id { get; set; }
    public string? codigo_cliente { get; set; }
    public string active_channel { get; set; }
    public string source_id { get; set; }
    public UserDetails? user_details { get; set; }
    public List<ConversationMessages>? messages { get; set; } = [];

    //public ConversationDocument() { }

    public ConversationDocument(Guid id, int? ticket_id, string? codigo_cliente, string active_channel, string source_id, UserDetails? user_details)
    {
        this.id = id;
        this.ticket_id = ticket_id;
        this.codigo_cliente = codigo_cliente;
        this.active_channel = active_channel;
        this.source_id = source_id;
        this.user_details = user_details;
    }

    public void SetMessages(List<ConversationMessages> messages)
    {
        this.messages = messages;
    }
}