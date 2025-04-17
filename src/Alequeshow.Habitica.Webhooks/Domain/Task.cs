namespace Alequeshow.Habitica.Webhooks.Domain;

public class Task
{
    public required string Id { get; set; }

    public required string Type { get; set; }

    public string? Frequency { get; set; }

    public int? Streak { get; set; }

    public bool Completed { get; set; }

    public List<CheckItem>? Checklist { get; set; }

    public List<string>? Tags { get; set; }

    public double Value { get; set; }

    public bool IsDue { get; set; }
}    