namespace Academy.Application.Contracts.Auth;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public long ExpiresInSeconds { get; set; }

    public UserInfo User { get; set; } = new();
}