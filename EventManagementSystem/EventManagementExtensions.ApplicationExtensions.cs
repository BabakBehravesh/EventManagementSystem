namespace EventManagementSystem;

using EventManagementSystem.Application.Profiles;
using EventManagementSystem.Application.Services;
using EventManagementSystem.Domain.Interfaces;


public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application Services - Scoped
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<IEmailService, EmailService>();

        // AutoMapper
        services.AddAutoMapper(typeof(EventMappingProfile));

        return services;
    }
}
