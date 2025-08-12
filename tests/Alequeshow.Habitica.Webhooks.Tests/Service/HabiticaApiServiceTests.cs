using System.Net;
using System.Text.Json;
using Alequeshow.Habitica.Webhooks.Domain;
using Alequeshow.Habitica.Webhooks.Service;
using Alequeshow.Habitica.Webhooks.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Refit;
using Xunit;
using DomainTask = Alequeshow.Habitica.Webhooks.Domain.Task;
using Task = System.Threading.Tasks.Task;

namespace Alequeshow.Habitica.Webhooks.Tests.Service;

public class HabiticaApiServiceTests
{
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly Mock<IHabiticaApiClient> _mockHabiticaApiClient;
    private readonly HabiticaApiService _service;

    public HabiticaApiServiceTests()
    {
        _mockLogger = new Mock<ILogger<TaskService>>();
        _mockHabiticaApiClient = new Mock<IHabiticaApiClient>();
        _service = new HabiticaApiService(_mockLogger.Object, _mockHabiticaApiClient.Object);
    }

    [Fact]
    public async Task CreateUserTasksAsync_WithValidTask_ReturnsTask()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");
        var expectedTask = CreateTestTask("daily", "Test Task");
        expectedTask.Id = "test-id";

        var successResponse = CreateSuccessfulApiResponse(expectedTask);
        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _service.CreateUserTasksAsync(inputTask);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTask.Id, result.Id);
        Assert.Equal(expectedTask.Text, result.Text);
        Assert.Equal(expectedTask.Type, result.Type);
        _mockHabiticaApiClient.Verify(x => x.CreateUserTasksAsync(inputTask), Times.Once);
    }

    [Fact]
    public async Task CreateUserTasksAsync_WhenApiCallThrowsException_ThrowsException()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");

        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserTasksAsync(inputTask));
        Assert.Equal("Failed to create user task", exception.Message);

        // Verify logging
        VerifyLoggerCalled(LogLevel.Error, "Exception occurred during API call");
    }

    [Fact]
    public async Task CreateUserTasksAsync_WhenApiReturnsUnsuccessfulResponse_ThrowsException()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");
        var unsuccessfulResponse = CreateUnsuccessfulApiResponse<DomainTask>();

        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ReturnsAsync(unsuccessfulResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserTasksAsync(inputTask));
        Assert.Equal("Failed to create user task", exception.Message);
    }

    [Fact]
    public async Task CreateUserTasksAsync_WhenApiReturnsNullContent_ThrowsException()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");
        var nullContentResponse = CreateNullContentApiResponse<DomainTask>();

        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ReturnsAsync(nullContentResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserTasksAsync(inputTask));
        Assert.Equal("Failed to create user task", exception.Message);
    }

    [Fact]
    public async Task CreateUserTasksAsync_WhenApiReturnsError_LogsErrorAndThrowsException()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");

        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ThrowsAsync(new HttpRequestException("Invalid task data"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserTasksAsync(inputTask));
        Assert.Equal("Failed to create user task", exception.Message);

        // Verify error logging
        VerifyLoggerCalled(LogLevel.Error, "Exception occurred during API call");
    }

    [Fact]
    public async Task CreateUserTasksAsync_WhenApiReturnsErrorWithoutErrorResponse_LogsErrorAndThrowsException()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");

        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ThrowsAsync(new InvalidOperationException("HTTP 400 Bad Request"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserTasksAsync(inputTask));
        Assert.Equal("Failed to create user task", exception.Message);

        // Verify error logging
        VerifyLoggerCalled(LogLevel.Error, "Exception occurred during API call");
    }

    [Fact]
    public async Task CreateUserTasksAsync_WhenApiCallReturnsNull_ThrowsException()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");

        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ReturnsAsync((ApiResponse<HabiticaApiResponse<DomainTask>>)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserTasksAsync(inputTask));
        Assert.Equal("Failed to create user task", exception.Message);

        // Verify logging
        VerifyLoggerCalled(LogLevel.Error, "Exception occurred during API call");
    }

    [Fact]
    public async Task GetUserTasksAsync_WithValidType_ReturnsTasks()
    {
        // Arrange
        var taskType = "dailys";
        var expectedTasks = new List<DomainTask>
        {
            CreateTestTask("daily", "Task 1"),
            CreateTestTask("daily", "Task 2")
        };

        var successResponse = CreateSuccessfulApiResponse(expectedTasks);
        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal(expectedTasks[0].Text, result.First().Text);
        Assert.Equal(expectedTasks[1].Text, result.Skip(1).First().Text);
        _mockHabiticaApiClient.Verify(x => x.GetUserTasksAsync(taskType), Times.Once);
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenApiReturnsEmptyList_ReturnsEmptyEnumerable()
    {
        // Arrange
        var taskType = "todos";
        var emptyTasks = new List<DomainTask>();
        var successResponse = CreateSuccessfulApiResponse(emptyTasks);

        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockHabiticaApiClient.Verify(x => x.GetUserTasksAsync(taskType), Times.Once);
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenApiCallThrowsException_ReturnsEmptyEnumerable()
    {
        // Arrange
        var taskType = "habits";

        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify logging
        VerifyLoggerCalled(LogLevel.Error, "Exception occurred during API call");
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenApiReturnsUnsuccessfulResponse_ReturnsEmptyEnumerable()
    {
        // Arrange
        var taskType = "rewards";
        var unsuccessfulResponse = CreateUnsuccessfulApiResponse<List<DomainTask>>();

        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ReturnsAsync(unsuccessfulResponse);

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenApiReturnsNullContent_ReturnsEmptyEnumerable()
    {
        // Arrange
        var taskType = "dailys";
        var nullContentResponse = CreateNullContentApiResponse<List<DomainTask>>();

        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ReturnsAsync(nullContentResponse);

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenApiReturnsError_LogsErrorAndReturnsEmptyEnumerable()
    {
        // Arrange
        var taskType = "dailys";

        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ThrowsAsync(new UnauthorizedAccessException("User not authorized"));

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify error logging
        VerifyLoggerCalled(LogLevel.Error, "Exception occurred during API call");
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenApiReturnsErrorWithInvalidJson_LogsErrorAndReturnsEmptyEnumerable()
    {
        // Arrange
        var taskType = "dailys";

        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ThrowsAsync(new HttpRequestException("HTTP 500 Internal Server Error"));

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify error logging
        VerifyLoggerCalled(LogLevel.Error, "Exception occurred during API call");
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenApiCallReturnsNull_ReturnsEmptyEnumerable()
    {
        // Arrange
        var taskType = "dailys";

        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ReturnsAsync((ApiResponse<HabiticaApiResponse<List<DomainTask>>>)null!);

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify logging
        VerifyLoggerCalled(LogLevel.Error, "Exception occurred during API call");
    }

    [Fact]
    public async Task GetUserTasksAsync_WhenApiReturnsNullData_ReturnsEmptyEnumerable()
    {
        // Arrange
        var taskType = "dailys";
        var responseWithNullData = CreateApiResponseWithNullData<List<DomainTask>>();

        _mockHabiticaApiClient
            .Setup(x => x.GetUserTasksAsync(taskType))
            .ReturnsAsync(responseWithNullData);

        // Act
        var result = await _service.GetUserTasksAsync(taskType);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateUserTasksAsync_WhenApiReturnsNullDataInSuccessResponse_ThrowsException()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");
        var responseWithNullData = CreateApiResponseWithNullData<DomainTask>();

        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ReturnsAsync(responseWithNullData);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserTasksAsync(inputTask));
        Assert.Equal("Failed to create user task", exception.Message);
    }

    [Fact]
    public async Task GetUserTasksAsync_WithDifferentTaskTypes_CallsApiCorrectly()
    {
        // Arrange
        var taskTypes = new[] { "dailys", "todos", "habits", "rewards" };
        var emptyResponse = CreateSuccessfulApiResponse(new List<DomainTask>());

        foreach (var taskType in taskTypes)
        {
            _mockHabiticaApiClient
                .Setup(x => x.GetUserTasksAsync(taskType))
                .ReturnsAsync(emptyResponse);
        }

        // Act & Assert
        foreach (var taskType in taskTypes)
        {
            var result = await _service.GetUserTasksAsync(taskType);
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockHabiticaApiClient.Verify(x => x.GetUserTasksAsync(taskType), Times.Once);
        }
    }

    [Fact]
    public async Task HandleApiResponse_WhenNoContentAndNoError_LogsUnknownError()
    {
        // Arrange
        var inputTask = CreateTestTask("daily", "Test Task");
        var responseWithNoContentAndNoError = CreateApiResponseWithNoContentAndNoError<DomainTask>();

        _mockHabiticaApiClient
            .Setup(x => x.CreateUserTasksAsync(inputTask))
            .ReturnsAsync(responseWithNoContentAndNoError);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.CreateUserTasksAsync(inputTask));
        Assert.Equal("Failed to create user task", exception.Message);

        // Verify unknown error logging
        VerifyLoggerCalled(LogLevel.Error, "API call failed with unkown error");
    }

    private void VerifyLoggerCalled(LogLevel logLevel, string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private static DomainTask CreateTestTask(string type, string text)
    {
        return new DomainTask
        {
            Id = Guid.NewGuid().ToString(),
            Type = type,
            Text = text,
            Value = 1.0,
            Priority = 1.0,
            Completed = false
        };
    }

    private static ApiResponse<HabiticaApiResponse<T>> CreateSuccessfulApiResponse<T>(T data)
    {
        var content = new HabiticaApiResponse<T>
        {
            Success = true,
            Data = data
        };

        return new ApiResponse<HabiticaApiResponse<T>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            content,
            new RefitSettings());
    }

    private static ApiResponse<HabiticaApiResponse<T>> CreateUnsuccessfulApiResponse<T>()
    {
        var content = new HabiticaApiResponse<T>
        {
            Success = false,
            Data = default
        };

        return new ApiResponse<HabiticaApiResponse<T>>(
            new HttpResponseMessage(HttpStatusCode.BadRequest),
            content,
            new RefitSettings());
    }

    private static ApiResponse<HabiticaApiResponse<T>> CreateNullContentApiResponse<T>()
    {
        return new ApiResponse<HabiticaApiResponse<T>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            null,
            new RefitSettings());
    }

    private static ApiResponse<HabiticaApiResponse<T>> CreateApiResponseWithNullData<T>()
    {
        var content = new HabiticaApiResponse<T>
        {
            Success = true,
            Data = default
        };

        return new ApiResponse<HabiticaApiResponse<T>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            content,
            new RefitSettings());
    }

    private static ApiResponse<HabiticaApiResponse<T>> CreateApiResponseWithNoContentAndNoError<T>()
    {
        return new ApiResponse<HabiticaApiResponse<T>>(
            new HttpResponseMessage(HttpStatusCode.OK),
            null,
            new RefitSettings(),
            null);
    }
}
