using AutoMapper;
using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EventManagementSystem.Presentation.Controllers.UnitTests;

public class ParticipationsControllerTests
{
    private readonly Mock<IParticipationService> _mockRegistrationService;
    private readonly IMapper mapper;
    private readonly ParticipationsController _controller;
    private readonly Mock<ILogger<ParticipationsController>> _mockLogger;

    public ParticipationsControllerTests()
    {
        _mockRegistrationService = new Mock<IParticipationService>();
        _mockLogger = new Mock<ILogger<ParticipationsController>>();
        _controller = new ParticipationsController(_mockRegistrationService.Object, mapper);
    }

    [Fact]
    public async Task GetParticipantsInEvent_ValidEventId_ReturnsRegistrations()
    {
        // Arrange
        var eventId = 1;
        var registrations = new List<Participation>
        {
            new Participation
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "123-456-7890",
                EventId = eventId,
                Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com" }
            },
            new Participation
            {
                Id = 2,
                Name = "Jane Smith",
                Email = "jane@example.com",
                PhoneNumber = "098-765-4321",
                EventId = eventId,
                Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Alice@gmail.com" }
            }
        };

        _mockRegistrationService
            .Setup(service => service.GetParticipantsInEventAsync(eventId))
            .ReturnsAsync(registrations);

        // Act
        var result = await _controller.GetParticipantsInEvent(eventId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as IEnumerable<ParticipationResponse>;

        response.Should().HaveCount(2);
        response.Should().Contain(r => r.Name == "John Doe");
        response.Should().Contain(r => r.Name == "Jane Smith");

        _mockRegistrationService.Verify(service => service.GetParticipantsInEventAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetParticipantsInEvent_NoParticipants_ReturnsEmptyList()
    {
        // Arrange
        var eventId = 1;
        var emptyRegistrations = new List<Participation>();

        _mockRegistrationService
            .Setup(service => service.GetParticipantsInEventAsync(eventId))
            .ReturnsAsync(emptyRegistrations);

        // Act
        var result = await _controller.GetParticipantsInEvent(eventId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as IEnumerable<ParticipationResponse>;

        response.Should().BeEmpty();
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
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as ParticipationResponse;

        response.Should().NotBeNull();
        response.Id.Should().Be(1);
        response.Name.Should().Be("John Doe");
        response.Email.Should().Be("john@example.com");
        response.PhoneNumber.Should().Be("123-456-7890");
        response.EventId.Should().Be(eventId);
        response.EventName.Should().Be("Test Event");

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
            Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com" }
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
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as ParticipationResponse;

        response.Should().NotBeNull();
        response.PhoneNumber.Should().Be(string.Empty);
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
            Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com" }
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
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as ParticipationResponse;

        response.Should().NotBeNull();
        response?.PhoneNumber.Should().Be("");
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