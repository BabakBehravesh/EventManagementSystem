using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Infrastructure.QrCode;
using System;
using System.Collections.Concurrent;

public class FakeEmailService : IEmailService
{
    public ConcurrentBag<(string RecipientEmail, string Subject)> SentEmails { get; } = new();

    public Task SendWelcomeEmailAsync(string recipientEmail, string recipientName, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((recipientEmail, $"Welcome to Our App, {recipientName}!"));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string resetLink, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((recipientEmail, "Password Reset Request"));
        return Task.CompletedTask;
    }

    public Task SendCustomEmailAsync(string recipientEmail, string recipientName, string subject, string htmlContent, string attachmentPath = null, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((recipientEmail, subject));
        return Task.CompletedTask;
    }

    public Task SendCustomEmailAsync(string recipientEmail, string recipientName, string subject, string htmlContent, QRDataBuilder qrDataBuilder, string qrCodeAltText = "QR Code", int qrCodeSize = 20, string attachmentPath = null, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((recipientEmail, subject));
        return Task.CompletedTask;
    }

    public Task SendPasswordChangeConfirmationEmailAsync(string recipientEmail, string recipientName, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((recipientEmail, "Password Changed Successfully"));
        return Task.CompletedTask;
    }

    public Task SendAccountCreatedEmailAsync(string recipientEmail, string recipientName, string temporaryPassword, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((recipientEmail, "Your Account Has Been Created"));
        return Task.CompletedTask;
    }
}
