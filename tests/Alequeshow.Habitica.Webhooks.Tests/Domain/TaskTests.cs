using Alequeshow.Habitica.Webhooks.Domain;
using DomainTask = Alequeshow.Habitica.Webhooks.Domain.Task;

namespace Alequeshow.Habitica.Webhooks.Tests.Domain;

public class TaskTests
{
    [Fact]
    public void IsDaily_WhenTypeIsDaily_ReturnsTrue()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Daily Task"
        };

        // Act
        var result = task.IsDaily();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDaily_WhenTypeIsDailyDifferentCasing_ReturnsTrue()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "DAILY",
            Text = "Test Daily Task"
        };

        // Act
        var result = task.IsDaily();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("todo")]
    [InlineData("habit")]
    [InlineData("reward")]
    [InlineData("")]
    [InlineData("other")]
    public void IsDaily_WhenTypeIsNotDaily_ReturnsFalse(string type)
    {
        // Arrange
        var task = new DomainTask
        {
            Type = type,
            Text = "Test Task"
        };

        // Act
        var result = task.IsDaily();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDueInDate_WhenIsDueTrueAndNotCompleted_ReturnsTrue()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            IsDue = true,
            Completed = false
        };

        // Act
        var result = task.IsDueInDate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDueInDate_WhenIsDueTrueButCompleted_ReturnsFalse()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            IsDue = true,
            Completed = true
        };

        // Act
        var result = task.IsDueInDate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDueInDate_WhenIsDueFalse_ReturnsFalse()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            IsDue = false,
            Completed = false
        };

        // Act
        var result = task.IsDueInDate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDueInDate_WithHistoryEntryForSameDateIsDueNotCompleted_ReturnsTrue()
    {
        // Arrange
        var testDate = new DateTime(2025, 9, 16);
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            IsDue = false,
            Completed = false,
            History = new List<History>
            {
                new History
                {
                    Date = testDate,
                    IsDue = true,
                    Completed = false
                }
            }
        };

        // Act
        var result = task.IsDueInDate(testDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDueInDate_WithHistoryEntryForSameDateIsDueButCompleted_ReturnsFalse()
    {
        // Arrange
        var testDate = new DateTime(2025, 9, 16);
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            IsDue = false,
            Completed = false,
            History = new List<History>
            {
                new History
                {
                    Date = testDate,
                    IsDue = true,
                    Completed = true
                }
            }
        };

        // Act
        var result = task.IsDueInDate(testDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDueInDate_WithHistoryEntryNotDue_FallsBackToTaskProperties()
    {
        // Arrange
        var testDate = new DateTime(2025, 9, 16);
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            IsDue = true,
            Completed = false,
            History = new List<History>
            {
                new History
                {
                    Date = testDate,
                    IsDue = false,
                    Completed = false
                }
            }
        };

        // Act
        var result = task.IsDueInDate(testDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDueInDate_WithNullDate_UsesTodaysDate()
    {
        // Arrange
        var today = DateTime.Today;
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            IsDue = true,
            Completed = false,
            History = new List<History>
            {
                new History
                {
                    Date = today,
                    IsDue = true,
                    Completed = false
                }
            }
        };

        // Act
        var result = task.IsDueInDate(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetLastHistoryEntry_WhenHistoryIsNull_ReturnsNull()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            History = null
        };

        // Act
        var result = task.GetLastHistoryEntry();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetLastHistoryEntry_WhenHistoryIsEmpty_ReturnsNull()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            History = new List<History>()
        };

        // Act
        var result = task.GetLastHistoryEntry();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetLastHistoryEntry_WithSingleHistoryEntry_ReturnsThatEntry()
    {
        // Arrange
        var historyEntry = new History
        {
            Date = new DateTime(2025, 9, 15),
            IsDue = true,
            Completed = false
        };

        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            History = new List<History> { historyEntry }
        };

        // Act
        var result = task.GetLastHistoryEntry(new DateTime(2025, 9, 16));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(historyEntry.Date, result.Date);
        Assert.Equal(historyEntry.IsDue, result.IsDue);
        Assert.Equal(historyEntry.Completed, result.Completed);
    }

    [Fact]
    public void GetLastHistoryEntry_WithMultipleEntries_ReturnsLatestBeforeOrOnDate()
    {
        // Arrange
        var referenceDate = new DateTime(2025, 9, 16);
        var oldestEntry = new History { Date = new DateTime(2025, 9, 14), IsDue = true };
        var middleEntry = new History { Date = new DateTime(2025, 9, 15), IsDue = false };
        var latestEntry = new History { Date = new DateTime(2025, 9, 16), IsDue = true };
        var futureEntry = new History { Date = new DateTime(2025, 9, 17), IsDue = false };

        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            History = new List<History> { oldestEntry, futureEntry, middleEntry, latestEntry }
        };

        // Act
        var result = task.GetLastHistoryEntry(referenceDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(latestEntry.Date, result.Date);
        Assert.Equal(latestEntry.IsDue, result.IsDue);
    }

    [Fact]
    public void GetLastHistoryEntry_WithNullDate_UsesTodaysDate()
    {
        // Arrange
        var today = DateTime.Today;
        var yesterdayEntry = new History { Date = today.AddDays(-1), IsDue = true };
        var todayEntry = new History { Date = today, IsDue = false };
        var tomorrowEntry = new History { Date = today.AddDays(1), IsDue = true };

        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            History = new List<History> { yesterdayEntry, tomorrowEntry, todayEntry }
        };

        // Act
        var result = task.GetLastHistoryEntry();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todayEntry.Date, result.Date);
        Assert.Equal(todayEntry.IsDue, result.IsDue);
    }

    [Fact]
    public void GetLastHistoryEntry_WhenAllEntriesAfterDate_ReturnsNull()
    {
        // Arrange
        var referenceDate = new DateTime(2025, 9, 10);
        var futureEntry1 = new History { Date = new DateTime(2025, 9, 15), IsDue = true };
        var futureEntry2 = new History { Date = new DateTime(2025, 9, 16), IsDue = false };

        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            History = new List<History> { futureEntry1, futureEntry2 }
        };

        // Act
        var result = task.GetLastHistoryEntry(referenceDate);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void HasTag_WhenTagsIsNull_ReturnsFalse()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Tags = null
        };

        // Act
        var result = task.HasTag("test-tag");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasTag_WhenTagsIsEmpty_ReturnsFalse()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Tags = new List<string>()
        };

        // Act
        var result = task.HasTag("test-tag");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasTag_WhenTagExists_ReturnsTrue()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Tags = new List<string> { "tag1", "test-tag", "tag3" }
        };

        // Act
        var result = task.HasTag("test-tag");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasTag_WhenTagDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Tags = new List<string> { "tag1", "tag2", "tag3" }
        };

        // Act
        var result = task.HasTag("nonexistent-tag");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("test-tag")]
    [InlineData("TEST-TAG")]
    public void HasTag_WithDifferentTagValues_ReturnsCorrectResult(string tagToFind)
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Tags = new List<string> { "test-tag", "another-tag" }
        };

        // Act
        var result = task.HasTag(tagToFind);

        // Assert
        var expected = task.Tags.Contains(tagToFind);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteNotes_WithNullNotes_DoesNotModifyNotes()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = "Original notes"
        };

        // Act
        task.WriteNotes(null!);

        // Assert
        Assert.Equal("Original notes", task.Notes);
    }

    [Fact]
    public void WriteNotes_WithEmptyNotesArray_DoesNotModifyNotes()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = "Original notes"
        };

        // Act
        task.WriteNotes();

        // Assert
        Assert.Equal("Original notes", task.Notes);
    }

    [Fact]
    public void WriteNotes_WithSingleNote_WhenNotesIsNull_SetsNote()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = null
        };

        // Act
        task.WriteNotes("New note");

        // Assert
        Assert.Equal("New note", task.Notes);
    }

    [Fact]
    public void WriteNotes_WithSingleNote_WhenNotesIsEmpty_SetsNote()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = ""
        };

        // Act
        task.WriteNotes("New note");

        // Assert
        Assert.Equal("New note", task.Notes);
    }

    [Fact]
    public void WriteNotes_WithSingleNote_WhenNotesExists_AppendsNote()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = "Existing notes"
        };

        // Act
        task.WriteNotes("New note");

        // Assert
        Assert.Equal("Existing notes\nNew note", task.Notes);
    }

    [Fact]
    public void WriteNotes_WithMultipleNotes_WhenNotesIsNull_SetsAllNotes()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = null
        };

        // Act
        task.WriteNotes("Note 1", "Note 2", "Note 3");

        // Assert
        Assert.Equal("Note 1\nNote 2\nNote 3", task.Notes);
    }

    [Fact]
    public void WriteNotes_WithMultipleNotes_WhenNotesExists_AppendsAllNotes()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = "Existing notes"
        };

        // Act
        task.WriteNotes("Note 1", "Note 2");

        // Assert
        Assert.Equal("Existing notes\nNote 1\nNote 2", task.Notes);
    }

    [Fact]
    public void WriteNotes_WithEmptyStringInNotes_IncludesEmptyLine()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = null
        };

        // Act
        task.WriteNotes("Note 1", "", "Note 3");

        // Assert
        Assert.Equal("Note 1\n\nNote 3", task.Notes);
    }

    [Fact]
    public void WriteNotes_WithWhitespaceNote_PreservesWhitespace()
    {
        // Arrange
        var task = new DomainTask
        {
            Type = "daily",
            Text = "Test Task",
            Notes = null
        };

        // Act
        task.WriteNotes("  Note with spaces  ");

        // Assert
        Assert.Equal("  Note with spaces  ", task.Notes);
    }

    private DomainTask CreateTestTask(string type = "daily", string text = "Test Task")
    {
        return new DomainTask
        {
            Id = Guid.NewGuid().ToString(),
            Type = type,
            Text = text,
            Value = 1.0,
            Priority = 1.0
        };
    }

    private History CreateHistoryEntry(DateTime date, bool? isDue = null, bool? completed = null)
    {
        return new History
        {
            Date = date,
            IsDue = isDue,
            Completed = completed
        };
    }

    [Fact]
    public void IsHabit_WhenTypeIsHabit_ReturnsTrue()
    {
        var task = new DomainTask { Type = "habit", Text = "Test Habit" };
        Assert.True(task.IsHabit());
    }

    [Fact]
    public void IsHabit_WhenTypeIsHabitUpperCase_ReturnsTrue()
    {
        var task = new DomainTask { Type = "HABIT", Text = "Test Habit" };
        Assert.True(task.IsHabit());
    }

    [Theory]
    [InlineData("daily")]
    [InlineData("todo")]
    [InlineData("reward")]
    [InlineData("")]
    public void IsHabit_WhenTypeIsNotHabit_ReturnsFalse(string type)
    {
        var task = new DomainTask { Type = type, Text = "Test Task" };
        Assert.False(task.IsHabit());
    }

    [Fact]
    public void IsWeakHabit_WhenTypeIsNotHabit_ReturnsFalse()
    {
        var task = new DomainTask { Type = "daily", Text = "Test", Frequency = "daily", CounterUp = 0 };
        Assert.False(task.IsWeakHabit(DateTime.Today));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, -1)]
    public void IsWeakHabit_DailyHabit_ReturnsTrueWhenCounterUpIsZeroOrCounterDownIsNegative(int counterUp, int counterDown)
    {
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "daily",
            CounterUp = counterUp,
            CounterDown = counterDown
        };
        Assert.True(task.IsWeakHabit(DateTime.Today));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(5, 0)]
    public void IsWeakHabit_DailyHabit_ReturnsFalseWhenCounterUpIsPositive(int counterUp, int counterDown)
    {
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "daily",
            CounterUp = counterUp,
            CounterDown = counterDown
        };
        Assert.False(task.IsWeakHabit(DateTime.Today));
    }

    [Fact]
    public void IsWeakHabit_WeeklyHabit_ReturnsTrueOnSaturdayWithLowCounter()
    {
        var saturday = GetNextDayOfWeek(DayOfWeek.Saturday);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "weekly",
            CounterUp = 1,
            CounterDown = 0
        };
        Assert.True(task.IsWeakHabit(saturday));
    }

    [Fact]
    public void IsWeakHabit_WeeklyHabit_ReturnsTrueOnSaturdayWithZeroCounter()
    {
        var saturday = GetNextDayOfWeek(DayOfWeek.Saturday);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "weekly",
            CounterUp = 0,
            CounterDown = 0
        };
        Assert.True(task.IsWeakHabit(saturday));
    }

    [Fact]
    public void IsWeakHabit_WeeklyHabit_ReturnsFalseOnSaturdayWithSufficientCounter()
    {
        var saturday = GetNextDayOfWeek(DayOfWeek.Saturday);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "weekly",
            CounterUp = 2,
            CounterDown = 0
        };
        Assert.False(task.IsWeakHabit(saturday));
    }

    [Theory]
    [InlineData(DayOfWeek.Sunday)]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    public void IsWeakHabit_WeeklyHabit_ReturnsFalseOnNonSaturdayRegardlessOfCounter(DayOfWeek dayOfWeek)
    {
        var nonSaturday = GetNextDayOfWeek(dayOfWeek);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "weekly",
            CounterUp = 0,
            CounterDown = 0
        };
        Assert.False(task.IsWeakHabit(nonSaturday));
    }

    [Fact]
    public void IsWeakHabit_WeeklyHabit_ReturnsTrueOnSaturdayWithNegativeCounterDown()
    {
        var saturday = GetNextDayOfWeek(DayOfWeek.Saturday);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "weekly",
            CounterUp = 5,
            CounterDown = -1
        };
        Assert.True(task.IsWeakHabit(saturday));
    }

    [Fact]
    public void IsWeakHabit_MonthlyHabit_ReturnsTrueOnLastDayOfMonthWithLowCounter()
    {
        var lastDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
            DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "monthly",
            CounterUp = 1,
            CounterDown = 0
        };
        Assert.True(task.IsWeakHabit(lastDayOfMonth));
    }

    [Fact]
    public void IsWeakHabit_MonthlyHabit_ReturnsTrueOnLastDayWithZeroCounter()
    {
        var lastDayOfMonth = new DateTime(2026, 1, 31);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "monthly",
            CounterUp = 0,
            CounterDown = 0
        };
        Assert.True(task.IsWeakHabit(lastDayOfMonth));
    }

    [Fact]
    public void IsWeakHabit_MonthlyHabit_ReturnsFalseOnLastDayWithSufficientCounter()
    {
        var lastDayOfMonth = new DateTime(2026, 1, 31);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "monthly",
            CounterUp = 2,
            CounterDown = 0
        };
        Assert.False(task.IsWeakHabit(lastDayOfMonth));
    }

    [Fact]
    public void IsWeakHabit_MonthlyHabit_ReturnsFalseOnNonLastDayEvenWithLowCounter()
    {
        var nonLastDay = new DateTime(2026, 1, 15);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "monthly",
            CounterUp = 0,
            CounterDown = 0
        };
        Assert.False(task.IsWeakHabit(nonLastDay));
    }

    [Fact]
    public void IsWeakHabit_MonthlyHabit_ReturnsTrueOnLastDayWithNegativeCounterDown()
    {
        var lastDayOfMonth = new DateTime(2026, 1, 31);
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "monthly",
            CounterUp = 5,
            CounterDown = -1
        };
        Assert.True(task.IsWeakHabit(lastDayOfMonth));
    }

    [Fact]
    public void IsWeakHabit_WithNullCounters_TreatsAsZero()
    {
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "daily",
            CounterUp = null,
            CounterDown = null
        };
        Assert.True(task.IsWeakHabit(DateTime.Today));
    }

    [Fact]
    public void IsWeakHabit_WithUnknownFrequency_ReturnsFalse()
    {
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "unknown",
            CounterUp = 0,
            CounterDown = 0
        };
        Assert.False(task.IsWeakHabit(DateTime.Today));
    }

    [Fact]
    public void IsWeakHabit_WithNullFrequency_ReturnsFalse()
    {
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = null,
            CounterUp = 0,
            CounterDown = 0
        };
        Assert.False(task.IsWeakHabit(DateTime.Today));
    }

    [Fact]
    public void IsWeakHabit_WithNullDate_UsesToday()
    {
        // A daily habit with counterUp = 0 should always return true regardless of date
        var task = new DomainTask
        {
            Type = "habit",
            Text = "Test",
            Frequency = "daily",
            CounterUp = 0,
            CounterDown = 0
        };
        Assert.True(task.IsWeakHabit(null));
    }

    private static DateTime GetNextDayOfWeek(DayOfWeek targetDay)
    {
        var today = DateTime.Today;
        int daysUntilTarget = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;
        return today.AddDays(daysUntilTarget == 0 ? 0 : daysUntilTarget);
    }
}
