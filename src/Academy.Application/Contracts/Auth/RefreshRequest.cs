namespace Academy.Application.Contracts.Auth;

public sealed class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}