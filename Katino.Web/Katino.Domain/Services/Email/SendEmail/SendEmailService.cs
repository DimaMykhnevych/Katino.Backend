using Katino.Domain.Entities;
using System.Net.Mail;
using System.Net;
using Katino.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Katino.Domain.Services.Email.SendEmail;

public class SendEmailService : ISendEmailService
{
    private readonly EmailServiceOptions _emailServiceDetails;
    private readonly ILogger _logger;

    public SendEmailService(IOptions<EmailServiceOptions> options, ILoggerFactory loggerFactory)
    {
        _emailServiceDetails = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(SendEmailService));
    }

    public async Task SendAccountConfirmationEmail(AppUser user, string url)
    {
        _logger.LogDebug("Sending account confirmation email to {userName}", user.UserName);

        MailAddress addressFrom = new(_emailServiceDetails.EmailAddress, "Katino");
        MailAddress addressTo = new(user.Email);
        MailMessage message = new(addressFrom, addressTo);

        // TODO localization
        message.Subject = "Підтвердження облікового запису - Katino";
        message.IsBodyHtml = true;

        string htmlString = GetModernEmailTemplate(user.UserName, url);
        message.Body = htmlString;

        SmtpClient smtp = new("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(_emailServiceDetails.EmailAddress, _emailServiceDetails.Password),
            EnableSsl = true
        };
        await smtp.SendMailAsync(message);
    }

    private string GetModernEmailTemplate(string userName, string confirmationUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""uk"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Підтвердження облікового запису</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            background-color: #f8fafc;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
            overflow: hidden;
        }}
        
        .email-header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 30px;
            text-align: center;
            color: white;
        }}
        
        .logo {{
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
            letter-spacing: 1px;
        }}
        
        .header-subtitle {{
            font-size: 16px;
            opacity: 0.9;
        }}
        
        .email-body {{
            padding: 40px 30px;
        }}
        
        .greeting {{
            font-size: 20px;
            font-weight: 600;
            color: #2d3748;
            margin-bottom: 20px;
        }}
        
        .message {{
            font-size: 16px;
            color: #4a5568;
            margin-bottom: 30px;
            line-height: 1.7;
        }}
        
        .cta-button {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white !important;
            padding: 16px 32px;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            margin: 20px 0;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
        }}
        
        .cta-button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
        }}
        
        .security-note {{
            background-color: #f7fafc;
            border-left: 4px solid #4299e1;
            padding: 16px;
            margin: 30px 0;
            border-radius: 0 8px 8px 0;
        }}
        
        .security-note p {{
            margin: 0;
            font-size: 14px;
            color: #2d3748;
        }}
        
        .footer {{
            background-color: #f8fafc;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e2e8f0;
        }}
        
        .footer p {{
            margin: 0;
            font-size: 14px;
            color: #718096;
        }}
        
        .footer .company-name {{
            color: #4a5568;
            font-weight: 600;
            margin-top: 10px;
        }}
        
        /* Responsive design */
        @media only screen and (max-width: 600px) {{
            .email-container {{
                margin: 10px;
                border-radius: 8px;
            }}
            
            .email-header,
            .email-body {{
                padding: 30px 20px;
            }}
            
            .cta-button {{
                padding: 14px 28px;
                font-size: 15px;
            }}
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            <div class=""logo"">Katino</div>
            <div class=""header-subtitle"">Підтвердження облікового запису</div>
        </div>
        
        <div class=""email-body"">
            <div class=""greeting"">
                Доброго дня, {userName}!
            </div>
            
            <div class=""message"">
                Дякуємо за реєстрацію в Katino! Для завершення процесу реєстрації та активації вашого облікового запису, 
                будь ласка, підтвердіть свою електронну адресу, натиснувши на кнопку нижче.
            </div>
            
            <div style=""text-align: center;"">
                <a href=""{confirmationUrl}"" class=""cta-button"">
                    Підтвердити обліковий запис
                </a>
            </div>
            
            <div class=""security-note"">
                <p><strong>Примітка безпеки:</strong> Якщо ви не створювали обліковий запис на Katino, 
                будь ласка, проігноруйте цей лист. Ваш обліковий запис не буде активований без підтвердження.</p>
            </div>
            
            <div class=""message"">
                Якщо у вас виникли питання або потрібна допомога, не соромтеся звертатися до нашої служби підтримки.
            </div>
        </div>
        
        <div class=""footer"">
            <p>З повагою,</p>
            <p class=""company-name"">Команда Katino</p>
            <p style=""margin-top: 20px; font-size: 12px;"">
                © {DateTime.UtcNow.Year} Katino. Всі права захищені.
            </p>
        </div>
    </div>
</body>
</html>";
    }
}