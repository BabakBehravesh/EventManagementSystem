namespace EventManagementSystem;

using Microsoft.OpenApi.Models;

public static class WebApiExtensions
{
    public static IServiceCollection AddWebApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // CORS
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!;

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            );
        });

        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; // 100MB
            options.CompactionPercentage = 0.25;
            options.ExpirationScanFrequency = TimeSpan.FromSeconds(30);
        });

        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 1024 * 1024;
            options.UseCaseSensitivePaths = false;
            options.SizeLimit = 100 * 1024 * 1024;
        });

        // Controllers
        services.AddControllers();

        // Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Registration API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
