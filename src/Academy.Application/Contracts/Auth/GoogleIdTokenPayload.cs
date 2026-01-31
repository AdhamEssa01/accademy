namespace Academy.Application.Contracts.Auth;

public sealed class GoogleIdTokenPayload
{
    public string Subject { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool EmailVerified { get; set; }

    public string? Name { get; set; }

    public string? PictureUrl { get; set; }
}