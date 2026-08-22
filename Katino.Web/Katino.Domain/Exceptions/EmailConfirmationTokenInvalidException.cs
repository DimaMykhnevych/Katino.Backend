namespace Katino.Domain.Exceptions;

public class EmailConfirmationTokenInvalidException : Exception
{
    private const string MESSAGE = "Email confirmation token is invalid or has expired.";

    public EmailConfirmationTokenInvalidException()
        : base(MESSAGE) { }

    public EmailConfirmationTokenInvalidException(string message)
        : base(message) { }
}
