using Academy.Application.Abstractions.Security;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Infrastructure.Identity;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Academy.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tenant-debug")]
public sealed class TenantDebugController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly bool _debugEndpointsEnabled;

    public TenantDebugController(
        AppDbContext dbContext,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ICurrentUserContext currentUserContext,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _currentUserContext = currentUserContext;
        _debugEndpointsEnabled = environment.IsDevelopment()
            || environment.IsEnvironment("Testing")
            || configuration.GetValue("DebugEndpoints:Enabled", false);
    }

    [HttpGet("me")]
    [Authorize(Policy = Policies.AnyAuthenticated)]
    public IActionResult Me()
        => Ok(new
        {
            _currentUserContext.UserId,
            _currentUserContext.AcademyId,
            _currentUserContext.Email,
            Roles = _currentUserContext.Roles
        });

    [HttpPost("seed-second-academy")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> SeedSecondAcademy(CancellationToken ct)
    {
        if (!_debugEndpointsEnabled)
        {
            return NotFound();
        }

        var academy = await _dbContext.Academies
            .FirstOrDefaultAsync(a => a.Name == "Second Academy", ct);

        if (academy is null)
        {
            academy = new Academy.Domain.Academy
            {
                Id = Guid.NewGuid(),
                Name = "Second Academy",
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Academies.Add(academy);
            await _dbContext.SaveChangesAsync(ct);
        }

        const string otherAdminEmail = "otheradmin@local.test";

        var user = await _userManager.FindByEmailAsync(otherAdminEmail);
        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = otherAdminEmail,
                Email = otherAdminEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, "Admin123$");
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Problem(title: "Seed failed", detail: errors, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        if (!await _roleManager.RoleExistsAsync(Roles.Admin))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Admin));
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Problem(title: "Seed failed", detail: errors, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Admin);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Problem(title: "Seed failed", detail: errors, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id, ct);

        if (profile is null)
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AcademyId = academy.Id,
                DisplayName = "Other Admin",
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.UserProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(ct);
        }

        return Ok(new { academyId = academy.Id, otherAdminUserId = user.Id });
    }

    [HttpGet("user-profiles/{userId:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> GetUserProfile(Guid userId, CancellationToken ct)
    {
        var profile = await _dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            profile.DisplayName,
            profile.AcademyId
        });
    }

    private bool IsDevOrTesting()
        => _debugEndpointsEnabled;
}
