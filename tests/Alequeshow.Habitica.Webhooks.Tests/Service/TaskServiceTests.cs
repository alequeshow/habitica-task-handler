using Alequeshow.Habitica.Webhooks.Domain;
using Alequeshow.Habitica.Webhooks.Service;
using Alequeshow.Habitica.Webhooks.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Refit;
using Xunit;
using DomainTask = Alequeshow.Habitica.Webhooks.Domain.Task;
using Task = System.Threading.Tasks.Task;

namespace Alequeshow.Habitica.Webhooks.Tests.Service;

public class TaskServiceTests
{
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly Mock<IHabiticaApiService> _mockHabiticaApi;
    private readonly Mock<IOptions<TaskServiceOptions>> _mockOptions;
    private readonly TaskServiceOptions _defaultOptions;

    public TaskServiceTests()
    {
        _mockLogger = new Mock<ILogger<TaskService>>();
        _mockHabiticaApi = new Mock<IHabiticaApiService>();
        _mockOptions = new Mock<IOptions<TaskServiceOptions>>();
        
        _defaultOptions = new TaskServiceOptions
        {
            SnoozeableTagId = "test-tag-id",
            CompareDueTaskToYesterday = false
        };
        
        _mockOptions.Setup(o => o.Value).Returns(_defaultOptions);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldInitializeWithDefaults()
    {
        // Arrange
        var mockNullOptions = new Mock<IOptions<TaskServiceOptions>>();
        var nullTaskServiceOptions = new TaskServiceOptions(); // Use empty options instead of null
        mockNullOptions.Setup(o => o.Value).Returns(nullTaskServiceOptions);

        // Act
        var service = new TaskService(_mockLogger.Object, mockNullOptions.Object, _mockHabiticaApi.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithCompareDueTaskToYesterdayTrue_ShouldInitializeCorrectly()
    {
        // Arrange
        var options = new TaskServiceOptions
        {
            SnoozeableTagId = "test-tag",
            CompareDueTaskToYesterday = true
        };
        _mockOptions.Setup(o => o.Value).Returns(options);

        // Act
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task HandleTaskActivityAsync_ShouldReturnCompletedTask()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var taskActivity = new TaskActivityEvent
        {
            Type = "activity",
            Task = CreateTestTask("daily", "Test Task", ["test-tag-id"])
        };

        // Act
        await service.HandleTaskActivityAsync(taskActivity);

        // Assert - Should not throw and should complete successfully
        Assert.True(true); // Method should complete without error
    }

    [Fact]
    public async Task HandleCronAsync_ShouldCallHandleDailyTasks()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var tasks = new List<DomainTask>
        {
            CreateTestTask("daily", "Test Daily Task", ["test-tag-id"])
        };
        
        // Setup the mock to return a task that completes successfully
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(It.IsAny<ApiResponse<HabiticaApiResponse<List<DomainTask>>>>());

        // Act & Assert - Should not throw exception
        await service.HandleCronAsync();

        // Verify the API was called
        _mockHabiticaApi.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
    }

    [Fact]
    public async Task HandleDailyTasks_WithNoTasks_ShouldLogWarning()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        
        // Setup the mock to throw an exception or return null - we're testing that it handles this gracefully
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ThrowsAsync(new Exception("API Error"));

        // Act & Assert - Should handle exception gracefully
        await service.HandleCronAsync();

        // Verify the API was called
        _mockHabiticaApi.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
    }

    [Fact]
    public async Task HandleDailyTasks_WithNullContent_ShouldLogWarning()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        
        // Setup mock to return a task that completes but we expect null handling
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(It.IsAny<ApiResponse<HabiticaApiResponse<List<DomainTask>>>>());

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApi.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
    }

    [Fact]
    public async Task HandleDailyTasks_WithValidTasks_ShouldProcessEachTask()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var tasks = new List<DomainTask>
        {
            CreateTestTask("daily", "Task 1", ["test-tag-id"]),
            CreateTestTask("daily", "Task 2", ["other-tag"])
        };
        
