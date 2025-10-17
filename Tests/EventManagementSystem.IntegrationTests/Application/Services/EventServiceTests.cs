using EventManagementSystem.Application.Services;
using EventManagementSystem.Domain.Models;
using FluentAssertions;

namespace EventManagementSystem.IntegrationTests.Application.Services;

public class EventServiceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly FakeEmailService _fakeEmailService;
    private readonly IServiceProvider _services;
    public EventServiceTests(CustomWebApplicationFactory factory)
    {
        _services = factory.Services;
        _fakeEmailService = factory.FakeEmailService;
    }

    [Fact]
    public async Task CreateEventAsync_ValidEvent_ReturnsSuccess()
    {
        // Arrange
        using var context = CustomWebApplicationFactory.GetInMemoryDbContext(nameof(CreateEventAsync_ValidEvent_ReturnsSuccess));
        var service = new EventService(context);
        var createdBy = "1";
        var newEvent = new Event { Name = "New Event", CreatedBy = createdBy, StartTime = DateTime.UtcNow.AddDays(1) };

        // Act
        var result = await service.CreateEventAsync(newEvent, createdBy);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Event created successfully!", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal<Event>(newEvent, result.Data);
    }

    [Fact]
    public async Task CreateEventAsync_DuplicateEvent_ReturnsFailure()
    {
        // Arrange
        using var context = CustomWebApplicationFactory.GetInMemoryDbContext(nameof(CreateEventAsync_DuplicateEvent_ReturnsFailure));
        var service = new EventService(context);
        var createdBy = "1";
        var existingEvent = new Event { Name = "Existing Event", Location = "Same location", CreatedBy = createdBy, StartTime = DateTime.UtcNow.AddDays(1) };
        context.Events.Add(existingEvent);
        await context.SaveChangesAsync();
        var newEvent = new Event { Name = "Existing Event", Location = "Same location", CreatedBy = createdBy, StartTime = existingEvent.StartTime };

        // Act
        var result = await service.CreateEventAsync(newEvent, createdBy);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("An event with the same name, start time and location already exists.", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetEventByIdAsync_EventExists_ReturnsSuccess()
    {
        // Arrange
        using var context = CustomWebApplicationFactory.GetInMemoryDbContext(nameof(GetEventByIdAsync_EventExists_ReturnsSuccess));
        var service = new EventService(context);
        var existingEvent = new Event { Id = 1, Name = "Existing Event", CreatedBy = "1", StartTime = DateTime.UtcNow.AddDays(1) };
        context.Events.Add(existingEvent);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventByIdAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(existingEvent, result.Data);
    }

    [Fact]
    public async Task GetEventByIdAsync_EventDoesNotExist_ReturnsFailure()
    {
        // Arrange
        using var context = CustomWebApplicationFactory.GetInMemoryDbContext(nameof(GetEventByIdAsync_EventDoesNotExist_ReturnsFailure));
        var service = new EventService(context);

        // Act
        var result = await service.GetEventByIdAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Event with ID 1 was not found.", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetEventParticipantsAsync_EventExists_ReturnsSuccess()
    {
        // Arrange
        using var context = CustomWebApplicationFactory.GetInMemoryDbContext(nameof(GetEventParticipantsAsync_EventExists_ReturnsSuccess));
        var service = new EventService(context);
        var existingEvent = new Event { Id = 1, Name = "Existing Event", CreatedBy = "1", StartTime = DateTime.UtcNow.AddDays(1) };
        context.Events.Add(existingEvent);

        var participant = new List<Participation>
        {
            new Participation { EventId = 1, Email = "test_1@gmail.com", Name = "Test User 1" },
            new Participation { EventId = 1, Email = "test_2@gmail.com", Name = "Test User 2" } 
        };
        await context.Participations.AddRangeAsync(participant);

        await context.SaveChangesAsync();
        
        // Act
        var result = await service.GetEventParticipantsAsync(1);
        
        // Assert
        result.Data.Should().BeEquivalentTo(
            participant, 
            options => options.Excluding(p => p.Event)
        );
    }

    [Fact]
    public async Task GetEventParticipantsAsync_EventDoesNotExist_ReturnsFailure()
    {
        // Arrange
        using var context = CustomWebApplicationFactory.GetInMemoryDbContext(nameof(GetEventParticipantsAsync_EventDoesNotExist_ReturnsFailure));
        var service = new EventService(context);

        // Act
        var result = await service.GetEventParticipantsAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Event with Id '1' not found", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsPaginatedEvents()
    {
        // Arrange
        using var context = CustomWebApplicationFactory.GetInMemoryDbContext(nameof(GetEventsAsync_ReturnsPaginatedEvents));
        var service = new EventService(context);
        for (int i = 1; i <= 15; i++)
        {
            context.Events.Add(new Event { Id = i, Name = $"Event {i}", CreatedBy = "1", StartTime = DateTime.UtcNow.AddDays(i) });
        }
        await context.SaveChangesAsync();
        int pageNumber = 2;
        int pageSize = 5;

        // Act
        var (events, totalCount) = await service.GetEventsAsync(pageNumber, pageSize);

        // Assert
        Assert.Equal(15, totalCount);
        Assert.Equal(pageSize, events.Count());
        Assert.All(events, e => Assert.InRange(e.Id, 6, 10));
    }
}
