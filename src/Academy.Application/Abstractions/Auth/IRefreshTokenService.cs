using Academy.Domain;

namespace Academy.Application.Abstractions.Auth;

public interface IRefreshTokenService
{
    Task<(string RawToken, RefreshToken Entity)> CreateAsync(Guid userId, string? ip, CancellationToken ct);

    string Hash(string rawToken);
}