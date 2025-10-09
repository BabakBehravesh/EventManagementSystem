using EventManagementSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;


namespace EventManagementSystem.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public FakeEmailService FakeEmailService { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEmailService));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddTransient<IEmailService, FakeEmailService>();
            });
        }
    }
}
