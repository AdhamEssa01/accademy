using System.Security.Cryptography;
using System.Text;
using Academy.Application.Abstractions.Auth;
using Academy.Application.Options;
using Academy.Domain;
using Microsoft.Extensions.Options;

namespace Academy.Infrastructure.Auth;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly JwtOptions _options;

    public RefreshTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Task<(string RawToken, RefreshToken Entity)> CreateAsync(Guid userId, string? ip, CancellationToken ct)
    {
        var rawToken = GenerateToken();
        var tokenHash = Hash(rawToken);

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshTokenDays),
            CreatedByIp = ip
        };

        return Task.FromResult((rawToken, entity));
    }

    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GenerateToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}