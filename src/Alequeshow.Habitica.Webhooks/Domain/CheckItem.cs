namespace Alequeshow.Habitica.Webhooks.Domain;

public record CheckItem
{
    public string? Id { get; set; }

    public required string Text { get; set; }

    public bool Completed { get; set; }
}