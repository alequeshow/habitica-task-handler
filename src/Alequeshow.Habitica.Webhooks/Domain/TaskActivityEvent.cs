namespace Alequeshow.Habitica.Webhooks.Domain;

public class TaskActivityEvent
{
    public required string Type { get; set; }

    public required string Direction { get; set; }   

    public Task? Task { get; set; }

    public string? WebhookType { get; set; }
}