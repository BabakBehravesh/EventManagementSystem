namespace EventManagementSystem;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Infrastructure.Email;
using EventManagementSystem.Infrastructure.QrCode;
using EventManagementSystem.Infrastructure.Security;
using EventManagementSystem.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Serilog;

public static class InfrastructureExtensions
{
    private static string _cachedConnectionString = string.Empty;

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(GetApplicationConnectionString(configuration)));

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

    private static string GetApplicationConnectionString(IConfiguration configuration)
    {

        if (_cachedConnectionString != null)
        {
            return _cachedConnectionString;
        }

        var keyVaultUrl = configuration["AzureKeyVault:Uri"];
        var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

        var environmet = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production").ToLower();

        try
        {
            // Get all secrets in parallel to reduce startup time
            var serverTask = client.GetSecretAsync("sql-server-" + environmet);
            var databaseTask = client.GetSecretAsync("sql-database-" + environmet);
            var userIdTask = client.GetSecretAsync("sql-username-" + environmet);
            var passwordTask = client.GetSecretAsync("sql-password-" + environmet);

            Task.WaitAll(serverTask, databaseTask, userIdTask, passwordTask);

            var server = serverTask.Result.Value.Value;
            var database = databaseTask.Result.Value.Value;
            var userId = userIdTask.Result.Value.Value;
            var password = passwordTask.Result.Value.Value;

            _cachedConnectionString = $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate=True;";

            return _cachedConnectionString;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve connection string from Key Vault");
            throw;
        }
    }
}
