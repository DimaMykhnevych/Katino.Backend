namespace Katino.Domain.Exceptions;

public class WeakPasswordException : Exception
{
    private const string MESSAGE = "Password is too weak.";

    public WeakPasswordException()
        : base(MESSAGE) { }

    public WeakPasswordException(string message)
        : base(message) { }
}
