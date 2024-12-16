using ConvCrmContracts.Conv.Querys;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;

namespace WebApi.Integration.Events.Consumers;

public class GetConversationConsumer(MongoDBService mongo, ILogger<GetConversationConsumer> logger) : IConsumer<GetConversation>
{
    public async Task Consume(ConsumeContext<GetConversation> context)
    {
        var @event = context.Message;

        logger.LogInformation("El id a consultar es: {Message}", @event.conversation_id);
        logger.LogInformation("Fecha Inicio: {inicio}", @event.start_timestamp);
        logger.LogInformation("Fecha Fin: {fin}", @event.end_timestamp);

        var filter = Builders<Conversation>.Filter.Eq(conv => conv.id, @event.conversation_id);
        var conversation = await mongo.Conversations.Find(filter).FirstOrDefaultAsync();

        //Extraer el ticketId si es que posee alguno, sino, se queda en null
        var ticketId = conversation.logs.FirstOrDefault(x => x.ticket_id == @event.ticket_id)?.ticket_id;

        var conversationDocument = new ConversationDocument(conversation.id, ticketId, conversation.codigo_cliente, conversation.active_channel.ToString(), conversation.source_id, conversation.user_details);

        //Filtrar mensajes
        List<Log> extractedLogs = conversation.logs
            .Where(x => x.ticket_id == @event.ticket_id)
            .WhereNotNull(@event.start_timestamp, x => x.timestamp >= @event.start_timestamp)
            .WhereNotNull(@event.end_timestamp, x => x.timestamp <= @event.end_timestamp)
            .ToList();

        //Extraer los mensajes del User (de whatsapp), IA y del Agente
        var ai = extractedLogs.OfType<AIAgentTextLog>().Select(x => new ConversationMessages(x.log_id, x.sender.agent_id, x.sender.display_name, x.fallback_text, x.timestamp, ticketId));
        var agent = extractedLogs.OfType<AgentTextLog>().Select(x => new ConversationMessages(x.log_id, x.sender.agent_id, x.sender.display_name, x.fallback_text, x.timestamp, ticketId));
        var user = extractedLogs.OfType<WhatsAppTextLog>().Select(x => new ConversationMessages(x.log_id, null, x.sender.display_name ?? conversationDocument.source_id, x.fallback_text, x.timestamp, ticketId, false));

        //Juntar y ordenar mensajes
        var messages = ai.Concat(agent).Concat(user)
            .OrderBy(x => x.time)
            .ToList();

        conversationDocument.SetMessages(messages);

        //Responder de esta forma ya que se utiliza con el request/response pattern
        await context.RespondAsync(conversationDocument);

        logger.LogInformation("Resultado de la consulta: {Resultado}", JsonSerializer.Serialize(conversationDocument, new JsonSerializerOptions { WriteIndented = true }));
    }
}
