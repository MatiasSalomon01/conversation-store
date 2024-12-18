using ConvCrmContracts.Crm.Events;
using WebApi.Schemas;
using WebApi.Common;
using ConvCrmContracts.Conv.Querys;
using MassTransit;
using Microsoft.VisualBasic;

namespace WebApi.Integration.Events.Consumers;

/// <summary>
/// Consumidor del evento WhatsAppBusinessTextMessageReceived.
/// Escucha los mensajes de texto entrantes de WhatsApp Business, crea una nueva conversación si es necesario,
/// y agrega el nuevo mensaje de texto a la conversación correspondiente.
/// </summary>
public class WABATextMsgConsumer(MongoDBService mongo, ILogger<WABATextMsgConsumer> logger, ISendEndpointProvider endpointProvider, ILlamaService llamaService, InitialResponse initialResponse) : IConsumer<WABATextMsg>
{
    public async Task Consume(ConsumeContext<WABATextMsg> context)
    {
        var @event = context.Message;

        logger.LogInformation("Mensaje recibido: {body}", @event.body);

        var fallbackText = @event.body;

        var userDetails = @event.user_details;

        @event.source_id = @event.sender; // TODO preprend "wa." to source_id. See documentation.

        // Crear entrada a ser agregada al final del log de la conversacion
        var campoSender = new WhatsAppSender(null, @event.sender);
        var whatsAppMessage = new WhatsAppTextLog(@event.body, campoSender, @event.timestamp);
        var lastMessageSended = new MinimalConversation(whatsAppMessage.timestamp, whatsAppMessage.fallback_text, whatsAppMessage.sender.display_name ?? whatsAppMessage.sender.phone_number);

        //Filtro para buscar conversacion por el source_id y en estado abierto
        var filter = Builders<Conversation>.Filter.Or(
            Builders<Conversation>.Filter.Eq(conv => conv.source_id, @event.source_id),
            Builders<Conversation>.Filter.Eq(conv => conv.source_id, @event.sender),
            Builders<Conversation>.Filter.Eq(conv => conv.user_details!.codigo_cliente, @event.user_details?.codigo_cliente ?? "cod_not_found")
        );

        //Recuperar conversacion, si existe, agregar nuevo log, sino, crear conversacion nueva
        var conversation = await mongo.Conversations.Find(filter).FirstOrDefaultAsync();

        if (conversation is null)
        {
            logger.LogInformation("No se encontró una conversación con el source_id {source_id}. Creando una nueva...", @event.source_id);

            var newConversation = await HandleNewConversation(@event, whatsAppMessage, @event.user_details);

            await SendNewLasMessage(newConversation!, whatsAppMessage);

            if (newConversation is null) return;

            await SendToAI(newConversation, @event.body, whatsAppMessage.timestamp, [lastMessageSended]);

            if (initialResponse.IsAgent())
            {
                var openTicketCommand = new OpenTicket(newConversation.id, newConversation.source_id, Channel.WhatsApp.ToString(), newConversation.user_details, whatsAppMessage.fallback_text);
                await endpointProvider.Send(nameof(OpenTicket), openTicketCommand);
            }

            logger.LogInformation("Conversación enviada al {name}", initialResponse.Name);
        }
        else
        {
            logger.LogInformation("Conversación con source_id {source_id} encontrada. Agregando log de mensaje de whatsapp...", @event.source_id);

            //Asignar ticket_id al log si la conversacion tiene un ticket activos
            if (conversation.current_ticket_id is not null)
            {
                whatsAppMessage.ticket_id = conversation.current_ticket_id;
            }

            await HandleExistingConversation(conversation, filter, whatsAppMessage);

            await SendNewLasMessage(conversation, whatsAppMessage);

            if (conversation.manage_by is ManageBy.AIAgent)
            {
                //Construir mensajes y agregar el ultimo mensaje recibido al listado
                List<MinimalConversation> messages = BuildMessages(conversation.logs);
                messages.Add(lastMessageSended);

                await SendToAI(conversation, @event.body, whatsAppMessage.timestamp, messages);

                logger.LogInformation("Conversación enviada al {name}", initialResponse.Name);
            }
            else if (conversation.manage_by is ManageBy.Agent) //Enviar al agente
            {
                //Crear mensaje
                var message = new ConversationMessages(whatsAppMessage.log_id, conversation.codigo_cliente,
                    whatsAppMessage.sender.display_name ?? conversation.source_id,
                    whatsAppMessage.fallback_text, whatsAppMessage.timestamp, whatsAppMessage.ticket_id, false);

                //Crear comando
                var command = new GenerateAgentAnswer(conversation.current_ticket_id ?? 0, conversation.id, message, whatsAppMessage.timestamp);
                await endpointProvider.Send(nameof(GenerateAgentAnswer), command);

                logger.LogInformation("Comando enviada al agente.");
            }
            else
            {
                logger.LogInformation("Mensaje guardado: {text} de {sender}", whatsAppMessage.fallback_text, whatsAppMessage.sender);
            }
        }
    }

