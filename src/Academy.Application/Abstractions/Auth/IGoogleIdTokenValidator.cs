using Academy.Application.Contracts.Auth;

namespace Academy.Application.Abstractions.Auth;

public interface IGoogleIdTokenValidator
{
    Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken ct);
}