using Newtonsoft.Json;

namespace WebApi.Configuration.RabbitMQ.Middlewares;

public class ConsumerLoggingMiddleware<T>(ILogger<ConsumerLoggingMiddleware<T>> logger) : IFilter<ConsumeContext<T>> where T : class
{
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(ConsumerLoggingMiddleware<dynamic>));
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        string messageType = typeof(T).Name;

        logger.LogInformation("Nuevo mensaje a consumir entrante.");
        logger.LogInformation("Fecha y hora: {date}.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        logger.LogInformation("Direccion de destino: {address}.", context.DestinationAddress);
        logger.LogInformation("Consumiendo mensaje de tipo: {messageType}.", messageType);
        logger.LogInformation("Payload recibido: {payload}.", JsonConvert.SerializeObject(context.Message, Formatting.Indented));

        try
        {
            logger.LogInformation("Procesando consumer...");

            await next.Send(context);

            logger.LogInformation("Mensaje de tipo {messageType} procesado exitosamente.", messageType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando mensaje de tipo: {messageType}", messageType);
            throw;
        }
    }
}
