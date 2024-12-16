namespace ConvCrmContracts.Conv.Events;

/// <summary>
/// Representa un evento de mensaje visto.
/// Este es el contrato entre este microservicio y el evento MessageViewed.
/// </summary>
public class MessageViewed : IBaseEvent
{
    /// <summary>
    /// UUID del mensaje RabbitMQ.
    /// </summary>
    public Guid uuid { get; set; }

    /// <summary>
    /// Marca de tiempo del mensaje RabbitMQ.
    /// </summary>
    public DateTime timestamp { get; set; }

    /// <summary>
    /// UUID de la conversación.
    /// </summary>
    public Guid conversation_id { get; set; }

    /// <summary>
    /// Identificador del ticket
    /// </summary>
    public int ticket_id { get; set; }

    /// <summary>
    /// Marca de tiempo cuando el mensaje fue leído por un agente.
    /// </summary>
    public DateTime was_viewed_at { get; set; }

    /// <summary>
    /// Detalles del agente que leyó el mensaje.
    /// </summary>
    public WasViewedBy was_viewed_by { get; set; } = default!;
}

/// <summary>
/// Clase ayudante. Representa la estructura de un agente que ha visto/leido un mensaje.
/// </summary>
public class WasViewedBy
{
    /// <summary>
    /// ID del agente interno al CRM.
    /// </summary>
    public string agent_crm_id { get; set; } = default!;

    /// <summary>
    /// ID del agente en Keycloak.
    /// </summary>
    public string agent_keycloak_id { get; set; } = default!;

    /// <summary>
    /// Nombre para mostrar del agente (usualmente nombre + apellido).
    /// </summary>
    public string agent_name { get; set; } = default!;
}
