namespace Katino.Domain.Exceptions;

public class IdentityResultException : Exception
{
    private const string MESSAGE = "Identity result exception occurred.";

    public IdentityResultException()
        : base(MESSAGE) { }

    public IdentityResultException(string message)
        : base(message) { }
}