    //Crear nueva conversacion y enviar comando para crear un ticket con dicha conversacion
    private async Task<Conversation?> HandleNewConversation(WABATextMsg @event, WhatsAppTextLog whatsAppMessage, UserDetails? userDetails)
    {
        try
        {
            var conversation = new Conversation(@event.source_id ?? @event.sender, whatsAppMessage, codigoCliente: userDetails?.codigo_cliente, userDetails: userDetails);

            if (initialResponse.IsAgent())
            {
                conversation.manage_by = ManageBy.Agent;
            }

            await mongo.Conversations.InsertOneAsync(conversation);

            logger.LogInformation("Conversacion creada correctamente.");

            return conversation;
        }
        catch (Exception ex)
        {
            logger.LogError("Error al crear en conversacion: {Error}", ex.Message);
            return null;
        }
    }

    //Agregar nuevo log de mensaje a la conversacion
    private async Task HandleExistingConversation(Conversation conversation, FilterDefinition<Conversation> filter, WhatsAppTextLog whatsAppMessage)
    {
        var filterUpdate = Builders<Conversation>.Update.Push(conv => conv.logs, whatsAppMessage);
        try
        {
            await mongo.Conversations.UpdateOneAsync(filter, filterUpdate);

            logger.LogInformation("Nuevo mensaje de whatsapp agregado correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError("Error al agregar nuevo mensaje de whatsapp a la conversacion: {Error}", ex.Message);
        }
    }

    //Construir array de mensajes para enviar al RAG
    private List<MinimalConversation> BuildMessages(List<Log> logs)
    {
        var messages = new List<MinimalConversation>();

        foreach (var log in logs)
        {
            if (log is WhatsAppTextLog wsLog)
            {
                messages.Add(new MinimalConversation(wsLog.timestamp, wsLog.fallback_text, wsLog.sender.display_name ?? wsLog.sender.phone_number));
            }

            if (log is AIAgentTextLog aiLog)
            {
                messages.Add(new MinimalConversation(aiLog.timestamp, aiLog.fallback_text, aiLog.sender.display_name));
            }
        }

        return messages;
    }

    private async Task SendToAI(Conversation conversation, string body, DateTime timestamp, List<MinimalConversation> messages)
    {
        if (initialResponse.IsRAG())
        {
            var command = new GenerateIAAgentAnswer(Guid.NewGuid(), conversation.id, timestamp, messages);
            await endpointProvider.Send(nameof(GenerateIAAgentAnswer), command);
        }
        else if (initialResponse.IsLlama())
        {
            await llamaService.SendAIAnswer(body, conversation);
        }
    }

    private async Task SendNewLasMessage(Conversation conversation, WhatsAppTextLog whatsAppMessage)
    {
        var newLastMessage = new NewActivityCountdown
        {
            uuid = Guid.NewGuid(),
            timestamp = whatsAppMessage.timestamp,
            conversation_id = conversation.id,
            ticket_id = conversation.current_ticket_id
        };

        await endpointProvider.Send(nameof(NewActivityCountdown), newLastMessage);
    }
}
