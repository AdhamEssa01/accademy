using Academy.Application.Abstractions.Auth;
using Academy.Application.Contracts.Auth;
using Academy.Application.Exceptions;
using Academy.Application.Options;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Academy.Infrastructure.Auth;

public sealed class GoogleIdTokenValidator : IGoogleIdTokenValidator
{
    private readonly GoogleAuthOptions _options;

    public GoogleIdTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            throw new InvalidOperationException("GoogleAuth:ClientId is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId }
            });
        }
        catch (Exception)
        {
            throw new InvalidGoogleTokenException();
        }

        if (string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Subject))
        {
            throw new InvalidGoogleTokenException();
        }

        if (!payload.EmailVerified)
        {
            throw new UnverifiedGoogleEmailException();
        }

        return new GoogleIdTokenPayload
        {
            Subject = payload.Subject ?? string.Empty,
            Email = payload.Email,
            EmailVerified = payload.EmailVerified,
            Name = payload.Name,
            PictureUrl = payload.Picture
        };
    }
}
