using Refit;

namespace Alequeshow.Habitica.Webhooks.Service.Interfaces;

public interface IHabiticaApiService
{
    [Post("/api/v3/tasks/user")]
    Task<ApiResponse<Domain.HabiticaApiResponse<Domain.Task>>> CreateUserTasksAsync(Domain.Task task);

    [Get("/api/v3/tasks/user")]
    Task<ApiResponse<Domain.HabiticaApiResponse<List<Domain.Task>>>> GetUserTasksAsync([Query] string type);
}