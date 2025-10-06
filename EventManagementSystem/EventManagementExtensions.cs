namespace EventManagementSystem
{
    public static class EventManagementExtensions
    {
        public static IServiceCollection AddEventManagementServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddInfrastructureServices(configuration)
                .AddApplicationServices()
                .AddSecurityServices(configuration)
                .AddWebApiServices(configuration);

            return services;
        }
    }
}
