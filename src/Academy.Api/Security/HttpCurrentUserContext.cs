using System.Security.Claims;
using Academy.Application.Abstractions.Security;
using Academy.Api.Extensions;

namespace Academy.Api.Security;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId => User.GetUserId();

    public Guid? AcademyId => User.GetAcademyId();

    public IReadOnlyList<string> Roles => User.GetRoles();

    public string? Email
        => User?.FindFirstValue(ClaimTypes.Email)
           ?? User?.FindFirstValue("email");
}