namespace Katino.Domain.Exceptions;

public class UserNotFoundException : Exception
{
    private const string MESSAGE = "User was not found.";

    public UserNotFoundException()
        : base(MESSAGE) { }

    public UserNotFoundException(string message)
        : base(message) { }
}
