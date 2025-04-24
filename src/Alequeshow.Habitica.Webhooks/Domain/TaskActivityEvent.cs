namespace Alequeshow.Habitica.Webhooks.Domain;

public class TaskActivityEvent
{
    public required string Type { get; set; }

    public string? Direction { get; set; }   

    public required Task Task { get; set; }

    public string? WebhookType { get; set; }

    public bool IsUpdateEvent() => string.Equals(Type, "updated", StringComparison.CurrentCultureIgnoreCase);
}