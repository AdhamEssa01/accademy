namespace Academy.Application.Exceptions;

public sealed class InvalidGoogleTokenException : Exception
{
    public InvalidGoogleTokenException()
        : base("Invalid Google token")
    {
    }
}