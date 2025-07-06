using Katino.Domain.Entities;

namespace Katino.Domain.Services.Email.SendEmail;

public interface ISendEmailService
{
    Task SendAccountConfirmationEmail(AppUser receiver, string url);
}