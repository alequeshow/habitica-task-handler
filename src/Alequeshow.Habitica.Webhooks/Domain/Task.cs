using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Alequeshow.Habitica.Webhooks.Domain;

public record Task
{
    public string? Id { get; set; }

    public required string Type { get; set; }

    public required string Text { get; set; }

    public double Value { get; set; }

    public string? Attribute { get; set; }

    public double Priority { get; set; }

    public string? Notes { get; set; }

    public List<CheckItem>? Checklist { get; set; }

    public List<string>? Tags { get; set; }

    public List<Reminder>? Reminders { get; set; }

    public List<History>? History { get; set; }

    /// <summary>
    /// Daily and Habit
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

    /// <summary>
    /// Habit Only
    /// </summary>
    public bool? Up { get; set; }

    /// <summary>
    /// Habit Only
    /// </summary>
    public bool? Down { get; set; }

    /// <summary>
    /// Habit Only
    /// </summary>
    public int? CounterUp { get; set; }

    /// <summary>
    /// Habit Only
    /// </summary>
    public int? CounterDown { get; set; }

    public bool IsDaily() => string.Equals(Type, "daily", StringComparison.CurrentCultureIgnoreCase);

    public bool IsHabit() => string.Equals(Type, "habit", StringComparison.CurrentCultureIgnoreCase);

    public bool IsWeakHabit(DateTime? date = null)
    {
        if (!IsHabit())
            return false;

        var today = date ?? DateTime.Today;
        var counterDown = CounterDown ?? 0;
        var counterUp = CounterUp ?? 0;

        return Frequency switch
        {
            "daily" => counterUp == 0 || counterDown < 0,
            "weekly" => today.DayOfWeek == DayOfWeek.Saturday && (counterUp < 2 || counterDown < 0),
            "monthly" => today.Day == DateTime.DaysInMonth(today.Year, today.Month) && (counterUp < 2 || counterDown < 0),
            _ => false
        };
    }

    public bool IsDueInDate(DateTime? date = null)
    {
        var dateToCompare = date ?? DateTime.Today;
        var lastEntry = GetLastHistoryEntry(dateToCompare);

        if (lastEntry != null)
        {
            return
                (lastEntry.IsDue == true &&
                lastEntry.Completed == false &&
                lastEntry.Date.Date == dateToCompare.Date)
                || (
                    IsDue == true &&
                    Completed == false
                );
        }

        return
            IsDue == true &&
            Completed == false;
    }

    public History? GetLastHistoryEntry(DateTime? dateRefInclusive = null)
    {
        var dateToCompare = dateRefInclusive ?? DateTime.Today;

        if (History == null || History.Count == 0)
        {
            return null;
        }

        return History
            .Where(h => h.Date.Date <= dateToCompare.Date)
            .OrderByDescending(h => h.Date)
            .FirstOrDefault();
    }

    public bool HasTag(string tagId)
    {
        return Tags?.Contains(tagId) == true;
    }

    public void WriteNotes(params string[] notes)
    {
        if (notes == null || notes.Length == 0)
        {
            return;
        }

        Notes = string.IsNullOrEmpty(Notes)
            ? string.Join("\n", notes)
            : $"{Notes}\n{string.Join("\n", notes)}";
    }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}