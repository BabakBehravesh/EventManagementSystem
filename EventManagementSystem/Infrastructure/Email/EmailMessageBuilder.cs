using MimeKit;

namespace EventManagementSystem.Infrastructure.Email;

public class EmailMessageBuilder
{
    private readonly MimeMessage _message;
    private readonly BodyBuilder _bodyBuilder;
    private readonly IWebHostEnvironment _environment;
    private string _imageHtmlCode = string.Empty;

    public EmailMessageBuilder(IWebHostEnvironment environment)
    {
        _message = new MimeMessage();
        _bodyBuilder = new BodyBuilder();
        _environment = environment;
    }

    public EmailMessageBuilder SetFrom(string name, string email)
    {
        _message.From.Add(new MailboxAddress(name, email));
        return this;
    }

    public EmailMessageBuilder SetFrom(string email)
    {
        _message.From.Add(new MailboxAddress("", email));
        return this;
    }

    public EmailMessageBuilder AddTo(string name, string email)
    {
        _message.To.Add(new MailboxAddress(name, email));
        return this;
    }

    public EmailMessageBuilder AddTo(string email)
    {
        _message.To.Add(new MailboxAddress("", email));
        return this;
    }

    public EmailMessageBuilder AddCc(string email)
    {
        _message.Cc.Add(new MailboxAddress("", email));
        return this;
    }

    public EmailMessageBuilder SetSubject(string subject)
    {
        _message.Subject = subject;
        return this;
    }

    public EmailMessageBuilder AddImageLogo(string imagePath = null)
    {
        imagePath ??= Path.Combine(_environment.ContentRootPath, "resources/images", "logo.jpg");

        if (File.Exists(imagePath))
        {
            var image = _bodyBuilder.LinkedResources.Add(imagePath);
            image.ContentId = "logo";
            _imageHtmlCode = "<img src='cid:logo' alt='Company Logo' style='max-width: 200px;'>";
        }
        return this;
    }

    public EmailMessageBuilder SetHtmlBody(string htmlContent)
    {
        _bodyBuilder.HtmlBody = htmlContent;
        return this;
    }

    public EmailMessageBuilder SetWelcomeTemplate(string recipientName)
    {
        var htmlBody = $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto;'>
        {_imageHtmlCode}
        <h1 style='color: #3366cc;'>Welcome {recipientName}!</h1>
        <p>Thank you for joining <strong>Our App</strong>! We're excited to have you on board.</p>
        <div style='margin-top: 20px; padding: 15px; background: #f8f9fa; border-radius: 5px;'>
            <h3>Next Steps:</h3>
            <ul>
                <li>Complete your profile</li>
                <li>Explore our features</li>
                <li>Join the community</li>
            </ul>
        </div>
        <p style='margin-top: 20px;'>Best regards,<br/><strong>The Our App Team</strong></p>
    </div>
</body>
</html>";

        _bodyBuilder.HtmlBody = htmlBody;
        _bodyBuilder.TextBody = StripHtml(htmlBody);
        return this;
    }

    public EmailMessageBuilder SetAccountCreatedTemplate(string recipientName, string temporaryPassword, string frontendBaseUrl)
    {

        var htmlBody = $@"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        {_imageHtmlCode}
        <h1 style='color: #3366cc;'>Your Account Has Been Created</h1>
        <p>Hello {recipientName},</p>
        <p>An administrator has created an account for you in our system.</p>
        
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
            <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
        </div>
        
        <p>Please login and change your password immediately.</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{frontendBaseUrl}/auth/login' 
               style='background: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px;'>
               Login to Your Account
            </a>
        </div>
        
        <div style='background: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
            <p><strong>Security Notice:</strong> For security reasons, please change your password after first login.</p>
        </div>
    </div>
</body>
</html>";

        _bodyBuilder.HtmlBody = htmlBody;
        _bodyBuilder.TextBody = StripHtml(htmlBody);
        return this;
    }

    public EmailMessageBuilder SetPasswordResetTemplate(string recipientName, string resetLink)
    {
        var htmlBody = $@"<html>
<body style='font-family: Arial, sans-serif;'>
    {_imageHtmlCode}
    <h1 style='color: #3366cc;'>Password Reset</h1>
    <p>Hello {recipientName},</p>
    <p>You requested to reset your password. Click the link below:</p>
    <p><a href='{resetLink}' style='background: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
    <p>If you didn't request this, please ignore this email.</p>
</body>
</html>";

        _bodyBuilder.HtmlBody = htmlBody;
        _bodyBuilder.TextBody = StripHtml(htmlBody);
        return this;
    }

    public EmailMessageBuilder SetPasswordChangeConfirmationTemplate(string recipientName)
    {
        var currentYear = DateTime.Now.Year;
        var timestamp = DateTime.Now.ToString("MMMM dd, yyyy 'at' hh:mm tt UTC");

        var htmlBody = $@"<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background: #ffffff;
            border-radius: 10px;
            padding: 30px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        .success-icon {{
            text-align: center;
            color: #28a745;
            font-size: 48px;
            margin-bottom: 20px;
        }}
        .greeting {{
            font-size: 18px;
            margin-bottom: 20px;
            color: #555;
        }}
        .message {{
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            border-left: 4px solid #28a745;
            margin: 20px 0;
        }}
        .security-tip {{
            background: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 8px;
            padding: 15px;
            margin: 20px 0;
            font-size: 14px;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
            color: #666;
            font-size: 12px;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='container'>
        {_imageHtmlCode}
        <div class='success-icon'>✓</div>
        
        <div class='greeting'>
            <strong>Hello {recipientName},</strong>
        </div>
        
        <p>Your password has been changed successfully. This is a confirmation that your account password was updated.</p>
        
        <div class='message'>
            <strong>Change Details:</strong>
            <ul>
                <li>Password updated on: {timestamp}</li>
                <li>Account: {recipientName}</li>
            </ul>
        </div>
        
        <div class='security-tip'>
            <strong>🔒 Security Tip:</strong> If you did not make this change, please contact our support team immediately.
        </div>
        
        <div class='footer'>
            <p>&copy; {currentYear} Our App. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        _bodyBuilder.HtmlBody = htmlBody;
        _bodyBuilder.TextBody = StripHtml(htmlBody);
        return this;
    }



    public EmailMessageBuilder SetTextBody(string textContent)
    {
        _bodyBuilder.TextBody = textContent;
        return this;
    }

    public EmailMessageBuilder AddAttachment(string filePath)
    {
        if (File.Exists(filePath))
        {
            _bodyBuilder.Attachments.Add(filePath);
        }
        return this;
    }
    public EmailMessageBuilder AddAttachment(byte[] fileData, string fileName)
    {
        _bodyBuilder.Attachments.Add(fileName, fileData);
        return this;
    }

    public MimeMessage Build()
    {
        _message.Body = _bodyBuilder.ToMessageBody();
        return _message;
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }
}
