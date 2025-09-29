namespace EventManagementSystem.Domain.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string recipientEmail, string recipientName, CancellationToken cancellationToken = default);
    
    Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string resetLink, CancellationToken cancellationToken = default);
    
    Task SendCustomEmailAsync(string recipientEmail, string recipientName, string subject, string htmlContent, string attachmentPath = null, 
        CancellationToken cancellationToken = default);

    Task SendPasswordChangeConfirmationEmailAsync(string recipientEmail, string recipientName, CancellationToken cancellationToken = default);

    Task SendAccountCreatedEmailAsync(string email, string userName, string password, CancellationToken cancellationToken = default);
}

