namespace Katino.Domain.Exceptions;

public class UsernameAlreadyTakenException : Exception
{
    private const string MESSAGE = "Username was already taken.";

    public UsernameAlreadyTakenException()
        : base(MESSAGE) { }

    public UsernameAlreadyTakenException(string message)
        : base(message) { }
}
