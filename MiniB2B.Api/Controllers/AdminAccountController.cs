using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Api.Extensions;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Users;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/admin/account")]
[Authorize(Roles = Roles.Admin)]
public class AdminAccountController : ControllerBase
{
    private readonly IAdminAccountService _adminAccountService;

    public AdminAccountController(IAdminAccountService adminAccountService)
    {
        _adminAccountService = adminAccountService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrent()
    {
        if (!User.TryGetUserId(out var adminUserId))
        {
            return Unauthorized();
        }

        var result = await _adminAccountService.GetCurrentAsync(adminUserId);

        return ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCurrent(UpdateAdminAccountRequest request)
    {
        if (!User.TryGetUserId(out var adminUserId))
        {
            return Unauthorized();
        }

        var result = await _adminAccountService.UpdateCurrentAsync(adminUserId, request);

        return ToActionResult(result);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangeAdminPasswordRequest request)
    {
        if (!User.TryGetUserId(out var adminUserId))
        {
            return Unauthorized();
        }

        var result = await _adminAccountService.ChangePasswordAsync(adminUserId, request);

        return ToActionResult(result);
    }

    [HttpPost("admins")]
    public async Task<IActionResult> CreateAdmin(CreateAdminRequest request)
    {
        var result = await _adminAccountService.CreateAdminAsync(request);

        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
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
            StatusCodes.Status404NotFound => NotFound(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
