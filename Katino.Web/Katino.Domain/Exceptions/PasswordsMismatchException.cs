namespace Katino.Domain.Exceptions;

public class PasswordsMismatchException : Exception
{
    private const string MESSAGE = "Passwords don't match.";

    public PasswordsMismatchException()
        : base(MESSAGE) { }

    public PasswordsMismatchException(string message)
        : base(message) { }
}
