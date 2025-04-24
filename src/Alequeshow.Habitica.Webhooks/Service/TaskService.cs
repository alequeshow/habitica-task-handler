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
        return HandleSnoozedTaskAsync(taskActivity.Task);
    }

    public async Task HandleCronAsync()
    {
        await HandleDailyTasks();
    }

    private async Task HandleDailyTasks()
    {
        var result = await habiticaApi.GetUserTasksAsync("dailys");
        var dailies = result.Content;

        if(dailies?.Data == null)
        {
            logger.LogWarning("No tasks found.");
            return;
        }

        foreach(var task in dailies.Data)
        {
            await HandleSnoozedTaskAsync(task);
        }
    }

    private async Task HandleSnoozedTaskAsync(Domain.Task task)
    {
        if(IsSnoozeableTask(task))
        {
            try
            {
                var todoTask = task! with
                {
                    Type = "todo",
                    Completed = false,
                    Tags = task.Tags?.Where(tag => tag != SnoozeableTagId).ToList(),
                    Date = DateTime.UtcNow,
                    Checklist = task.Checklist?.Select(
                        item => item with 
                        { 
                            Id = Guid.NewGuid().ToString(),
                        }
                    ).ToList(),
                    Reminders = [
                        new Domain.Reminder
                        {
                            Id = Guid.NewGuid().ToString(),
                            Time = DateTime.UtcNow.AddHours(8),
                        }
                    ],
                    Notes = "Daily Snoozed. Do it!!",
                    Id = null,
                    Frequency = null,
                    Streak = null,
                    IsDue = null,
                    History = null,
                };
    
                logger.LogInformation("Snoozed task detected to be created with payload {NewTask}", todoTask);
    
                var result = await habiticaApi.CreateUserTasksAsync(todoTask);
    
                logger.LogInformation("Snoozed task created! {NewTask}", result.Content?.Data?.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while handling Snoozed task. Task: {Task}", task.ToString());
            }
        }
    }

    private static bool IsSnoozeableTask(Domain.Task task)
    {
        return 
            task.IsDaily() &&
            task.IsDueToday() &&
            (task.Tags?.Contains(SnoozeableTagId) == true);
    }
}