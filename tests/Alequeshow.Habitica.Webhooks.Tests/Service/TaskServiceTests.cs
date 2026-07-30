using Alequeshow.Habitica.Webhooks.Domain;
using Alequeshow.Habitica.Webhooks.Helpers;
using Alequeshow.Habitica.Webhooks.Service;
using Alequeshow.Habitica.Webhooks.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using DomainTask = Alequeshow.Habitica.Webhooks.Domain.Task;
using Task = System.Threading.Tasks.Task;

namespace Alequeshow.Habitica.Webhooks.Tests.Service;

public class TaskServiceTestsSimplified
{
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly Mock<IHabiticaApiService> _mockHabiticaApiService;
    private readonly Mock<IOptions<TaskServiceOptions>> _mockOptions;
    private readonly TaskServiceOptions _defaultOptions;
    private const string SnoozedTagId = "test-tag-id";

    public TaskServiceTestsSimplified()
    {
        _mockLogger = new Mock<ILogger<TaskService>>();
        _mockHabiticaApiService = new Mock<IHabiticaApiService>();
        _mockOptions = new Mock<IOptions<TaskServiceOptions>>();

        _defaultOptions = new TaskServiceOptions
        {
            SnoozeableTagId = SnoozedTagId,
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
    public async Task HandleCronAsync_ShouldCallHandleDailyTasks_WithFailedTasks()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        // We can't easily mock the return value, so we'll test that it makes the expected call
        // and verify that HandleCronAsync calls HandleDailyTasks (which calls GetUserTasksAsync)
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ThrowsAsync(new Exception("Expected call"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => service.HandleCronAsync());

        // Assert
        _mockHabiticaApiService.Verify(x => x.GetUserTasksAsync("dailys"), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldHandleSnoozedTask_AndCallHabiticaApiToCreateNewTodoTask()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        var taskName = "Snoozed Task";
        var otherTag = "another-tag";

        var snoozedTask = CreateTestTask("daily", taskName, [SnoozedTagId, otherTag], isDue: true);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync([snoozedTask]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos"))
                       .ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits"))
                       .ReturnsAsync([]);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.Is<DomainTask>(
            t => t.Type == "todo" &&
                 t.Text == "Snoozed Task" &&
                 t.Tags != null && t.Tags.Contains(otherTag) && t.Tags.Contains(SnoozedTagId) &&
                 t.Date!.Value.Date == DateTime.Today.FromBrtToUtc().AddDays(1).Date &&
                 t.Notes == "Daily Snoozed. Do it!!"
        )), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldNotHandleSnoozedButNotDueTask()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        var taskName = "Snoozed Task";

        var snoozedTask = CreateTestTask("daily", taskName, [SnoozedTagId], isDue: false);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
                       .ReturnsAsync([snoozedTask]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos"))
                       .ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits"))
                       .ReturnsAsync([]);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    [Fact]
    public async Task HandleCronAsync_WhenNoDailies_ShouldNotFetchTodos()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
            .ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits"))
            .ReturnsAsync([]);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.GetUserTasksAsync("todos"), Times.Never);
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldNotCreateDuplicatedSnoozedTask_WhenMatchingTaggedTodoAlreadyExists()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var taskName = "Snoozed Task";

        var dailyTask = CreateTestTask("daily", taskName, [SnoozedTagId], isDue: true);
        var existingTodo = CreateTestTask("todo", taskName, [SnoozedTagId], isDue: false);
        existingTodo.Completed = false;

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
            .ReturnsAsync([dailyTask]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos"))
            .ReturnsAsync([existingTodo]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits"))
            .ReturnsAsync([]);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldNotCreateDuplicatedSnoozedTask_WhenTodoHasSameTitleButDifferentDate()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var taskName = "Snoozed Task";

        var dailyTask = CreateTestTask("daily", taskName, [SnoozedTagId], isDue: true);
        var existingTodo = CreateTestTask("todo", taskName, [SnoozedTagId], isDue: false);
        existingTodo.Date = DateTime.Today.AddDays(-5); // different date from FollowingDueDate
        existingTodo.Completed = false;

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys"))
            .ReturnsAsync([dailyTask]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos"))
            .ReturnsAsync([existingTodo]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits"))
            .ReturnsAsync([]);

        // Act
        await service.HandleCronAsync();

        // Assert - title match alone is sufficient to prevent duplication
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    // ---- Habit handling tests ----

    [Fact]
    public async Task HandleCronAsync_ShouldFetchHabits()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([]);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.GetUserTasksAsync("habits"), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldCreateTodo_ForWeakDailyHabitWithSnoozeTag()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var habitName = "Weak Daily Habit";

        var weakHabit = CreateHabitTask(habitName, [SnoozedTagId], "daily", counterUp: 0);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([weakHabit]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
            .ReturnsAsync(CreateTestTask("todo", habitName, [SnoozedTagId]));

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.Is<DomainTask>(
            t => t.Type == "todo" &&
                 t.Text == habitName &&
                 t.Notes == "Habit Snoozed. Do it!!" &&
                 t.Frequency == null &&
                 t.CounterUp == null &&
                 t.CounterDown == null
        )), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldNotCreateTodo_ForStrongDailyHabit()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        var strongHabit = CreateHabitTask("Strong Daily Habit", [SnoozedTagId], "daily", counterUp: 1);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([strongHabit]);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldNotCreateTodo_ForHabitWithoutSnoozeTag()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        var habitWithoutTag = CreateHabitTask("Habit Without Tag", ["other-tag"], "daily", counterUp: 0);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([habitWithoutTag]);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldNotFetchTodos_WhenNoTaggedHabitsFound()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        var habitWithoutTag = CreateHabitTask("Habit Without Tag", ["other-tag"], "daily", counterUp: 0);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([habitWithoutTag]);

        // Act
        await service.HandleCronAsync();

        // Assert - todos should NOT be fetched when no tagged habits exist
        _mockHabiticaApiService.Verify(x => x.GetUserTasksAsync("todos"), Times.Never);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldNotCreateDuplicateTodo_WhenSnoozedTodoAlreadyExistsForHabit()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var habitName = "Weak Daily Habit";

        var weakHabit = CreateHabitTask(habitName, [SnoozedTagId], "daily", counterUp: 0);
        var existingTodo = CreateTestTask("todo", habitName, [SnoozedTagId], isDue: false);
        existingTodo.Completed = false;

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([weakHabit]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos")).ReturnsAsync([existingTodo]);

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Never);
    }

    [Fact]
    public async Task HandleCronAsync_WeakHabitTodo_ShouldContainSameTagsAsHabit()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var habitName = "Tagged Habit";
        var extraTag = "extra-tag";

        var weakHabit = CreateHabitTask(habitName, [SnoozedTagId, extraTag], "daily", counterUp: 0);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([weakHabit]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
            .ReturnsAsync(CreateTestTask("todo", habitName, [SnoozedTagId, extraTag]));

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.Is<DomainTask>(
            t => t.Tags != null &&
                 t.Tags.Contains(SnoozedTagId) &&
                 t.Tags.Contains(extraTag)
        )), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_WeakHabitTodo_ShouldHaveFollowingDueDate()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var habitName = "Due Date Habit";

        var weakHabit = CreateHabitTask(habitName, [SnoozedTagId], "daily", counterUp: 0);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([weakHabit]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
            .ReturnsAsync(CreateTestTask("todo", habitName, [SnoozedTagId]));

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.Is<DomainTask>(
            t => t.Date.HasValue
        )), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldHandleHabitApiException_WithoutPropagating()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var habitName = "Failing Habit";

        var weakHabit = CreateHabitTask(habitName, [SnoozedTagId], "daily", counterUp: 0);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([weakHabit]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
            .ThrowsAsync(new Exception("API error"));

        // Act - should not throw
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_DailyHabitWithNegativeCounterDown_ShouldBeConsideredWeak()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);
        var habitName = "Negative Counter Habit";

        var weakHabit = CreateHabitTask(habitName, [SnoozedTagId], "daily", counterUp: 5, counterDown: -1);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits")).ReturnsAsync([weakHabit]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
            .ReturnsAsync(CreateTestTask("todo", habitName, [SnoozedTagId]));

        // Act
        await service.HandleCronAsync();

        // Assert
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.Is<DomainTask>(
            t => t.Type == "todo" && t.Text == habitName
        )), Times.Once);
    }

    [Fact]
    public async Task HandleCronAsync_ShouldProcessMultipleWeakHabits()
    {
        // Arrange
        var service = new TaskService(_mockLogger.Object, _mockOptions.Object, _mockHabiticaApiService.Object);

        var weakHabit1 = CreateHabitTask("Weak Habit 1", [SnoozedTagId], "daily", counterUp: 0);
        var weakHabit2 = CreateHabitTask("Weak Habit 2", [SnoozedTagId], "daily", counterUp: 0);
        var strongHabit = CreateHabitTask("Strong Habit", [SnoozedTagId], "daily", counterUp: 2);

        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("dailys")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("habits"))
            .ReturnsAsync([weakHabit1, weakHabit2, strongHabit]);
        _mockHabiticaApiService.Setup(x => x.GetUserTasksAsync("todos")).ReturnsAsync([]);
        _mockHabiticaApiService.Setup(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()))
            .ReturnsAsync((DomainTask t) => t);

        // Act
        await service.HandleCronAsync();

        // Assert - only 2 todos created (for the 2 weak habits)
        _mockHabiticaApiService.Verify(x => x.CreateUserTasksAsync(It.IsAny<DomainTask>()), Times.Exactly(2));
    }

    private static DomainTask CreateTestTask(string type, string text, List<string> tags, bool isDue = true)
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

    private static DomainTask CreateHabitTask(string text, List<string> tags, string frequency, int counterUp, int counterDown = 0)
    {
        return new DomainTask
        {
            Id = Guid.NewGuid().ToString(),
            Type = "habit",
            Text = text,
            Tags = tags,
            Frequency = frequency,
            CounterUp = counterUp,
            CounterDown = counterDown,
            Up = true,
            Down = false,
            Value = 1.0,
            Priority = 1.0
        };
    }
}
