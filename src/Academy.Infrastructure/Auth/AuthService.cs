using Academy.Application.Abstractions.Auth;
using Academy.Application.Contracts.Auth;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Infrastructure.Identity;
using Academy.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IGoogleIdTokenValidator _googleIdTokenValidator;

    public AuthService(
        AppDbContext dbContext,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IGoogleIdTokenValidator googleIdTokenValidator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _googleIdTokenValidator = googleIdTokenValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, string? ip, CancellationToken ct)
    {
        var existingUser = await _userManager.FindByEmailAsync(req.Email);
        if (existingUser is not null)
        {
            throw CreateClientException("Email already registered.");
        }

        var academy = await ResolveAcademyAsync(req.AcademyId, ct);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = req.Email,
            Email = req.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, req.Password);
        if (!createResult.Succeeded)
        {
            throw CreateClientException(CollectErrors(createResult));
        }

        if (!await _roleManager.RoleExistsAsync(Roles.Parent))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Parent));
            if (!roleResult.Succeeded)
            {
                throw CreateClientException(CollectErrors(roleResult));
            }
        }

        var roleAssign = await _userManager.AddToRoleAsync(user, Roles.Parent);
        if (!roleAssign.Succeeded)
        {
            throw CreateClientException(CollectErrors(roleAssign));
        }

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AcademyId = academy.Id,
            DisplayName = req.DisplayName,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.UserProfiles.Add(profile);

        var (rawToken, refreshEntity) = await _refreshTokenService.CreateAsync(user.Id, ip, ct);
        _dbContext.RefreshTokens.Add(refreshEntity);

        await _dbContext.SaveChangesAsync(ct);

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var accessToken = _jwtTokenService.CreateAccessToken(user.Id, req.Email, academy.Id, roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresInSeconds = _jwtTokenService.GetAccessTokenExpiresInSeconds(),
            User = new UserInfo
            {
                Id = user.Id,
                Email = req.Email,
                DisplayName = profile.DisplayName,
                AcademyId = academy.Id,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, string? ip, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        var valid = await _userManager.CheckPasswordAsync(user, req.Password);
        if (!valid)
        {
            throw new InvalidCredentialsException();
        }

        var profile = await _dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == user.Id, ct);

        if (profile is null)
        {
            throw new KeyNotFoundException("User profile not found.");
        }

        var (rawToken, refreshEntity) = await _refreshTokenService.CreateAsync(user.Id, ip, ct);
        _dbContext.RefreshTokens.Add(refreshEntity);
        await _dbContext.SaveChangesAsync(ct);

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.Email ?? req.Email, profile.AcademyId, roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresInSeconds = _jwtTokenService.GetAccessTokenExpiresInSeconds(),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? req.Email,
                DisplayName = profile.DisplayName,
                AcademyId = profile.AcademyId,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest req, string? ip, CancellationToken ct)
    {
        var tokenHash = _refreshTokenService.Hash(req.RefreshToken);
        var existing = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (existing is null || !existing.IsActive)
        {
            throw new InvalidRefreshTokenException();
        }

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.RevokedByIp = ip;

        var (rawToken, newEntity) = await _refreshTokenService.CreateAsync(existing.UserId, ip, ct);
        existing.ReplacedByTokenHash = newEntity.TokenHash;

        _dbContext.RefreshTokens.Add(newEntity);
        await _dbContext.SaveChangesAsync(ct);

        var user = await _userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var profile = await _dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == existing.UserId, ct);

        if (profile is null)
        {
            throw new KeyNotFoundException("User profile not found.");
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.Email ?? string.Empty, profile.AcademyId, roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresInSeconds = _jwtTokenService.GetAccessTokenExpiresInSeconds(),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = profile.DisplayName,
                AcademyId = profile.AcademyId,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest req, string? ip, CancellationToken ct)
    {
        var payload = await _googleIdTokenValidator.ValidateAsync(req.IdToken, ct);

        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                throw CreateClientException(CollectErrors(createResult));
            }
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw CreateClientException(CollectErrors(updateResult));
            }
        }

        await EnsureGoogleLoginAsync(user, payload.Subject);

        var academy = await ResolveAcademyForGoogleAsync(req.AcademyId, ct);
        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id, ct);

        if (profile is null)
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AcademyId = academy.Id,
                DisplayName = !string.IsNullOrWhiteSpace(payload.Name)
                    ? payload.Name
                    : payload.Email.Split('@')[0],
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.UserProfiles.Add(profile);
        }

        await EnsureDefaultRoleAsync(user);

        var (rawToken, refreshEntity) = await _refreshTokenService.CreateAsync(user.Id, ip, ct);
        _dbContext.RefreshTokens.Add(refreshEntity);

        await _dbContext.SaveChangesAsync(ct);

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.Email ?? payload.Email, profile.AcademyId, roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresInSeconds = _jwtTokenService.GetAccessTokenExpiresInSeconds(),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? payload.Email,
                DisplayName = profile.DisplayName,
                AcademyId = profile.AcademyId,
                Roles = roles
            }
        };
    }

    public async Task LogoutAsync(LogoutRequest req, string? ip, CancellationToken ct)
    {
        var tokenHash = _refreshTokenService.Hash(req.RefreshToken);
        var existing = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (existing is null || !existing.IsActive)
        {
            return;
        }

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.RevokedByIp = ip;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<UserInfo> MeAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var profile = await _dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
        {
            throw new KeyNotFoundException("User profile not found.");
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();

        return new UserInfo
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = profile.DisplayName,
            AcademyId = profile.AcademyId,
            Roles = roles
        };
    }

    private async Task<Academy.Domain.Academy> ResolveAcademyAsync(Guid? academyId, CancellationToken ct)
    {
        Academy.Domain.Academy? academy;

        if (academyId.HasValue)
        {
            academy = await _dbContext.Academies
                .FirstOrDefaultAsync(a => a.Id == academyId.Value, ct);
        }
        else
        {
            academy = await _dbContext.Academies
                .OrderBy(a => a.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
        }

        if (academy is null)
        {
            throw new KeyNotFoundException("Academy not found.");
        }

        return academy;
    }

    private async Task<Academy.Domain.Academy> ResolveAcademyForGoogleAsync(Guid? academyId, CancellationToken ct)
    {
        Academy.Domain.Academy? academy;

        if (academyId.HasValue)
        {
            academy = await _dbContext.Academies
                .FirstOrDefaultAsync(a => a.Id == academyId.Value, ct);
        }
        else
        {
            academy = await _dbContext.Academies
                .OrderBy(a => a.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
        }

        if (academy is null)
        {
            throw new InvalidAcademyException();
        }

        return academy;
    }

    private async Task EnsureGoogleLoginAsync(AppUser user, string subject)
    {
        var logins = await _userManager.GetLoginsAsync(user);
        if (logins.Any(login => login.LoginProvider == "Google" && login.ProviderKey == subject))
        {
            return;
        }

        var addResult = await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", subject, "Google"));
        if (!addResult.Succeeded)
        {
            throw CreateClientException(CollectErrors(addResult));
        }
    }

    private async Task EnsureDefaultRoleAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count > 0)
        {
            return;
        }

        if (!await _roleManager.RoleExistsAsync(Roles.Parent))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Parent));
            if (!roleResult.Succeeded)
            {
                throw CreateClientException(CollectErrors(roleResult));
            }
        }

        var assignResult = await _userManager.AddToRoleAsync(user, Roles.Parent);
        if (!assignResult.Succeeded)
        {
            throw CreateClientException(CollectErrors(assignResult));
        }
    }

    private static InvalidOperationException CreateClientException(string message)
    {
        var exception = new InvalidOperationException(message);
        exception.Data["IsClientError"] = true;
        return exception;
    }

    private static string CollectErrors(IdentityResult result)
        => string.Join(", ", result.Errors.Select(e => e.Description));
}
