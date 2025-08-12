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

public class TaskServiceTestsSimplified
{
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly Mock<IHabiticaApiService> _mockHabiticaApiService;
    private readonly Mock<IOptions<TaskServiceOptions>> _mockOptions;
    private readonly TaskServiceOptions _defaultOptions;

    public TaskServiceTestsSimplified()
    {
        _mockLogger = new Mock<ILogger<TaskService>>();
        _mockHabiticaApiService = new Mock<IHabiticaApiService>();
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
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithEmptyOptions_ShouldInitializeWithDefaults()
    {
        // Arrange
        var emptyOptions = new TaskServiceOptions();
        var mockOptions = new Mock<IOptions<TaskServiceOptions>>();
        mockOptions.Setup(o => o.Value).Returns(emptyOptions);

        // Act
        var service = new TaskService(_mockLogger.Object, mockOptions.Object, _mockHabiticaApiService.Object);

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
        var mockOptions = new Mock<IOptions<TaskServiceOptions>>();
        mockOptions.Setup(o => o.Value).Returns(options);

        // Act
        var service = new TaskService(_mockLogger.Object, mockOptions.Object, _mockHabiticaApiService.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task HandleTaskActivityAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var taskActivity = new TaskActivityEvent
        {
            Type = "activity",
            Task = CreateTestTask("daily", "Test Task", ["test-tag-id"])
        };

        // Act & Assert - Should not throw and should complete successfully
        await service.HandleTaskActivityAsync(taskActivity);
        
        // The method is currently a no-op, so we just verify it completes without error
        Assert.True(true);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldCallGetUserTasksAsync()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        
        // Setup the mock to return a completed task - we don't need to validate the return value details
        // Since we can't easily mock ApiResponse, we'll test by verifying the API call is made
        // and allowing the method to throw if needed
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ThrowsAsync(new Exception("Expected API call"));

        // Act & Assert - We expect the method to call the API and potentially throw
        var exception = await Assert.ThrowsAsync<Exception>(() => service.HandleCronAsync());
        
        // Verify the API was called
        _mockHabiticaApiService.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_WhenApiThrowsException_ShouldPropagateException()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        
        // Setup the mock to throw an exception
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ThrowsAsync(new Exception("API Error"));

        // Act & Assert - Exception should propagate up
        var exception = await Assert.ThrowsAsync<Exception>(() => service.HandleCronAsync());
        Assert.Equal("API Error", exception.Message);

        // Verify the API was called
        _mockHabiticaApiService.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
    }

    [Theory]
    [InlineData("test-tag-id", false)]
    [InlineData("different-tag", true)]
    [InlineData(null, true)]
    public void Constructor_WithDifferentSnoozeableTagIds_ShouldInitializeCorrectly(string? tagId, bool compareDueTaskToYesterday)
    {
        // Arrange
        var options = new TaskServiceOptions
        {
            SnoozeableTagId = tagId,
            CompareDueTaskToYesterday = compareDueTaskToYesterday
        };
        var mockOptions = new Mock<IOptions<TaskServiceOptions>>();
        mockOptions.Setup(o => o.Value).Returns(options);

        // Act
        var service = new TaskService(_mockLogger.Object, mockOptions.Object, _mockHabiticaApiService.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task HandleTaskActivityAsync_WithDifferentTaskTypes_ShouldCompleteSuccessfully()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        
        var testCases = new[]
        {
            CreateTestTask("daily", "Daily Task", ["tag1"]),
            CreateTestTask("todo", "Todo Task", ["tag2"]),
            CreateTestTask("habit", "Habit Task", ["tag3"]),
            CreateTestTask("reward", "Reward Task", ["tag4"])
        };

        // Act & Assert
        foreach (var testTask in testCases)
        {
            var taskActivity = new TaskActivityEvent
            {
                Type = "updated",
                Task = testTask
            };
            
            // Should complete without throwing
            await service.HandleTaskActivityAsync(taskActivity);
        }
        
        Assert.True(true); // All task types handled successfully
    }

    [Fact]
    public void TaskService_WithOptionValues_ShouldInitializeCorrectly()
    {
        // Arrange
        var options = new TaskServiceOptions
        {
            SnoozeableTagId = "custom-snooze-tag",
            CompareDueTaskToYesterday = true
        };
        var mockOptions = new Mock<IOptions<TaskServiceOptions>>();
        mockOptions.Setup(o => o.Value).Returns(options);

        // Act
        var service = new TaskService(_mockLogger.Object, mockOptions.Object, _mockHabiticaApiService.Object);

        // Assert
        Assert.NotNull(service);
        // We can't directly test private fields, but we can verify the service initializes without exception
    }

    [Fact]
    public async Task TaskService_ErrorHandling_ShouldPropagateApiExceptions()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var expectedException = new InvalidOperationException("API is unavailable");
        
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.HandleCronAsync());
        Assert.Equal("API is unavailable", exception.Message);
        
        // Verify the API was called
        _mockHabiticaApiService.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldCallHandleDailyTasks()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        
        // We can't easily mock the return value, so we'll test that it makes the expected call
        // and verify that HandleCronAsync calls HandleDailyTasks (which calls GetUserTasksAsync)
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ThrowsAsync(new Exception("Expected call"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.HandleCronAsync());
        
        // Verify HandleCronAsync called HandleDailyTasks which calls GetUserTasksAsync with "dailys"
        _mockHabiticaApiService.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
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
}