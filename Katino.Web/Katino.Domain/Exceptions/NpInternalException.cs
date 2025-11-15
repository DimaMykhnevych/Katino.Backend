namespace Katino.Domain.Exceptions;

public class NpInternalException : Exception
{
    private const string MESSAGE = "An error occurred while communicating with NP API.";

    public NpInternalException()
        : base(MESSAGE) { }

    public NpInternalException(string message)
        : base(message) { }
}
