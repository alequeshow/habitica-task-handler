namespace Alequeshow.Habitica.Webhooks.Domain;

public record Reminder
{
    public string? Id { get; set; }

    public required DateTime Time { get; set; }
}