namespace Academy.Application.Exceptions;

public sealed class TenantScopeException : Exception
{
    public TenantScopeException()
        : base("Missing academy scope")
    {
    }
}