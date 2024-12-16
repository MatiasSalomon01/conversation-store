using ConvCrmContracts.Crm.Events;

namespace WebApi.Integration.Events.Consumers;

public class TicketCreatedConsumer(MongoDBService mongo) : IConsumer<TicketCreated>
{
    public async Task Consume(ConsumeContext<TicketCreated> context)
    {
        var @event = context.Message;

        var ticketStateLog = new TicketStateLog(@event.ticket_id, new AgentSender("BOT"), @event.timestamp);

        var filter = Builders<Conversation>.Filter.Eq(conv => conv.id, @event.conversation_id);

        //Actualizar que esta manejado por el Agente y agregar el log
        var filterManagedBy = Builders<Conversation>.Update
            .Set(conv => conv.manage_by, ManageBy.Agent)
            .Push(x => x.logs, ticketStateLog);

        await mongo.Conversations.UpdateOneAsync(filter, filterManagedBy);

        // Filtro para elementos en el array `logs` donde `ticket_id` es `null`
        var arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument($"elem.{nameof(Log.ticket_id)}", BsonNull.Value))
            };

        //Actualizar el current_ticket_id y actualizar en todos los logs donde no tiene ticket_id, con el valor nuevo
        var updateTicketId = Builders<Conversation>.Update
        .Set(conv => conv.current_ticket_id, @event.ticket_id)
        .Set($"{nameof(Conversation.logs)}.$[elem].{nameof(Log.ticket_id)}", @event.ticket_id);

        await mongo.Conversations.UpdateOneAsync(filter, updateTicketId, new UpdateOptions { ArrayFilters = arrayFilters });
    }
}
