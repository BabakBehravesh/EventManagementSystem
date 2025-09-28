namespace EventManagementSystem.Application.Services.IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using EventManagementSystem.Application.Services;
    using EventManagementSystem.Domain.Models;
    using EventManagementSystem.Infrastructure.Storage;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class ParticipationServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task ParticipateInEventAsync_EventDoesNotExist_ReturnsFailure()
        {
            // Arrange
            using var context = GetInMemoryDbContext(nameof(ParticipateInEventAsync_EventDoesNotExist_ReturnsFailure));
            var service = new ParticipationService(context);

            var registration = new Participation { Email = "test@example.com", Name = "Test User" };

            // Act
            var result = await service.ParticipateInEventAsync(1, registration);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Event not found.", result.Message);
        }

        [Fact]
        public async Task ParticipateInEventAsync_AlreadyRegistered_ReturnsFailure()
        {
            // Arrange
            using var context = GetInMemoryDbContext(nameof(ParticipateInEventAsync_AlreadyRegistered_ReturnsFailure));

            context.Events.Add(new Event { Id = 1, Name = "Test Event", CreatedBy = "1" });
            context.Registrations.Add(new Participation { EventId = 1, Email = "test@example.com", Name = "Existing User" });
            await context.SaveChangesAsync();

            var service = new ParticipationService(context);
            var registration = new Participation { Email = "test@example.com", Name = "New User" };

            // Act
            var result = await service.ParticipateInEventAsync(1, registration);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You have been already refistered to this event.", result.Message);
        }

        [Fact]
        public async Task ParticipateInEventAsync_NewRegistration_Success()
        {
            // Arrange
            using var context = GetInMemoryDbContext(nameof(ParticipateInEventAsync_NewRegistration_Success));

            var ev = new Event { Id = 1, Name = "Test Event", CreatedBy = "1" };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var service = new ParticipationService(context);
            var registration = new Participation { Email = "test@example.com", Name = "New User" };

            // Act
            var result = await service.ParticipateInEventAsync(1, registration);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Registered successfully!", result.Message);
            Assert.NotNull(result.Participation);
            Assert.Equal(1, context.Registrations.Count());
        }

        [Fact]
        public async Task GetParticipantsInEventAsync_ReturnsCorrectRegistrations()
        {
            // Arrange
            using var context = GetInMemoryDbContext(nameof(GetParticipantsInEventAsync_ReturnsCorrectRegistrations));

            var ev1 = new Event { Id = 1, Name = "Event 1", CreatedBy = "1" };
            var ev2 = new Event { Id = 2, Name = "Event 2", CreatedBy = "1" };
            context.Events.AddRange(ev1, ev2);

            context.Registrations.AddRange(
                new Participation { EventId = 1, Email = "a@test.com", Name = "User A" },
                new Participation { EventId = 1, Email = "b@test.com", Name = "User B" },
                new Participation { EventId = 2, Email = "c@test.com", Name = "User C" }
            );

            await context.SaveChangesAsync();

            var service = new ParticipationService(context);

            // Act
            var result = await service.GetParticipantsInEventAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(1, r.EventId));
        }
    }
}
