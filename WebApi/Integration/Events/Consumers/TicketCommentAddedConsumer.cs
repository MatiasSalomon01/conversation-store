namespace WebApi.Integration.Events.Consumers;

public class TicketCommentAddedConsumer(MongoDBService mongo) : IConsumer<TicketCommentAdded>
{
    public async Task Consume(ConsumeContext<TicketCommentAdded> context)
    {
        var @event = context.Message;

        var sender = new AgentSender(@event.authorized_by.agent_name, @event.authorized_by.agent_id.ToString());

        var comment = new CommentLog(@event.comment, sender, @event.timestamp, @event.ticket_id);

        var filter = Builders<Conversation>.Filter.Eq(conv => conv.id, @event.conversation_id);

        var existsConversation = await mongo.Conversations.Find(filter).AnyAsync();

        if (!existsConversation) return; //TODO: agregar log en consola

        var filterUpdate = Builders<Conversation>.Update.Push(conv => conv.logs, comment);

        await mongo.Conversations.UpdateOneAsync(filter, filterUpdate);
    }
}
