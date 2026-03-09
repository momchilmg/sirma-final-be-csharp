using FootballPairs.Api.Contracts.Requests.Auth;
using FootballPairs.Api.Contracts.Responses.Auth;
using FootballPairs.Application.Auth;
using FootballPairs.Application.Auth.Models;
using FootballPairs.Application.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FootballPairs.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public sealed class AuthController(IAuthService authService, ITokenRevocationService tokenRevocationService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Username, request.Password);
        var user = await authService.RegisterAsync(command, cancellationToken);
        var response = new RegisterResponse(user.Id, user.Username, user.Role, user.CreatedAt);
        return Created(string.Empty, response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Username, request.Password);
        var result = await authService.LoginAsync(command, cancellationToken);
        return Ok(new LoginResponse(result.AccessToken));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(jti))
        {
            throw new UnauthenticatedException("Authenticated token is missing jti claim.");
        }

        var expiresAtUtc = ResolveTokenExpiry(User);
        await tokenRevocationService.RevokeAsync(jti, expiresAtUtc, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<MeResponse> Me()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var configuredRoleClaimType = claimsIdentity?.RoleClaimType;
        var configuredNameClaimType = claimsIdentity?.NameClaimType;
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(configuredRoleClaimType ?? ClaimTypes.Role)
            ?? User.FindFirstValue(ClaimTypes.Role)
            ?? User.FindFirstValue("role");
        var username = User.FindFirstValue(configuredNameClaimType ?? JwtRegisteredClaimNames.UniqueName)
            ?? User.Identity?.Name
            ?? User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
            ?? User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
        {
            throw new UnauthenticatedException("Authenticated token is missing required identity claims.");
        }

        return Ok(new MeResponse(userId, role, username));
    }

    [HttpGet("admin-check")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<object> AdminCheck()
    {
        return Ok(new { allowed = true });
    }

    private static DateTime ResolveTokenExpiry(ClaimsPrincipal user)
    {
        var expClaim = user.FindFirstValue(JwtRegisteredClaimNames.Exp) ?? user.FindFirstValue("exp");
        if (long.TryParse(expClaim, out var expSeconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
        }

        return DateTime.UtcNow;
    }
}
