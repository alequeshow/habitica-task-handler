using System.Text.Json;
using Alequeshow.Habitica.Webhooks.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Refit;

namespace Alequeshow.Habitica.Webhooks.Service;

public class HabiticaApiService(
        ILogger<TaskService> logger,
        IHabiticaApiClient habiticaApi) : IHabiticaApiService
{
    private readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Domain.Task> CreateUserTasksAsync(Domain.Task task)
    {
        var result = await HandleApiResponseAsync(habiticaApi.CreateUserTasksAsync(task));

        return result ?? throw new Exception("Failed to create user task");
    }

    public async Task<IEnumerable<Domain.Task>> GetUserTasksAsync(string type)
    {

        var result = await HandleApiResponseAsync(habiticaApi.GetUserTasksAsync(type));

        return result ?? [];
    }

    private async Task<T?> HandleApiResponseAsync<T>(Task<ApiResponse<Domain.HabiticaApiResponse<T>>> apiCall)
    {
        try
        {
            var response = await apiCall ?? throw new Exception("Unknown error from API request");

            if (response.Content is not null)
            {
                if (response.Content.Success)
                {
                    return response.Content.Data;
                }
            }

            if (response.Error is not null)
            {
                var apiErrorResponse = response.Error.Content is not null
                    ? JsonSerializer.Deserialize<Domain.HabiticaApiErrorResponse>(response.Error.Content, JsonSerializerOptions)
                    : null;

                logger.LogError(response.Error, "API call failed: {Message}",
                    apiErrorResponse?.Message ?? response.Error.Message);
            }
            else
            {
                logger.LogError("API call failed with unkown error");
            }            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during API call");
        }

        return default;
    }
}