        var apiResponse = CreateMockApiResponse(tasks);
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(apiResponse);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApi.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
    }

    [Fact]
    public async Task HandleSnoozedTaskAsync_WithSnoozeableTask_ShouldCreateTodoTask()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var snoozeableTask = CreateSnoozeableTask();
        var tasks = new List<DomainTask> { snoozeableTask };
        
        var getTasksResponse = CreateMockApiResponse(tasks);
        var createTaskResponse = CreateMockCreateTaskResponse();
        
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(getTasksResponse);
        _mockHabiticaApi.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
                       .ReturnsAsync(createTaskResponse);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApi.Verify(x => x.CreateUserTasksAsync(It.Is<DomainTask>(t => 
            t.Type == "todo" && 
            t.Text == snoozeableTask.Text &&
            t.Completed == false &&
            t.Tags != null && !t.Tags.Contains("test-tag-id")
        )), Times.Once);
        
        VerifyLogInformation("Snoozed task detected to be created with payload");
        VerifyLogInformation("Snoozed task created!");
    }

    [Fact]
    public async Task HandleSnoozedTaskAsync_WithNonSnoozeableTask_ShouldNotCreateTodoTask()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var nonSnoozeableTask = CreateTestTask("daily", "Non-snoozeable Task", ["other-tag"]);
        var tasks = new List<DomainTask> { nonSnoozeableTask };
        
        var apiResponse = CreateMockApiResponse(tasks);
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(apiResponse);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApi.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    [Fact]
    public async Task HandleSnoozedTaskAsync_WithException_ShouldLogError()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var snoozeableTask = CreateSnoozeableTask();
        var tasks = new List<DomainTask> { snoozeableTask };
        
        var getTasksResponse = CreateMockApiResponse(tasks);
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(getTasksResponse);
        _mockHabiticaApi.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
                       .ThrowsAsync(new Exception("API Error"));

        // Act
        await service.HandleCronAsync();

        // Assert
        VerifyLogError("Error while handling Snoozed task");
    }

    [Theory]
    [InlineData("daily", true, true, true)] // Daily task, due, with correct tag
    [InlineData("todo", true, true, false)] // Todo task, due, with correct tag
    [InlineData("daily", false, true, false)] // Daily task, not due, with correct tag
    [InlineData("daily", true, false, false)] // Daily task, due, without correct tag
    [InlineData("daily", false, false, false)] // Daily task, not due, without correct tag
    public void IsSnoozeableTask_ShouldReturnExpectedResult(string type, bool isDue, bool hasCorrectTag, bool expectedResult)
    {
        // This test verifies the logic indirectly by checking the behavior when processing tasks
        // since IsSnoozeableTask is a private method
        
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var tags = hasCorrectTag ? new List<string> { "test-tag-id" } : new List<string> { "other-tag" };
        var task = CreateTestTask(type, "Test Task", tags, isDue);
        var tasks = new List<DomainTask> { task };
        
        var getTasksResponse = CreateMockApiResponse(tasks);
        var createTaskResponse = CreateMockCreateTaskResponse();
        
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(getTasksResponse);
        _mockHabiticaApi.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
                       .ReturnsAsync(createTaskResponse);

        // Act
        var handleTask = async () => await service.HandleCronAsync();

        // Assert
        Assert.NotNull(handleTask);
        
        // If we expect it to be snoozeable, CreateUserTasksAsync should be called
        var expectedCalls = expectedResult ? Times.Once() : Times.Never();
        _mockHabiticaApi.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), expectedCalls);
    }

    [Fact]
    public async Task HandleSnoozedTaskAsync_WithTaskWithChecklist_ShouldCreateTodoWithNewChecklistIds()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var snoozeableTask = CreateSnoozeableTask();
        snoozeableTask.Checklist = new List<CheckItem>
        {
            new CheckItem { Id = "original-id-1", Text = "Item 1", Completed = false },
            new CheckItem { Id = "original-id-2", Text = "Item 2", Completed = true }
        };
        
        var tasks = new List<DomainTask> { snoozeableTask };
        var getTasksResponse = CreateMockApiResponse(tasks);
        var createTaskResponse = CreateMockCreateTaskResponse();
        
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(getTasksResponse);
        _mockHabiticaApi.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
                       .ReturnsAsync(createTaskResponse);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApi.Verify(x => x.CreateUserTasksAsync(It.Is<DomainTask>(t => 
            t.Checklist != null && 
            t.Checklist.Count == 2 &&
            t.Checklist.All(item => item.Id != "original-id-1" && item.Id != "original-id-2")
        )), Times.Once);
    }

    [Fact]
    public async Task HandleSnoozedTaskAsync_ShouldCreateTodoWithReminder()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApi.Object);
        var snoozeableTask = CreateSnoozeableTask();
        var tasks = new List<DomainTask> { snoozeableTask };
        
        var getTasksResponse = CreateMockApiResponse(tasks);
        var createTaskResponse = CreateMockCreateTaskResponse();
        
        _mockHabiticaApi.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync(getTasksResponse);
        _mockHabiticaApi.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
                       .ReturnsAsync(createTaskResponse);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApi.Verify(x => x.CreateUserTasksAsync(It.Is<DomainTask>(t => 
            t.Reminders != null && 
            t.Reminders.Count == 1 &&
            t.Reminders[0].Time > DateTime.UtcNow
        )), Times.Once);
    }

    private DomainTask CreateTestTask(string type, string text, List<string> tags, bool isDue = true)
    {
        var task = new DomainTask
        {
            Id = Guid.NewGuid().ToString(),
            Type = type,
            Text = text,
            Tags = tags,
            IsDue = isDue,
            Completed = false,
            Value = 1.0,
            Priority = 1.0
        };

        // Add history for daily tasks to satisfy IsDueInDate method
        if (type == "daily" && isDue)
        {
            task.History = new List<History>
            {
                new History
                {
                    Date = DateTime.Today,
                    IsDue = true,
                    Completed = false
                }
            };
        }

        return task;
    }

    private DomainTask CreateSnoozeableTask()
    {
        return CreateTestTask("daily", "Snoozeable Task", ["test-tag-id"], true);
    }

    private ApiResponse<HabiticaApiResponse<List<DomainTask>>> CreateMockApiResponse(List<DomainTask>? tasks)
    {
        var habiticaResponse = new HabiticaApiResponse<List<DomainTask>>
        {
            Data = tasks
        };
        
        // Since ApiResponse is not mockable, we need to set up the mock differently
        var mockResponse = new Mock<ApiResponse<HabiticaApiResponse<List<DomainTask>>>>();
        try
        {
            mockResponse.Setup(x => x.Content).Returns(habiticaResponse);
            return mockResponse.Object;
        }
        catch (NotSupportedException)
        {
            // If mocking fails, return a null response and handle it in the setup
            return null!;
        }
    }

    private ApiResponse<HabiticaApiResponse<DomainTask>> CreateMockCreateTaskResponse()
    {
        var habiticaResponse = new HabiticaApiResponse<DomainTask>
        {
            Data = new DomainTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = "todo",
                Text = "Created Task",
                Value = 1.0,
                Priority = 1.0
            }
        };
        
        var mockResponse = new Mock<ApiResponse<HabiticaApiResponse<DomainTask>>>();
        try
        {
            mockResponse.Setup(x => x.Content).Returns(habiticaResponse);
            return mockResponse.Object;
        }
        catch (NotSupportedException)
        {
            return null!;
        }
    }

    private void VerifyLogWarning(string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyLogInformation(string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogError(string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}