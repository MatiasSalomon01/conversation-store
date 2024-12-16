using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApi.Services
{
    public class MessagePublisherService(ISendEndpointProvider sendEndpointProvider)
    {
        public async Task SendMessageAsync<T>(T message, string queueName)
        {
            // Obtener el endpoint de la cola
            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));
            // Enviar el mensaje a la cola
            await endpoint.Send(message);

            var messageContent = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                WriteIndented = true // Para formatear el JSON con saltos de línea y sangría
            });

            Console.WriteLine($"Mensaje enviado a la cola '{queueName}': {messageContent}");
        }
    }
}