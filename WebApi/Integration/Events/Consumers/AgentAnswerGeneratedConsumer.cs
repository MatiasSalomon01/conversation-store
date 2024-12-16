namespace WebApi.Integration.Events.Consumers;

public class AgentAnswerGeneratedConsumer(MongoDBService mongo, ISendEndpointProvider endpointProvider) : IConsumer<AgentAnswerGenerated>
{
    public async Task Consume(ConsumeContext<AgentAnswerGenerated> context)
    {
        var @event = context.Message;

        var sender = new AgentSender(@event.agent_details.name, @event.agent_details.id);
        var agentTextLog = new AgentTextLog(@event.body, sender, @event.timestamp, @event.ticket_id);

        var filter = Builders<Conversation>.Filter.Eq(conv => conv.id, @event.conversation_id);

        var projection = Builders<Conversation>.Projection.Include(conv => conv.source_id);

        var conversation = await mongo.Conversations
            .Find(filter)
            .Project<Conversation>(projection)
            .FirstOrDefaultAsync();

        if (conversation is null) return; //TODO: agregar log en consola

        var filterUpdate = Builders<Conversation>.Update.Push(conv => conv.logs, agentTextLog);
        await mongo.Conversations.UpdateOneAsync(filter, filterUpdate);

        var command = new SendWABATextMessage(Guid.NewGuid(), DateTime.Now, conversation.source_id, conversation.source_id, @event.body);
        await endpointProvider.Send(nameof(SendWABATextMessage), command);
    }
}
