using Katino.Domain.Auth;
using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Exceptions;
using Katino.Domain.Options;
using Katino.Domain.Repositories.CustomerRepository;
using Katino.Domain.Services.Email.SendEmail;
using Katino.Store.Application.DTOs;
using Katino.Store.Application.Factories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Katino.Store.Application.Services.AuthorizationService;

public class CustomerAuthorizationService : ICustomerAuthorizationService
{
    private const int EmailConfirmationTokenValidityHours = 1;
    private static readonly TimeSpan CustomerTokenLifetime = TimeSpan.FromDays(2);
    // Special character set matches what's documented on CustomerAuthController's Register endpoint - keep both in sync.
    private static readonly Regex PasswordStrengthRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;:'"",.<>/?]).{7,}$",
        RegexOptions.Compiled);

    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher<Customer> _passwordHasher;
    private readonly IAuthTokenFactory _tokenFactory;
    private readonly ISendEmailService _emailService;
    private readonly SecretKeyOptions _secretKeyOptions;
    private readonly ILogger _logger;

    public CustomerAuthorizationService(
        ICustomerRepository customerRepository,
        IPasswordHasher<Customer> passwordHasher,
        IAuthTokenFactory tokenFactory,
        ISendEmailService emailService,
        IOptions<SecretKeyOptions> secretKeyOptions,
        ILoggerFactory loggerFactory)
    {
        _customerRepository = customerRepository;
        _passwordHasher = passwordHasher;
        _tokenFactory = tokenFactory;
        _emailService = emailService;
        _secretKeyOptions = secretKeyOptions.Value;
        _logger = loggerFactory?.CreateLogger(nameof(CustomerAuthorizationService));
    }

    public async Task RegisterAsync(string email, string password, string confirmPassword, string clientUriForEmailConfirmation)
    {
        _logger.LogDebug("Registering customer {email}", email);

        if (password != confirmPassword)
        {
            _logger.LogWarning("Passwords don't match.");
            throw new PasswordsMismatchException();
        }

        if (!PasswordStrengthRegex.IsMatch(password ?? string.Empty))
        {
            _logger.LogWarning("Password is too weak.");
            throw new WeakPasswordException();
        }

        string normalizedEmail = NormalizeEmail(email);

        Customer existingCustomer = await _customerRepository.GetByEmailAsync(normalizedEmail);
        if (existingCustomer != null)
        {
            _logger.LogWarning("Email {email} has already been taken", normalizedEmail);
            throw new EmailAlreadyTakenException();
        }

        Customer customer = new()
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(null, password),
            IsEmailConfirmed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        SetNewConfirmationToken(customer);

        await _customerRepository.Insert(customer);
        await _customerRepository.Save();

        string url = QueryHelpers.AddQueryString(clientUriForEmailConfirmation, new Dictionary<string, string>
        {
            { "token", customer.EmailConfirmationToken },
            { "email", customer.Email }
        });
        await _emailService.SendAccountConfirmationEmail(customer, url);
    }

    public async Task ConfirmEmailAsync(string email, string token)
    {
        _logger.LogDebug("Confirming customer email {email}", email);

        Customer customer = await _customerRepository.GetByEmailAsync(NormalizeEmail(email));
        bool tokenIsValid = customer != null
            && !customer.IsEmailConfirmed
            && !string.IsNullOrEmpty(customer.EmailConfirmationToken)
            && customer.EmailConfirmationToken == token
            && customer.EmailConfirmationTokenExpiresAt >= DateTimeOffset.UtcNow;

        if (!tokenIsValid)
        {
            _logger.LogWarning("Email confirmation token is invalid or expired for {email}", email);
            throw new EmailConfirmationTokenInvalidException();
        }

        customer.IsEmailConfirmed = true;
        customer.EmailConfirmationToken = null;
        customer.EmailConfirmationTokenExpiresAt = null;

        await _customerRepository.Update(customer);
        await _customerRepository.Save();
    }

    public async Task<CustomerAuthResultDto> SignInAsync(string email, string password)
    {
        _logger.LogDebug("Signing in customer {email}", email);

        Customer customer = await _customerRepository.GetByEmailAsync(NormalizeEmail(email));
        LoginErrorCode status = Verify(customer, password);

        if (status != LoginErrorCode.None)
        {
            return new CustomerAuthResultDto()
            {
                Token = null,
                IsAuthorized = false,
                LoginErrorCode = status
            };
        }

        IEnumerable<Claim> claims =
        [
            new(AuthorizationConstants.ID, customer.Id.ToString()),
            new(ClaimTypes.Email, customer.Email),
            new(ClaimTypes.Role, Role.Customer)
        ];

        AuthOptions authOptions = new(_secretKeyOptions);
        JwtSecurityToken token = _tokenFactory.CreateToken(
            customer.Email,
            authOptions.GetSymmetricSecurityKey(),
            AuthOptions.ISSUER,
            AuthOptions.CUSTOMER_AUDIENCE,
            claims,
            CustomerTokenLifetime);

        return new CustomerAuthResultDto()
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            IsAuthorized = true,
            LoginErrorCode = LoginErrorCode.None,
            CustomerInfo = new CustomerAuthInfoDto()
            {
                CustomerId = customer.Id,
                Email = customer.Email,
                RegistryDate = customer.CreatedAt
            }
        };
    }

    private LoginErrorCode Verify(Customer customer, string password)
    {
        if (customer == null)
        {
            return LoginErrorCode.InvalidUsernameOrPassword;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return LoginErrorCode.InvalidUsernameOrPassword;
        }

        return customer.IsEmailConfirmed
            ? LoginErrorCode.None
            : LoginErrorCode.EmailConfirmationRequired;
    }

    private static void SetNewConfirmationToken(Customer customer)
    {
        customer.EmailConfirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        customer.EmailConfirmationTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(EmailConfirmationTokenValidityHours);
    }

    private static string NormalizeEmail(string email) => email?.Trim().ToLowerInvariant();
}
