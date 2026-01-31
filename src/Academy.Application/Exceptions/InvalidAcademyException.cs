namespace Academy.Application.Exceptions;

public sealed class InvalidAcademyException : Exception
{
    public InvalidAcademyException()
        : base("Invalid academy")
    {
    }
}