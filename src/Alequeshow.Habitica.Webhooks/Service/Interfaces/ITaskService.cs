namespace Alequeshow.Habitica.Webhooks.Service.Interfaces;

public interface ITaskService
{
    Task HandleTaskActivityAsync(Domain.TaskActivityEvent taskActivity);

    Task HandleCronAsync();
}