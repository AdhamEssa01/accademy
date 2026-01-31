using Academy.Application.Abstractions.Security;
using Academy.Application.Exceptions;

namespace Academy.Application.Services;

public sealed class TenantGuard : ITenantGuard
{
    private readonly ICurrentUserContext _currentUserContext;

    public TenantGuard(ICurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
    }

    public void EnsureAcademyScopeOrThrow()
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.AcademyId.HasValue)
        {
            throw new TenantScopeException();
        }
    }

    public Guid GetAcademyIdOrThrow()
    {
        EnsureAcademyScopeOrThrow();
        return _currentUserContext.AcademyId!.Value;
    }
}