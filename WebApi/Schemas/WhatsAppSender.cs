namespace WebApi.Schemas;

/// <summary>
/// Representa el remitente de un mensaje de WhatsApp.
/// Este es parte del contrato con MongoDB, en particular corresponde al campo "sender".
/// "sender" es de tipo objeto, por lo que se requiere una clase para representar su estructura.
/// </summary>
public class WhatsAppSender(string? displayName, string phoneNumber)
{
    /// <summary>
    /// Nombre para mostrar del remitente.
    /// </summary>
    public string? display_name { get; set; } = displayName;

    /// <summary>
    /// Número de teléfono del remitente.
    /// </summary>
    public string phone_number { get; set; } = phoneNumber;
}
