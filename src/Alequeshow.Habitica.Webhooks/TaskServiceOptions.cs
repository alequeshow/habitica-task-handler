namespace Alequeshow.Habitica.Webhooks;

public class TaskServiceOptions
{
    public bool CompareDueTaskToYesterday { get; set; }

    public string? SnoozeableTagId { get; set; }
}