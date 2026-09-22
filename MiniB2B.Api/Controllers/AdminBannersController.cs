using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniB2B.Api.Services;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Banners;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/admin/banners")]
[Authorize(Roles = Roles.Admin)]
public class AdminBannersController : ControllerBase
{
    private const long MaxImageSizeInBytes = 5 * 1024 * 1024;

    private readonly IBannerService _bannerService;
    private readonly IImageStorageService _imageStorageService;

    public AdminBannersController(
        IBannerService bannerService,
        IImageStorageService imageStorageService)
    {
        _bannerService = bannerService;
        _imageStorageService = imageStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBanners(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _bannerService.GetAdminPagedAsync(page, pageSize);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBanner(int id)
    {
        var banner = await _bannerService.GetByIdAsync(id);

        if (banner is null)
        {
            return NotFound(new { errors = new[] { "Banner bulunamadı." } });
        }

        return Ok(banner);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBanner(CreateBannerRequest request)
    {
        var result = await _bannerService.CreateAsync(request);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return CreatedAtAction(nameof(GetBanner), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBanner(int id, UpdateBannerRequest request)
    {
        var result = await _bannerService.UpdateAsync(id, request);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:int}/image")]
    [RequestSizeLimit(MaxImageSizeInBytes)]
    public async Task<IActionResult> UploadImage(int id, [FromForm] IFormFile file)
    {
        if (file is null)
        {
            return BadRequest(new { errors = new[] { "Dosya zorunludur." } });
        }

        var existingBanner = await _bannerService.GetByIdAsync(id);

        if (existingBanner is null)
        {
            return NotFound(new { errors = new[] { "Banner bulunamadı." } });
        }

        var imageResult = await _imageStorageService.SaveImageAsync(file, "banners");

        if (!imageResult.Succeeded
            || string.IsNullOrWhiteSpace(imageResult.RelativePath))
        {
            return BadRequest(new { errors = imageResult.Errors });
        }

        try
        {
            var result = await _bannerService.UpdateImageAsync(id, imageResult.RelativePath);

            if (!result.Succeeded || result.Data is null)
            {
                _imageStorageService.DeleteIfSafe(imageResult.RelativePath, "banners");
                return ToErrorResult(result);
            }

            _imageStorageService.DeleteIfSafe(existingBanner.ImagePath, "banners");

            return Ok(result.Data);
        }
        catch
        {
            _imageStorageService.DeleteIfSafe(imageResult.RelativePath, "banners");
            throw;
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBanner(int id)
    {
        var result = await _bannerService.DeleteAsync(id);

        if (!result.Succeeded || result.Data is null)
        {
            return ToErrorResult(result);
        }

        _imageStorageService.DeleteIfSafe(result.Data.ImagePath, "banners");

        return NoContent();
    }

    private IActionResult ToErrorResult<T>(ServiceResult<T> result)
    {
        var errorResponse = new { errors = result.Errors };

        return result.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
