using MassTransit.Initializers;
using MongoDB.Driver;

namespace WebApi.Integration.Events.Consumers;

public class GetTicketsPreviewsConsumer(MongoDBService mongo) : IConsumer<GetTicketsPreviews>
{
    public async Task Consume(ConsumeContext<GetTicketsPreviews> context)
    {
        var @event = context.Message;

        //Filtrar por todos los logs que se encuentran en el array de tickets_ids
        var filterLogs = Builders<Conversation>.Filter.ElemMatch(x => x.logs, x => @event.tickets_ids.Contains(x.ticket_id!.Value));

        //Recuperar solamente el array de logs
        var projection = Builders<Conversation>.Projection.Expression(c =>
            new
            {
                c.user_details.name,
                c.source_id,
                logs = c.logs.Where(log => log.ticket_id != null &&
                (log is WhatsAppTextLog ||
                log is AIAgentTextLog ||
                log is AgentTextLog))
            }
        );

        //Recuperar todos los logs encontrados, y agruparlos por ticket_id.
        var objs = await mongo.Conversations
            .Find(filterLogs)
            .Project(projection)
            .ToListAsync();

        var previews = new List<TicketsPreviews>();

        foreach (var obj in objs)
        {
            var allLogs = obj.logs.GroupBy(x => x.ticket_id).ToList();

            foreach (var groupLogs in allLogs)
            {
                //Log de mensaje mas nuevo
                var newestLog = groupLogs.OrderByDescending(x => x.timestamp).First();

                //Contador de mensajes del cliente (customer) que estan sin leer
                var count = groupLogs.OfType<WhatsAppTextLog>().Where(x => x.was_viewed_at == null).Count();

                previews.Add(new TicketsPreviews(groupLogs.Key!.Value, newestLog.GetText(), obj.name ?? obj.source_id, newestLog.timestamp, count));
            }
        }

        var document = new GetTicketsPreviewsDocument(Guid.NewGuid(), DateTime.Now);
        document.SetPreviews(previews);

        await context.RespondAsync(document);
    }
}