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
        // TODO more user-friendly design
        message.Subject = "Підтвердження облікового запису";
        message.IsBodyHtml = true;
        string htmlString = @$"<html>
                      <body style='background-color: #f7f1d5; 
                        padding: 15px; border-radius: 15px; 
                        box-shadow: 5px 5px 15px 5px #9F9F9F;
                        font-size: 16px;'>
                      <p>Доброго дня {user.UserName},</p>
                      <p>Будь ласка, підтвердіть свій обліковий запис, перейшовши за наступним посиланням.</p>
                      <a href={url}>Підтвердити обліковий запис</a>
                         <p>Дякуємо,<br>-Katino</br></p>
                      </body>
                      </html>
                     ";
        message.Body = htmlString;

        SmtpClient smtp = new("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(_emailServiceDetails.EmailAddress, _emailServiceDetails.Password),
            EnableSsl = true
        };
        await smtp.SendMailAsync(message);
    }
}
