using CollectA.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Succeeded) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Refresh([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        if (!result.Succeeded) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await _authService.RevokeRefreshTokenAsync(refreshToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfile>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profile = await _authService.GetCurrentUserAsync(Guid.Parse(userId));
        if (profile == null) return NotFound();
        return Ok(profile);
    }
}
