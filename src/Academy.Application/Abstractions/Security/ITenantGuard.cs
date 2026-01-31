namespace Academy.Application.Abstractions.Security;

public interface ITenantGuard
{
    void EnsureAcademyScopeOrThrow();

    Guid GetAcademyIdOrThrow();
}