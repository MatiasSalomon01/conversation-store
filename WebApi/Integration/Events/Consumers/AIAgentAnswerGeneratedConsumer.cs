using MongoDB.Driver;
using WebApi.Extensions;
using WebApi.Schemas.Logs;

namespace WebApi.Integration.Events.Consumers;

public class AIAgentAnswerGeneratedConsumer(MongoDBService mongo, ISendEndpointProvider endpointProvider) : IConsumer<AIAgentAnswerGenerated>
{
    public async Task Consume(ConsumeContext<AIAgentAnswerGenerated> context)
    {
        var @event = context.Message;
        var aiMessage = new AIAgentTextLog(@event.body, @event.timestamp, @event.ticket_id);

        var filter = Builders<Conversation>.Filter.Eq(conv => conv.id, @event.conversation_id);

        var projection = Builders<Conversation>.Projection
            .Include(conv => conv.id)
            .Include(conv => conv.source_id)
            .Include(conv => conv.user_details)
            .Include(conv => conv.logs);

        var conversation = await mongo.Conversations
            .Find(filter)
            .Project<Conversation>(projection)
            .FirstOrDefaultAsync();

        if (conversation is null) return; //TODO: agregar log en consola

        var filterUpdate = Builders<Conversation>.Update.Push(conv => conv.logs, aiMessage);

        await mongo.Conversations.UpdateOneAsync(filter, filterUpdate);

        if (@event.open_ticket)
        {
            var lastMessage = conversation.logs.OfType<WhatsAppTextLog>().LastOrDefault()?.fallback_text ?? string.Empty;

            var openTicketCommand = new OpenTicket(conversation.id, conversation.source_id, Channel.WhatsApp.ToString(), conversation.user_details, lastMessage);
            await endpointProvider.Send(nameof(OpenTicket), openTicketCommand);

            var filterManagedBy = Builders<Conversation>.Update
                .Set(conv => conv.manage_by, ManageBy.Waiting);

            await mongo.Conversations.UpdateOneAsync(filter, filterManagedBy);
        }

        //TODO: de donde sacar el sender y el receiver para enviar de vuelta al whatsapp
        var command = new SendWABATextMessage(Guid.NewGuid(), DateTime.Now, conversation.source_id, conversation.source_id, @event.body);
        await endpointProvider.Send(nameof(SendWABATextMessage), command);
    }
}
