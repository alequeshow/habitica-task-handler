namespace Alequeshow.Habitica.Webhooks.Domain;

public class Reminder
{
    public string? Id { get; set; }

    public required DateTime Time { get; set; }
}