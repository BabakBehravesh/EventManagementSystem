using EventManagementSystem.Application.Services;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Infrastructure.Storage;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace EventManagementSystem.UnitTests.Application.Services;

public class EventServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly EventService _eventService;
    private readonly Mock<DbSet<Event>> _mockEventsSet;
    private readonly Mock<DbSet<Registration>> _mockRegistrationsSet;

    private readonly List<Event> _fakeEvents;
    private readonly List<Registration> _fakeRegistrations;

    public EventServiceTests()
    {
        // Arrange: Set up the fake data
        _fakeEvents = new List<Event>
        {
            new() { Id = 1, Name = "Tech Conference", Description = "A conference about tech", CreatedBy = "user1" },
            new() { Id = 2, Name = "Music Festival", Description = "Annual music festival", CreatedBy = "user2" }
        };

        _fakeRegistrations = new List<Registration>
        {
            new() { Id = 1, Name = "Alice", Email = "alice@email.com", EventId = 1},
            new() { Id = 2, Name = "Bob", Email = "bob@email.com", EventId = 1},
            new() { Id = 3, Name = "Charlie", Email = "charlie@email.com", EventId = 2 }
        };

        // Create mock DbSets
        _mockEventsSet = _fakeEvents.AsQueryable().BuildMockDbSet();
        _mockRegistrationsSet = _fakeRegistrations.AsQueryable().BuildMockDbSet();

        // Mock the async operations
        _mockEventsSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                     .ReturnsAsync((object[] ids) => _fakeEvents.FirstOrDefault(e => e.Id == (int)ids[0]));

        // Create mock DbContext
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockContext = new Mock<ApplicationDbContext>(options);
        _mockContext.Setup(c => c.Events).Returns(_mockEventsSet.Object);
        _mockContext.Setup(c => c.Registrations).Returns(_mockRegistrationsSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1); // Simulate saving 1 entity

        _eventService = new EventService(_mockContext.Object);
    }

    [Fact]
    public async Task GetEventParticipantsAsync_ExistingEventName_ReturnsParticipants()
    {
        // Arrange
        var eventName = 1; //"Tech Conference";

        // Act
        var result = await _eventService.GetEventParticipantsAsync(eventName);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Alice");
        result.Should().Contain(r => r.Name == "Bob");
    }

    [Fact]
    public async Task GetEventParticipantsAsync_NonExistentEventName_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentEventName = 10000; // "Non-Existent Event";

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _eventService.GetEventParticipantsAsync(nonExistentEventName));
    }

    [Fact]
    public async Task CreateEventAsync_ValidEvent_CreatesEventWithCreatorId()
    {
        // Arrange
        var newEvent = new Event
        {
            Name = "New Event",
            Description = "New description",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
            CreatedBy = "C01"
        };
        var creatorId = "test-user-id";

        // Act
        var result = await _eventService.CreateEventAsync(newEvent);

        // Assert
        result.Should().NotBeNull();
        result.CreatedBy.Should().Be(creatorId);
        _mockContext.Verify(c => c.Events.Add(It.IsAny<Event>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}