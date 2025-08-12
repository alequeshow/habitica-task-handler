namespace Alequeshow.Habitica.Webhooks.Service.Interfaces;

public interface IHabiticaApiService
{
    Task<Domain.Task> CreateUserTasksAsync(Domain.Task task);

    Task<IEnumerable<Domain.Task>> GetUserTasksAsync(string type);
}