namespace Katino.Domain.Exceptions;

public class EmailAlreadyTakenException : Exception
{
    private const string MESSAGE = "Email was already taken.";

    public EmailAlreadyTakenException()
        : base(MESSAGE) { }

    public EmailAlreadyTakenException(string message)
        : base(message) { }
}
