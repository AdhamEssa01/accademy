using System.Security.Claims;

namespace Academy.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return null;
        }

        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static Guid? GetAcademyId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return null;
        }

        var value = principal.FindFirstValue("academyId");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return Array.Empty<string>();
        }

        return principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }
}