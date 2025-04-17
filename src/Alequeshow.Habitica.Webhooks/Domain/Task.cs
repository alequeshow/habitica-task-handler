namespace Alequeshow.Habitica.Webhooks.Domain;

public class Task
{
    public string? Id { get; set; }

    public required string Type { get; set; }

    public required string Text { get; set; }

    public double Value { get; set; }

    public string? Attribute { get; set; }

    public List<CheckItem>? Checklist { get; set; }

    public List<string>? Tags { get; set; }

    public List<Reminder>? Reminders { get; set; }

    /// <summary>
    /// Daily Only
    /// </summary>
    public string? Frequency { get; set; }

    /// <summary>
    /// Daily Only
    /// </summary>
    public int? Streak { get; set; }

    /// <summary>
    /// ToDo Only
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// N/A to Habits
    /// </summary>
    public bool? Completed { get; set; }

    /// <summary>
    /// N/A to Habits
    /// </summary>
    public bool? IsDue { get; set; }
}    