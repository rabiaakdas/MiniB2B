using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.DTOs.Banners;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.Interfaces;
using MiniB2B.Domain.Entities;
using MiniB2B.Infrastructure.Data;

namespace MiniB2B.Infrastructure.Services;

public class BannerService : IBannerService
{
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;

    public BannerService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<BannerDto>> GetAdminPagedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _dbContext.Banners.AsNoTracking();
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(banner => banner.DisplayOrder)
            .ThenBy(banner => banner.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(banner => ToDtoExpression(banner))
            .ToListAsync();

        return new PagedResponse<BannerDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<BannerDto?> GetByIdAsync(int id)
    {
        return await _dbContext.Banners
            .AsNoTracking()
            .Where(banner => banner.Id == id)
            .Select(banner => ToDtoExpression(banner))
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<BannerDto>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;

        return await _dbContext.Banners
            .AsNoTracking()
            .Where(banner =>
                banner.IsActive &&
                (banner.StartDate == null || banner.StartDate <= now) &&
                (banner.EndDate == null || banner.EndDate >= now))
            .OrderBy(banner => banner.DisplayOrder)
            .ThenBy(banner => banner.Id)
            .Select(banner => ToDtoExpression(banner))
            .ToListAsync();
    }

    public async Task<ServiceResult<BannerDto>> CreateAsync(CreateBannerRequest request)
    {
        var errors = ValidateBanner(
            request.Title,
            request.ImagePath,
            request.DisplayOrder,
            request.StartDate,
            request.EndDate);

        if (errors.Count > 0)
        {
            return ServiceResult<BannerDto>.Failure(400, errors.ToArray());
        }

        var banner = new Banner
        {
            Title = request.Title.Trim(),
            Subtitle = NormalizeOptional(request.Subtitle),
            ImagePath = request.ImagePath.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _dbContext.Banners.Add(banner);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<BannerDto>.Success(ToDto(banner), 201);
    }

    public async Task<ServiceResult<BannerDto>> UpdateAsync(int id, UpdateBannerRequest request)
    {
        var errors = ValidateBanner(
            request.Title,
            request.ImagePath,
            request.DisplayOrder,
            request.StartDate,
            request.EndDate);

        if (errors.Count > 0)
        {
            return ServiceResult<BannerDto>.Failure(400, errors.ToArray());
        }

        var banner = await _dbContext.Banners.FirstOrDefaultAsync(item => item.Id == id);

        if (banner is null)
        {
            return ServiceResult<BannerDto>.Failure(404, "Banner bulunamadı.");
        }

        banner.Title = request.Title.Trim();
        banner.Subtitle = NormalizeOptional(request.Subtitle);
        banner.ImagePath = request.ImagePath.Trim();
        banner.DisplayOrder = request.DisplayOrder;
        banner.IsActive = request.IsActive;
        banner.StartDate = request.StartDate;
        banner.EndDate = request.EndDate;

        await _dbContext.SaveChangesAsync();

        return ServiceResult<BannerDto>.Success(ToDto(banner));
    }

    public async Task<ServiceResult<BannerDto>> UpdateImageAsync(int id, string imagePath)
    {
        var banner = await _dbContext.Banners.FirstOrDefaultAsync(item => item.Id == id);

        if (banner is null)
        {
            return ServiceResult<BannerDto>.Failure(404, "Banner bulunamadı.");
        }

        banner.ImagePath = imagePath;

        await _dbContext.SaveChangesAsync();

        return ServiceResult<BannerDto>.Success(ToDto(banner));
    }

    public async Task<ServiceResult<BannerDto>> DeleteAsync(int id)
    {
        var banner = await _dbContext.Banners.FirstOrDefaultAsync(item => item.Id == id);

        if (banner is null)
        {
            return ServiceResult<BannerDto>.Failure(404, "Banner bulunamadı.");
        }

        var deletedBanner = ToDto(banner);

        _dbContext.Banners.Remove(banner);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<BannerDto>.Success(deletedBanner);
    }

    private static BannerDto ToDtoExpression(Banner banner)
    {
        return new BannerDto
        {
            Id = banner.Id,
            Title = banner.Title,
            Subtitle = banner.Subtitle,
            ImagePath = banner.ImagePath,
            DisplayOrder = banner.DisplayOrder,
            IsActive = banner.IsActive,
            StartDate = banner.StartDate,
            EndDate = banner.EndDate
        };
    }

    private static BannerDto ToDto(Banner banner)
    {
        return ToDtoExpression(banner);
    }

    private static List<string> ValidateBanner(
        string title,
        string imagePath,
        int displayOrder,
        DateTime? startDate,
        DateTime? endDate)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Başlık zorunludur.");
        }

        if (title.Trim().Length > 150)
        {
            errors.Add("Başlık en fazla 150 karakter olabilir.");
        }

        if (string.IsNullOrWhiteSpace(imagePath))
        {
            errors.Add("Banner görsel yolu zorunludur.");
        }

        if (!IsSafeBannerImagePath(imagePath))
        {
            errors.Add("Banner görsel yolu geçersizdir.");
        }

        if (displayOrder < 0)
        {
            errors.Add("Sıralama değeri negatif olamaz.");
        }

        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
        {
            errors.Add("Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        return errors;
    }

    private static bool IsSafeBannerImagePath(string imagePath)
    {
        var trimmed = imagePath.Trim();

        return trimmed.StartsWith("/uploads/banners/", StringComparison.OrdinalIgnoreCase)
            && !trimmed.Contains("..", StringComparison.Ordinal)
            && !trimmed.Contains('\\', StringComparison.Ordinal);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
