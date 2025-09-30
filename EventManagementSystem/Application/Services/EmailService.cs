using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Infrastructure.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EventManagementSystem.Application.Services;

public class EmailService : IEmailService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly EmailMessageBuilder _builder;

    public EmailService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
        _builder = new EmailMessageBuilder(environment);
    }

    public async Task SendWelcomeEmailAsync(string recipientEmail, string recipientName, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessageBuilder(_environment)
            .SetFrom(_configuration["EmailSettings:FromName"], _configuration["EmailSettings:Username"])
            .AddTo(recipientName, recipientEmail)
            .SetSubject($"Welcome to Our App, {recipientName}!")
            .AddImageLogo()
            .SetWelcomeTemplate(recipientName)
            .AddAttachment(Path.Combine(_environment.ContentRootPath, "README.md"))
            .Build();

        await SendEmailAsync(message, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string resetLink, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessageBuilder(_environment)
            .SetFrom(_configuration["EmailSettings:FromName"], _configuration["EmailSettings:Username"])
            .AddTo(recipientName, recipientEmail)
            .SetSubject("Password Reset Request")
            .AddImageLogo()
            .SetPasswordResetTemplate(recipientName, resetLink)
            .Build();

        await SendEmailAsync(message, cancellationToken);
    }

    public async Task SendCustomEmailAsync(string recipientEmail, string recipientName, string subject, string htmlContent, string attachmentPath = null, CancellationToken cancellationToken = default)
    {
        var builder = new EmailMessageBuilder(_environment)
            .SetFrom(_configuration["EmailSettings:FromName"], _configuration["EmailSettings:Username"])
            .AddTo(recipientName, recipientEmail)
            .SetSubject(subject)
            .AddImageLogo()
            .SetHtmlBody(htmlContent);

        if (!string.IsNullOrEmpty(attachmentPath))
        {
            builder.AddAttachment(attachmentPath);
        }

        var message = builder.Build();
        await SendEmailAsync(message, cancellationToken);
    }

    public async Task SendPasswordChangeConfirmationEmailAsync(string recipientEmail, string recipientName, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessageBuilder(_environment)
            .SetFrom(_configuration["EmailSettings:FromName"], _configuration["EmailSettings:Username"])
            .AddTo(recipientName, recipientEmail)
            .SetSubject("Password Changed Successfully")
            .AddImageLogo()
            .SetPasswordChangeConfirmationTemplate(recipientName)
            .Build();

        await SendEmailAsync(message, cancellationToken);
    }

    public async Task SendAccountCreatedEmailAsync(string recipientEmail, string recipientName, string temporaryPassword, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessageBuilder(_environment)
            .SetFrom(_configuration["EmailSettings:FromName"], _configuration["EmailSettings:Username"])
            .AddTo(recipientName, recipientEmail)
            .SetSubject("Your Account Has Been Created")
            .AddImageLogo()
            .SetAccountCreatedTemplate(recipientName, temporaryPassword, _configuration["Frontend:BaseUrl"])
            .Build();

        await SendEmailAsync(message, cancellationToken);
    }

    private async Task SendEmailAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient();

        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]!);
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);

            Console.WriteLine($"✅ Email sent to {message.To}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending email: {ex.Message}");
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
    }


}