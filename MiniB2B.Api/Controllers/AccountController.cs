using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Api.Extensions;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Users;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IUserAccountService _userAccountService;

    public AccountController(IUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrent()
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _userAccountService.GetCurrentAsync(userId);

        return ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCurrent(UpdateUserAccountRequest request)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _userAccountService.UpdateCurrentAsync(userId, request);

        return ToActionResult(result);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangeUserPasswordRequest request)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _userAccountService.ChangePasswordAsync(userId, request);

        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.Succeeded)
        {
            return Ok(result.Data);
        }

        var errorResponse = new { errors = result.Errors };

        return result.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
