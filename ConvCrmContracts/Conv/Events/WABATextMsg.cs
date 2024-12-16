namespace ConvCrmContracts.Conv.Events;

/// <summary>
/// Representa un evento de mensaje de texto de WhatsApp Business recibido.
/// Este es el contrato entre este microservicio y el evento WhatsAppBusinessTextMessageReceived.
/// </summary>
public class WABATextMsg : IBaseEvent
{
    /// <summary>
    /// Identificador único del mensaje.
    /// </summary>
    public Guid uuid { get; set; }

    /// <summary>
    /// Marca de tiempo cuando se recibió el mensaje.
    /// </summary>
    public DateTime timestamp { get; set; }

    /// <summary>
    /// Remitente del mensaje. Formato: número de teléfono.
    /// </summary>
    public string sender { get; set; } = default!;

    /// <summary>
    /// Receptor del mensaje. Formato: número de teléfono.
    /// </summary>
    public string receiver { get; set; } = default!;

    /// <summary>
    /// Cuerpo del mensaje. Este es el contenido real del mensaje.
    /// </summary>
    public string body { get; set; } = default!;

    /// <summary>
    /// Identificador de origen del mensaje.
    /// </summary>
    /// TODO No deberíamos esperar esto de RabbitMQ.
    public string source_id { get; set; } = default!;

    /// <summary>
    /// Este es el identificador utilizado por Meta para identificar el mensaje.
    /// </summary>
    public string message_id { get; set; } = default!;

    /// <summary>
    /// Detalles del usuario asociados con el mensaje.
    /// </summary>
    public UserDetails? user_details { get; set; }
}
