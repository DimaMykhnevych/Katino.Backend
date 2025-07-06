using Katino.Domain.Entities;
using Katino.Domain.Exceptions;
using Katino.Domain.Extensions;
using Katino.Domain.Services.Email.SendEmail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Katino.Domain.Services.User.CreateUser;

public class CreateUserService : ICreateUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ISendEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public CreateUserService(
        UserManager<AppUser> userManager,
        ISendEmailService emailService,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = loggerFactory?.CreateLogger(nameof(CreateUserService));
    }

    public async Task<AppUser> CreateUserAsync(AppUser user, string password, string confirmPassword, string clientURIForEmailConfirmation)
    {
        _logger.LogDebug("Creating user {userName}", user.UserName);

        if (password != confirmPassword)
        {
            _logger.LogWarning("Passwords don't match.");
            throw new PasswordsMismatchException();
        }

        AppUser existingUser = await _userManager.FindByNameAsync(user.UserName);
        if (existingUser != null)
        {
            _logger.LogWarning("Username {userName} has already been taken", user.UserName);
            throw new UsernameAlreadyTakenException();
        }

        user.RegistryDate = DateTime.Now;
        IdentityResult addUserResult = await _userManager.CreateAsync(user, password);

        addUserResult.ValidateIdentityResult();

        if (_configuration.EmailConfirmationEnabled())
        {
            _logger.LogDebug("Email confirmation enabled. Sending confirmation email to {userName}", user.UserName);

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var param = new Dictionary<string, string>
                {
                    {"token", token },
                    {"email", user.Email }
                };
            string url = QueryHelpers.AddQueryString(clientURIForEmailConfirmation, param);
            await _emailService.SendAccountConfirmationEmail(user, url);
        }

        return await _userManager.FindByNameAsync(user.UserName);
    }
}
