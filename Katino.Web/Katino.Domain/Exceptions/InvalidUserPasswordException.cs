namespace Katino.Domain.Exceptions;

public class InvalidUserPasswordException : Exception
{
    private const string MESSAGE = "Invalid user password.";

    public InvalidUserPasswordException()
        : base(MESSAGE) { }

    public InvalidUserPasswordException(string message)
        : base(message) { }
}

