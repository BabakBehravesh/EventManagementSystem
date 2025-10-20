﻿namespace EventManagementSystem;

using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Infrastructure.Email;
using EventManagementSystem.Infrastructure.QrCode;
using EventManagementSystem.Infrastructure.Security;
using EventManagementSystem.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Serilog;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Security - Singletons
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IQRCodeGeneratorService, QRCodeGeneratorService>();

        // Infrastructure Services - Scoped
        services.AddScoped<DbInitializer>();

        // Infrastructure Services - Transient
        services.AddTransient<IEmailMessageBuilder, EmailMessageBuilder>();

        Log.Information("Configured Infrastructure Services.");

        return services;
    }
}