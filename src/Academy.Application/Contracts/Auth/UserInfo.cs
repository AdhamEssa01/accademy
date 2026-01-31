namespace Academy.Application.Contracts.Auth;

public sealed class UserInfo
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public Guid AcademyId { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}