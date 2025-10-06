using EventManagementSystem.Infrastructure.Email;
using MimeKit;

namespace EventManagementSystem.Domain.Interfaces
{
    public interface IEmailMessageBuilder
    {
        EmailMessageBuilder AddAttachment(byte[] fileData, string fileName);
        EmailMessageBuilder AddAttachment(string filePath);
        EmailMessageBuilder AddCc(string email);
        EmailMessageBuilder AddImageLogo(string imagePath = null);
        EmailMessageBuilder AddTo(string name, string email);
        MimeMessage Build();
        EmailMessageBuilder SetAccountCreatedTemplate(string recipientName, string temporaryPassword, string frontendBaseUrl);
        EmailMessageBuilder SetFrom(string email);
        EmailMessageBuilder SetFrom(string name, string email);
        EmailMessageBuilder SetHtmlBody(string htmlContent);
        EmailMessageBuilder SetPasswordChangeConfirmationTemplate(string recipientName);
        EmailMessageBuilder SetPasswordResetTemplate(string recipientName, string resetLink);
        EmailMessageBuilder SetSubject(string subject);
        EmailMessageBuilder SetTextBody(string textContent);
        EmailMessageBuilder SetWelcomeTemplate(string recipientName);

        EmailMessageBuilder AddQRCode(
                                byte[] qrCodeBytes,
                                string? altText,
                                string? sectionTitle,
                                string? footerNote);
    }
}