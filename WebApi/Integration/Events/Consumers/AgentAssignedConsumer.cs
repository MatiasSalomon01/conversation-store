namespace WebApi.Integration.Events.Consumers;

public class AgentAssignedConsumer(MongoDBService mongo) : IConsumer<AgentAssigned>
{
    public async Task Consume(ConsumeContext<AgentAssigned> context)
    {
        var @event = context.Message;

        var newAgent = new AgentSender(@event.agent_assigned.agent_name, @event.agent_assigned.agent_id);
        var by = new AgentSender(@event.authorized_by.agent_name, @event.authorized_by.agent_id);

        var assignmentLog = new AssignmentLog(newAgent, by, @event.timestamp, @event.ticket_id);

        var filter = Builders<Conversation>.Filter.Eq(conv => conv.id, @event.conversation_id);

        var existsConversation = await mongo.Conversations.Find(filter).AnyAsync();

        if (!existsConversation) return; //TODO: agregar log en consola

        var filterUpdate = Builders<Conversation>.Update.Push(conv => conv.logs, assignmentLog);

        await mongo.Conversations.UpdateOneAsync(filter, filterUpdate);
    }
}
