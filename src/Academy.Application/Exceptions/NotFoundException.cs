namespace Academy.Application.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException()
        : base("Not found")
    {
    }
}