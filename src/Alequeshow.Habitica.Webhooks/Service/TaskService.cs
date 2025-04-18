using Alequeshow.Habitica.Webhooks.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alequeshow.Habitica.Webhooks.Service;

public class TaskService(
        ILogger<TaskService> logger,
        IHabiticaApiService habiticaApi) : ITaskService
{
    private const string SnoozeableTagId = "1557c38a-33f0-4391-8fc1-b3f01eb94906";

    public Task HandleTaskActivityAsync(Domain.TaskActivityEvent taskActivity)
    {
        return HandleSnoozedTaskAsync(taskActivity);
    }

    private async Task HandleSnoozedTaskAsync(Domain.TaskActivityEvent taskActivity)
    {
        if(IsSnoozeableTask(taskActivity))
        {
            var todoTask = taskActivity.Task! with
            {
                Type = "todo",
                Completed = false,
                Tags = taskActivity.Task.Tags?.Where(tag => tag != SnoozeableTagId).ToList(),
                Date = DateTime.UtcNow,
                Checklist = taskActivity.Task.Checklist?.Select(
                    item => item with 
                    { 
                        Id = Guid.NewGuid().ToString(),
                    }
                ).ToList(),
                Reminders = [
                    new Domain.Reminder
                    {
                        Id = Guid.NewGuid().ToString(),
                        Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0),
                    }
                ],
                Id = null,
                Frequency = null,
                Streak = null,
                IsDue = null,
                History = null,
            };

            logger.LogInformation("Snoozed task detected to be created with payload {NewTask}", todoTask.ToString());

            var result = await habiticaApi.CreateUserTasksAsync(todoTask);
        }
    }

    private static bool IsSnoozeableTask(Domain.TaskActivityEvent taskActivity)
    {
        var lastEntry = taskActivity.Task?.GetLastHistoryEntry();

        if(lastEntry == null)
        {
            return false;
        }

        var wasSkipped = lastEntry.IsDue == true && 
            lastEntry.Completed == false &&
            taskActivity.Task?.IsDue == true && 
            taskActivity.Task?.Streak == 0 &&
            taskActivity.Task?.Completed == false;

        return taskActivity.IsUpdateEvent() &&
            (taskActivity.Task?.IsDaily() == true) &&
            (taskActivity.Task?.Tags?.Contains(SnoozeableTagId) == true) &&             
            wasSkipped;                        
    }
}