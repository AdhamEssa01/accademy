namespace Academy.Application.Exceptions;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("Forbidden")
    {
    }
}