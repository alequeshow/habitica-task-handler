using System.Text.Json;

namespace Alequeshow.Habitica.Webhooks.Domain;

public record Task
{
    public string? Id { get; set; }

    public required string Type { get; set; }

    public required string Text { get; set; }

    public double Value { get; set; }

    public string? Attribute { get; set; }

    public List<CheckItem>? Checklist { get; set; }

    public List<string>? Tags { get; set; }

    public List<Reminder>? Reminders { get; set; }

    public List<History>? History { get; set; }

    /// <summary>
    /// Daily Only
    /// </summary>
    public string? Frequency { get; set; }

    /// <summary>
    /// Daily Only
    /// </summary>
    public int? Streak { get; set; }

    /// <summary>
    /// Daily Only
    /// </summary>
    public bool? IsDue { get; set; }

    /// <summary>
    /// ToDo Only
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// N/A to Habits
    /// </summary>
    public bool? Completed { get; set; }    

    public bool IsDaily() => string.Equals(Type, "daily", StringComparison.CurrentCultureIgnoreCase);

    public History? GetLastHistoryEntry(bool excludesToday = false)
    {
        if (History == null || History.Count == 0)
        {
            return null;
        }
        
        if (!excludesToday)
        {
            return History
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

        return History
            .Where(h => h.Date.Date < DateTime.Today)
            .OrderByDescending(h => h.Date)
            .FirstOrDefault();
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}    