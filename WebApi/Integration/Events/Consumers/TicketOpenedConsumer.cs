namespace WebApi.Integration.Events.Consumers;

public class TicketOpenedConsumer(MongoDBService mongo) : IConsumer<TicketOpened>
{
    public async Task Consume(ConsumeContext<TicketOpened> context)
    {
        var @event = context.Message;

        var sender = new AgentSender(@event.authorized_by.agent_name, @event.authorized_by.agent_id);
        var ticketStateLog = new TicketStateLog(@event.ticket_id, sender, @event.timestamp, "open");

        var filter = Builders<Conversation>.Filter.Eq(conv => conv.id, @event.conversation_id);

        var existsConversation = await mongo.Conversations.Find(filter).AnyAsync();

        if (!existsConversation) return; //TODO: agregar log en consola

        var filterUpdate = Builders<Conversation>.Update.Push(conv => conv.logs, ticketStateLog);

        await mongo.Conversations.UpdateOneAsync(filter, filterUpdate);
    }
}
