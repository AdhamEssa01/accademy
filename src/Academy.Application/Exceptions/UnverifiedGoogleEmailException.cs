namespace Academy.Application.Exceptions;

public sealed class UnverifiedGoogleEmailException : Exception
{
    public UnverifiedGoogleEmailException()
        : base("Unverified Google email")
    {
    }
}