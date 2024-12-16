namespace WebApi.Integration.Events.Consumers;

/// <summary>
/// Consumidor del evento MessageViewed.
/// Escucha los eventos de mensajes vistos y procesa la información correspondiente.
/// </summary>
public class MessageViewedConsumer(MongoDBService mongo, ILogger<MessageViewedConsumer> logger) : IConsumer<MessageViewed>
{
    public async Task Consume(ConsumeContext<MessageViewed> context)
    {
        var @event = context.Message;

        logger.LogInformation("Evento MessageViewed recibido: {uuid}", @event.uuid);
        logger.LogDebug("Conversacion {conversation_id}", @event.conversation_id);

        // Buscar la conversación y el mensaje
        // TODO Refactor 
        var filter = Builders<Conversation>.Filter.Eq(c => c.id, @event.conversation_id) &
                     Builders<Conversation>.Filter.ElemMatch(c => c.logs, log => log.ticket_id == @event.ticket_id);

        //Recuperar solamente el array de logs
        var projection = Builders<Conversation>.Projection.Expression(c => c.logs.Where(log => log is WhatsAppTextLog));

        var logs = (await mongo.Conversations
            .Find(filter)
            .Project(projection)
            .FirstOrDefaultAsync())
            .OfType<WhatsAppTextLog>()
            .Where(x => x.was_viewed_at == null)
            .ToList();

        //Si no hay logs sin leer, terminar proceso
        if (logs.Count == 0)
        {
            logger.LogError("Todos los mensajes ya se encuentran estan leidos");
            return;
        }

        var viewedBy = new Schemas.Logs.WasViewedBy
        {
            agent_crm_id = @event.was_viewed_by.agent_crm_id,
            agent_keycloak_id = @event.was_viewed_by.agent_keycloak_id,
            agent_name = @event.was_viewed_by.agent_name
        };

        //// Update the conversation with the new message
        //// TODO Refactor this into proper repository pattern
        var arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument(
                        $"elem.{nameof(Log.log_id)}", new BsonDocument("$in", new BsonArray(logs.Select(x => x.log_id.ToString())))))
            };

        var update = Builders<Conversation>.Update
        .Set($"{nameof(Conversation.logs)}.$[elem].was_viewed_at", @event.was_viewed_at)
        .Set($"{nameof(Conversation.logs)}.$[elem].was_viewed_by", viewedBy);
        
        await mongo.Conversations.UpdateOneAsync(
            filter, 
            update,
            new UpdateOptions { ArrayFilters = arrayFilters });

        logger.LogDebug("Mensaje actualizado");
    }
}
