namespace Alequeshow.Habitica.Webhooks.Domain;

public class CheckItem
{
    public string? Id { get; set; }

    public required string Text { get; set; }

    public bool Completed { get; set; }
}