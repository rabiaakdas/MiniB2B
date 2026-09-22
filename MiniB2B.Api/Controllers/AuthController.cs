using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Api.Extensions;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Auth;
using MiniB2B.Application.Interfaces;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        IAuthService authService,
        UserManager<ApplicationUser> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        return ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return ToActionResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Unauthorized(new { errors = new[] { "Kullanıcı hesabı bulunamadı." } });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? Roles.User;

        return Ok(new
        {
            UserId = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role
        });
    }

    private IActionResult ToActionResult(AuthResult result)
    {
        if (result.Succeeded)
        {
            return result.StatusCode == StatusCodes.Status201Created
                ? StatusCode(StatusCodes.Status201Created, result.Data)
                : Ok(result.Data);
        }

        var errorResponse = new { errors = result.Errors };

        return result.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => Unauthorized(errorResponse),
            StatusCodes.Status403Forbidden => StatusCode(StatusCodes.Status403Forbidden, errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
