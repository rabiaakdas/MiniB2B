using Microsoft.AspNetCore.Mvc;
using MiniB2B.Application.Interfaces;

namespace MiniB2B.Api.Controllers;

[ApiController]
[Route("api/banners")]
public class BannersController : ControllerBase
{
    private readonly IBannerService _bannerService;

    public BannersController(IBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveBanners()
    {
        var banners = await _bannerService.GetActiveAsync();

        return Ok(banners);
    }
}
