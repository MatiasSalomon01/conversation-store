using Newtonsoft.Json;

namespace WebApi.Configuration.RabbitMQ.Middlewares;

public class CommandLoggingMiddleware<T>(ILogger<CommandLoggingMiddleware<T>> logger) : IFilter<SendContext<T>> where T : class
{
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(CommandLoggingMiddleware<dynamic>));
    }

    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        if (typeof(T).IsInterface) // Hago esto porque se duplica los logs porque agarra el IBaseEvent y luego su implementacion
        {
            await next.Send(context);
            return;
        }

        string messageType = typeof(T).Name;

        logger.LogInformation("Nuevo comando a enviar.");
        logger.LogInformation("Fecha y hora: {date}.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        logger.LogInformation("Direccion de origen: {source}.", context.SourceAddress);
        logger.LogInformation("Direccion de destino: {destination}.", context.DestinationAddress);
        logger.LogInformation("Enviando mensaje de tipo: {messageType}.", messageType);
        logger.LogInformation("Payload a enviar: {payload}.", JsonConvert.SerializeObject(context.Message, Formatting.Indented));

        try
        {
            logger.LogInformation("Procesando command...");

            await next.Send(context);

            logger.LogInformation("Comando de tipo {messageType} procesado exitosamente.", messageType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando comando de tipo: {messageType}", messageType);
            throw;
        }
    }
}
