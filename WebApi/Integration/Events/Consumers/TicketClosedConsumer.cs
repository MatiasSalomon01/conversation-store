namespace WebApi.Integration.Events.Consumers;

public class TicketClosedConsumer(MongoDBService mongo) : IConsumer<TicketClosed>
{
    public async Task Consume(ConsumeContext<TicketClosed> context)
    {
        var @event = context.Message;

        var sender = new AgentSender(@event.authorized_by.agent_name, @event.authorized_by.agent_id);
        var ticketStateLog = new TicketStateLog(@event.ticket_id, sender, @event.timestamp, "closed", @event.was_automatic_changed, @event.automatic_changed_id);

        var filter = Builders<Conversation>.Filter.Eq(conv => conv.id, @event.conversation_id);

        var existsConversation = await mongo.Conversations.Find(filter).AnyAsync();

        if (!existsConversation) return; //TODO: agregar log en consola

        //Una vez cerrado, cambiar de manage_by de Agent a AIAgent para dar culminacion al ticket, de esa forma el siguiente mensaje entrante se redirigira a la AI
        var filterUpdate = Builders<Conversation>.Update
            .Set(conv => conv.manage_by, ManageBy.AIAgent)
            .Set(conv => conv.current_ticket_id, null)
            .Push(conv => conv.logs, ticketStateLog);

        await mongo.Conversations.UpdateOneAsync(filter, filterUpdate);

        if (@event.was_automatic_changed ?? false)
        {
            var arrayFilter = @event.ticket_id != 0 
                ? new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument($"elem.{nameof(Log.ticket_id)}", @event.ticket_id))
                : new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument($"elem.{nameof(Log.ticket_id)}", BsonNull.Value));
           
            var arrayFilters = new List<ArrayFilterDefinition> { arrayFilter };

            //Actualizar el current_ticket_id y actualizar en todos los logs donde no tiene ticket_id, con el valor nuevo
            var updateTicketId = Builders<Conversation>.Update
            .Set($"{nameof(Conversation.logs)}.$[elem].{nameof(Log.automatic_changed_id)}", @event.automatic_changed_id.ToString())
            .Set($"{nameof(Conversation.logs)}.$[elem].{nameof(Log.ticket_id)}", @event.ticket_id);

            await mongo.Conversations.UpdateOneAsync(filter, updateTicketId, new UpdateOptions { ArrayFilters = arrayFilters });
        }
    }
}
