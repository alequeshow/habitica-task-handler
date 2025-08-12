using Alequeshow.Habitica.Webhooks.Helpers;
using Alequeshow.Habitica.Webhooks.Service.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alequeshow.Habitica.Webhooks.Service;

public class TaskService(
        ILogger<TaskService> logger,
        IOptions<TaskServiceOptions> options,
        IHabiticaApiService habiticaApiService) : ITaskService
{
    private readonly string SnoozeableTagId = options?.Value.SnoozeableTagId ?? string.Empty;

    private readonly DateTime IsDueDateComparer = options?.Value.CompareDueTaskToYesterday == true
        ? DateTime.Today.AddDays(-1)
        : DateTime.Today;

    private DateTime FollowingDueDate => options?.Value.CompareDueTaskToYesterday == true
            ? DateTime.Today.FromBrtToUtc()
            : DateTime.Today.FromBrtToUtc().AddDays(1);

    public Task HandleTaskActivityAsync(Domain.TaskActivityEvent taskActivity)
    {
        // Method commented because turns out the webhook is not the best way to validate
        // uncompleted tasks when cron runs and score them down.
        //return HandleSnoozedTaskAsync(taskActivity.Task);

        return Task.CompletedTask;
    }

    public async Task HandleCronAsync()
    {
        await HandleDailyTasks();
    }

    private async Task HandleDailyTasks()
    {
        var dailies = await habiticaApiService.GetUserTasksAsync("dailys");    

        if(!dailies.Any())
        {
            logger.LogWarning("No tasks found.");
            return;
        }

        foreach(var task in dailies)
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
                    Date = FollowingDueDate,
                    Checklist = task.Checklist?.Where(c => !c.Completed).Select(
                        item => item with 
                        { 
                            Id = Guid.NewGuid().ToString(),
                        }
                    ).ToList(),
                    Reminders = [
                        new Domain.Reminder
                        {
                            Id = Guid.NewGuid().ToString(),
                            Time = FollowingDueDate.AddHours(10),
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
    
                var result = await habiticaApiService.CreateUserTasksAsync(todoTask);
    
                logger.LogInformation("Snoozed task created! {NewTask}", result.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while handling Snoozed task. Task: {Task}", task.ToString());
            }
        }
    }

    private bool IsSnoozeableTask(Domain.Task task)
    {
        return 
            task.IsDaily() &&
            task.IsDueInDate(IsDueDateComparer) &&
            (task.Tags?.Contains(SnoozeableTagId) == true);
    }
}