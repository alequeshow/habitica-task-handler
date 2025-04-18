using Refit;

namespace Alequeshow.Habitica.Webhooks.Service.Interfaces;

public interface IHabiticaApiService
{
    [Post("/api/v3/tasks/user")]
    Task<ApiResponse<Domain.Task>> CreateUserTasksAsync(Domain.Task task);
}