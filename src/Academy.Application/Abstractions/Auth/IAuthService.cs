using Academy.Application.Contracts.Auth;

namespace Academy.Application.Abstractions.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest req, string? ip, CancellationToken ct);

    Task<AuthResponse> LoginAsync(LoginRequest req, string? ip, CancellationToken ct);

    Task<AuthResponse> RefreshAsync(RefreshRequest req, string? ip, CancellationToken ct);

    Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest req, string? ip, CancellationToken ct);

    Task LogoutAsync(LogoutRequest req, string? ip, CancellationToken ct);

    Task<UserInfo> MeAsync(Guid userId, CancellationToken ct);
}
