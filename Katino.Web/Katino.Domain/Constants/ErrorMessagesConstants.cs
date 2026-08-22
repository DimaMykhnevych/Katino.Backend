namespace Katino.Domain.Constants;

public static class ErrorMessagesConstants
{
    public const string NOT_ALL_PASS_FIELDS_FILLED = "Not all password fields are filled.";
    public const string PASSWORDS_DO_NOT_MATCH = "New password and confirm password must be equal.";

    public const string USERNAME_ALREADY_TAKEN = "usernameAlreadyTaken";
    public const string INVALID_PASSWORD = "invalidPassword";
    public const string EMAIL_ALREADY_TAKEN = "emailAlreadyTaken";
    public const string EMAIL_CONFIRMATION_TOKEN_INVALID = "emailConfirmationTokenInvalid";
    public const string PASSWORD_MISMATCH = "passwordMismatch";
    public const string PASSWORD_TOO_WEAK = "passwordTooWeak";
}
