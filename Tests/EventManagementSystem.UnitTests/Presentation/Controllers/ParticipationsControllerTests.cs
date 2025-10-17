using AutoMapper;
using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Presentation.DTOs;
using EventManagementSystem.UnitTests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EventManagementSystem.Presentation.Controllers.UnitTests;

public class ParticipationsControllerTests : TestBase
{
    private readonly Mock<IParticipationService> _mockRegistrationService;
    private readonly ParticipationsController _controller;
    private readonly Mock<ILogger<ParticipationsController>> _mockLogger;

    public ParticipationsControllerTests()
    {
        _mockRegistrationService = new Mock<IParticipationService>();
        _mockLogger = new Mock<ILogger<ParticipationsController>>();
        _controller = new ParticipationsController(_mockRegistrationService.Object, Mapper);
    }

    [Fact]
    public async Task GetParticipantsInEvent_ValidEventId_ReturnsRegistrations()
    {
        // Arrange
        var eventId = 1;
        var registrations = new List<Participation>
        {
            new() {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "123-456-7890",
                EventId = eventId,
                Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "bob@gmail.com", StartTime = DateTime.UtcNow.AddDays(1) }
            },
            new() {
                Id = 2,
                Name = "Jane Smith",
                Email = "jane@example.com",
                PhoneNumber = "098-765-4321",
                EventId = eventId,
                Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "alice@gmail.com", StartTime = DateTime.UtcNow.AddDays(1)}
            }
        };

        var serviceResult = ServiceResult<IEnumerable<Participation>>.SuccessResult(registrations, "Participants retrieved successfully");

        _mockRegistrationService
            .Setup(service => service.GetParticipantsInEventAsync(eventId))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetParticipantsInEvent(eventId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as ApiResponse<IEnumerable<ParticipationResponse>>;

        response.Data.Should().HaveCount(2);
        response.Data.Should().Contain(r => r.Name == "John Doe");
        response.Data.Should().Contain(r => r.Name == "Jane Smith");

        _mockRegistrationService.Verify(service => service.GetParticipantsInEventAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetParticipantsInEvent_NoParticipants_ReturnsEmptyList()
    {
        // Arrange
        var eventId = 1;
        var emptyRegistrations = new ServiceResult<IEnumerable<Participation>>();

        _mockRegistrationService
            .Setup(service => service.GetParticipantsInEventAsync(eventId))
            .ReturnsAsync(emptyRegistrations);

        // Act
        var result = await _controller.GetParticipantsInEvent(eventId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var NotFoundResult = result as NotFoundObjectResult;
        var response = NotFoundResult?.Value as IEnumerable<ParticipationResponse>;

        response.Should().BeNull();
        _mockRegistrationService.Verify(service => service.GetParticipantsInEventAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetParticipantsForEvent_ServiceThrowsException_ThrowsException()
    {
        // Arrange
        var eventId = 1;

        _mockRegistrationService
            .Setup(service => service.GetParticipantsInEventAsync(eventId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetParticipantsInEvent(eventId));
        _mockRegistrationService.Verify(service => service.GetParticipantsInEventAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Participate_ValidRequest_ReturnsParticipationResponse()
    {
        // Arrange
        var eventId = 1;
        var request = new ParticipationRequest
        (
            Name: "John Doe",
            Email: "john@example.com",
            PhoneNumber: "123-456-7890"
        );

        var registrationEntity = new Participation
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber!,
            EventId = eventId
        };

        var eventEntity = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com" };
        registrationEntity.Event = eventEntity;

        var registrationResult = ServiceResult<Participation>.SuccessResult
        (
            registrationEntity, 
            "Participation successful"
        );

        _mockRegistrationService
            .Setup(service => service.ParticipateInEventAsync(
                eventId,
                It.Is<Participation>(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == request.PhoneNumber)))
            .ReturnsAsync(registrationResult);

        // Act
        var result = await _controller.Participate(eventId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as ApiResponse<ParticipationResponse>;

        response.Should().NotBeNull();
        response.Data.Id.Should().Be(1);
        response.Data.Name.Should().Be("John Doe");
        response.Data.Email.Should().Be("john@example.com");
        response.Data.PhoneNumber.Should().Be("123-456-7890");
        response.Data.EventId.Should().Be(eventId);
        response.Data.EventName.Should().Be("Test Event");

        _mockRegistrationService.Verify(service => service.ParticipateInEventAsync(
            eventId,
            It.Is<Participation>(r =>
                r.Name == request.Name &&
                r.Email == request.Email &&
                r.PhoneNumber == request.PhoneNumber)),
            Times.Once);
    }

    [Fact]
    public async Task Participate_NullPhoneNumber_HandlesNullCorrectly()
    {
        // Arrange
        var eventId = 1;
        var request = new ParticipationRequest
        (
            Name: "John Doe",
            Email: "john@example.com",
            PhoneNumber: null
        );

        var registrationEntity = new Participation
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = string.Empty, // Should be handled as empty string
            EventId = eventId,
            Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "bob@gmail.com" }
        };

        var registrationResult = ServiceResult<Participation>.SuccessResult
        (
            registrationEntity, 
            "Participation successful"
        );

        _mockRegistrationService
            .Setup(service => service.ParticipateInEventAsync(
                eventId,
                It.Is<Participation>(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == string.Empty))) 
            .ReturnsAsync(registrationResult);

        // Act
        var result = await _controller.Participate(eventId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as ApiResponse<ParticipationResponse>;

        response.Should().NotBeNull();
        response.Data.PhoneNumber.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Participate_EmptyPhoneNumber_HandlesEmptyString()
    {
        // Arrange
        var eventId = 1;
        var request = new ParticipationRequest
        (
            Name: "John Doe",
            Email: "john@example.com",
            PhoneNumber: ""
        );

        var registrationEntity = new Participation
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = "",
            EventId = eventId,
            Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com", StartTime = DateTime.UtcNow.AddDays(1) }
        };

        var registrationResult = ServiceResult<Participation>.SuccessResult
                (
                    registrationEntity,
                    "Participation successful"
                );

        _mockRegistrationService
            .Setup(service => service.ParticipateInEventAsync(
                eventId,
                It.Is<Participation>(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == "")))
            .ReturnsAsync(registrationResult);

        // Act
        var result = await _controller.Participate(eventId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as ApiResponse<ParticipationResponse>;

        response.Data.Should().NotBeNull();
        response?.Data?.PhoneNumber.Should().Be("");
    }   

    [Fact]
    public async Task Participate_ServiceThrowsException_ThrowsException()
    {
        // Arrange
        var eventId = 1;
        var request = new ParticipationRequest
        (
            Name: "John Doe",
            PhoneNumber: "123-456-7890",
            Email: "john@example.com"
        );

        _mockRegistrationService
            .Setup(service => service.ParticipateInEventAsync(eventId, It.IsAny<Participation>()))
            .ThrowsAsync(new Exception("Participation failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.Participate(eventId, request));
        _mockRegistrationService.Verify(service => service.ParticipateInEventAsync(
            eventId, It.IsAny<Participation>()), Times.Once);
    }
}