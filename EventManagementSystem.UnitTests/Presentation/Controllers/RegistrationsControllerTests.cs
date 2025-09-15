using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EventManagementSystem.Presentation.Controllers.UnitTests;

public class RegistrationsControllerTests
{
    private readonly Mock<IRegistrationService> _mockRegistrationService;
    private readonly RegistrationsController _controller;
    private readonly Mock<ILogger<RegistrationsController>> _mockLogger;

    public RegistrationsControllerTests()
    {
        _mockRegistrationService = new Mock<IRegistrationService>();
        _mockLogger = new Mock<ILogger<RegistrationsController>>();
        _controller = new RegistrationsController(_mockRegistrationService.Object);
    }

    [Fact]
    public async Task GetRegistrationsForEvent_ValidEventId_ReturnsRegistrations()
    {
        // Arrange
        var eventId = 1;
        var registrations = new List<Registration>
        {
            new Registration
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "123-456-7890",
                EventId = eventId,
                Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com" }
            },
            new Registration
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
            .Setup(service => service.GetRegistrationsForEventAsync(eventId))
            .ReturnsAsync(registrations);

        // Act
        var result = await _controller.GetRegistrationsForEvent(eventId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as IEnumerable<RegistrationResponse>;

        response.Should().HaveCount(2);
        response.Should().Contain(r => r.Name == "John Doe");
        response.Should().Contain(r => r.Name == "Jane Smith");

        _mockRegistrationService.Verify(service => service.GetRegistrationsForEventAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetRegistrationsForEvent_NoRegistrations_ReturnsEmptyList()
    {
        // Arrange
        var eventId = 1;
        var emptyRegistrations = new List<Registration>();

        _mockRegistrationService
            .Setup(service => service.GetRegistrationsForEventAsync(eventId))
            .ReturnsAsync(emptyRegistrations);

        // Act
        var result = await _controller.GetRegistrationsForEvent(eventId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as IEnumerable<RegistrationResponse>;

        response.Should().BeEmpty();
        _mockRegistrationService.Verify(service => service.GetRegistrationsForEventAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetRegistrationsForEvent_ServiceThrowsException_ThrowsException()
    {
        // Arrange
        var eventId = 1;

        _mockRegistrationService
            .Setup(service => service.GetRegistrationsForEventAsync(eventId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetRegistrationsForEvent(eventId));
        _mockRegistrationService.Verify(service => service.GetRegistrationsForEventAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsRegistrationResponse()
    {
        // Arrange
        var eventId = 1;
        var request = new RegistrationRequest
        (
            Name: "John Doe",
            Email: "john@example.com",
            PhoneNumber: "123-456-7890"
        );

        var registrationEntity = new Registration
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber!,
            EventId = eventId
        };

        var eventEntity = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com" };
        registrationEntity.Event = eventEntity;

        var registrationResult = new RegistrationResult
        (
            Success: true,
            Registration: registrationEntity, 
            Message: "Registration successful"
        );

        _mockRegistrationService
            .Setup(service => service.RegisterForEventAsync(
                eventId,
                It.Is<Registration>(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == request.PhoneNumber)))
            .ReturnsAsync(registrationResult);

        // Act
        var result = await _controller.Register(eventId, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as RegistrationResponse;

        response.Should().NotBeNull();
        response.Id.Should().Be(1);
        response.Name.Should().Be("John Doe");
        response.Email.Should().Be("john@example.com");
        response.PhoneNumber.Should().Be("123-456-7890");
        response.EventId.Should().Be(eventId);
        response.EventName.Should().Be("Test Event");

        _mockRegistrationService.Verify(service => service.RegisterForEventAsync(
            eventId,
            It.Is<Registration>(r =>
                r.Name == request.Name &&
                r.Email == request.Email &&
                r.PhoneNumber == request.PhoneNumber)),
            Times.Once);
    }

    [Fact]
    public async Task Register_NullPhoneNumber_HandlesNullCorrectly()
    {
        // Arrange
        var eventId = 1;
        var request = new RegistrationRequest
        (
            Name: "John Doe",
            Email: "john@example.com",
            PhoneNumber: null
        );

        var registrationEntity = new Registration
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = string.Empty, // Should be handled as empty string
            EventId = eventId,
            Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com" }
        };

        var registrationResult = new RegistrationResult
        (
            Success: true,
            Registration: registrationEntity, 
            Message: "Registration successful"
        );

        _mockRegistrationService
            .Setup(service => service.RegisterForEventAsync(
                eventId,
                It.Is<Registration>(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == string.Empty))) // Should be empty string, not null
            .ReturnsAsync(registrationResult);

        // Act
        var result = await _controller.Register(eventId, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as RegistrationResponse;

        response.Should().NotBeNull();
        response.PhoneNumber.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Register_EmptyPhoneNumber_HandlesEmptyString()
    {
        // Arrange
        var eventId = 1;
        var request = new RegistrationRequest
        (
            Name: "John Doe",
            Email: "john@example.com",
            PhoneNumber: ""
        );

        var registrationEntity = new Registration
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = "",
            EventId = eventId,
            Event = new Event { Id = eventId, Name = "Test Event", CreatedBy = "Bob@gmail.com" }
        };

        var registrationResult = new RegistrationResult
                (
                    Success: true,
                    Registration: registrationEntity,
                    Message: "Registration successful"
                );

        _mockRegistrationService
            .Setup(service => service.RegisterForEventAsync(
                eventId,
                It.Is<Registration>(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == "")))
            .ReturnsAsync(registrationResult);

        // Act
        var result = await _controller.Register(eventId, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as RegistrationResponse;

        response.Should().NotBeNull();
        response.PhoneNumber.Should().Be("");
    }

   

    [Fact]
    public async Task Register_ServiceThrowsException_ThrowsException()
    {
        // Arrange
        var eventId = 1;
        var request = new RegistrationRequest
        (
            Name: "John Doe",
            PhoneNumber: "123-456-7890",
            Email: "john@example.com"
        );

        _mockRegistrationService
            .Setup(service => service.RegisterForEventAsync(eventId, It.IsAny<Registration>()))
            .ThrowsAsync(new Exception("Registration failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.Register(eventId, request));
        _mockRegistrationService.Verify(service => service.RegisterForEventAsync(
            eventId, It.IsAny<Registration>()), Times.Once);
    }
}

// Supporting classes for tests
