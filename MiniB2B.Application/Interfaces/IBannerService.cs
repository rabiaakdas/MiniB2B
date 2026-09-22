using MiniB2B.Application.DTOs.Banners;
using MiniB2B.Application.DTOs.Common;

namespace MiniB2B.Application.Interfaces;

public interface IBannerService
{
    Task<PagedResponse<BannerDto>> GetAdminPagedAsync(int page, int pageSize);
    Task<BannerDto?> GetByIdAsync(int id);
    Task<IReadOnlyList<BannerDto>> GetActiveAsync();
    Task<ServiceResult<BannerDto>> CreateAsync(CreateBannerRequest request);
    Task<ServiceResult<BannerDto>> UpdateAsync(int id, UpdateBannerRequest request);
    Task<ServiceResult<BannerDto>> UpdateImageAsync(int id, string imagePath);
    Task<ServiceResult<BannerDto>> DeleteAsync(int id);
}
