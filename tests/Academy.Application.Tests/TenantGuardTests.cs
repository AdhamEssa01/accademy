using Academy.Application.Abstractions.Security;
using Academy.Application.Exceptions;
using Academy.Application.Services;
using Xunit;

namespace Academy.Application.Tests;

public sealed class TenantGuardTests
{
    [Fact]
    public void EnsureAcademyScopeOrThrow_Throws_When_NotAuthenticated()
    {
        var guard = new TenantGuard(new TestCurrentUserContext
        {
            IsAuthenticated = false,
            AcademyId = null
        });

        Assert.Throws<TenantScopeException>(() => guard.EnsureAcademyScopeOrThrow());
    }

    [Fact]
    public void EnsureAcademyScopeOrThrow_Throws_When_AcademyMissing()
    {
        var guard = new TenantGuard(new TestCurrentUserContext
        {
            IsAuthenticated = true,
            AcademyId = null
        });

        Assert.Throws<TenantScopeException>(() => guard.EnsureAcademyScopeOrThrow());
    }

    [Fact]
    public void GetAcademyIdOrThrow_Returns_AcademyId()
    {
        var academyId = Guid.NewGuid();
        var guard = new TenantGuard(new TestCurrentUserContext
        {
            IsAuthenticated = true,
            AcademyId = academyId
        });

        Assert.Equal(academyId, guard.GetAcademyIdOrThrow());
    }

    private sealed class TestCurrentUserContext : ICurrentUserContext
    {
        public bool IsAuthenticated { get; init; }
        public Guid? UserId { get; init; }
        public Guid? AcademyId { get; init; }
        public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
        public string? Email { get; init; }
    }
}
