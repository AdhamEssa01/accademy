using System.Security.Claims;
using Academy.Application.Abstractions.Auth;
using Academy.Application.Contracts.Auth;
using Academy.Application.Exceptions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var response = await _authService.RegisterAsync(request, GetIp(), ct);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.LoginAsync(request, GetIp(), ct);
            return Ok(response);
        }
        catch (InvalidCredentialsException)
        {
            return Problem(title: "Invalid credentials", statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.RefreshAsync(request, GetIp(), ct);
            return Ok(response);
        }
        catch (InvalidRefreshTokenException)
        {
            return Problem(title: "Invalid refresh token", statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Google([FromBody] GoogleLoginRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.GoogleLoginAsync(request, GetIp(), ct);
            return Ok(response);
        }
        catch (InvalidGoogleTokenException)
        {
            return Problem(title: "Invalid Google token", statusCode: StatusCodes.Status400BadRequest);
        }
        catch (UnverifiedGoogleEmailException)
        {
            return Problem(title: "Unverified Google email", statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidAcademyException)
        {
            return Problem(title: "Invalid academy", statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request, GetIp(), ct);
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> Me(CancellationToken ct)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var response = await _authService.MeAsync(userId, ct);
        return Ok(response);
    }

    private string? GetIp()
        => HttpContext.Connection.RemoteIpAddress?.ToString();
}
