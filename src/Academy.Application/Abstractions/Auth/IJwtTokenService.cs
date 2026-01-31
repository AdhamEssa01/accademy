namespace Academy.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(Guid userId, string email, Guid academyId, IReadOnlyList<string> roles);

    long GetAccessTokenExpiresInSeconds();
}