using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Users;
using MiniB2B.Application.Interfaces;
using MiniB2B.Application.Validation;
using MiniB2B.Infrastructure.Data;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<PagedResponse<AdminUserListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = ProjectManageableUsers();
        var trimmedSearch = search?.Trim();

        if (!string.IsNullOrWhiteSpace(trimmedSearch))
        {
            query = query.Where(user =>
                user.FirstName.Contains(trimmedSearch) ||
                user.LastName.Contains(trimmedSearch) ||
                user.Email!.Contains(trimmedSearch) ||
                (user.PhoneNumber != null && user.PhoneNumber.Contains(trimmedSearch)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(user => user.CreatedAt)
            .ThenByDescending(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new AdminUserListItemDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<AdminUserListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ServiceResult<AdminUserDetailDto>> GetByIdAsync(int id)
    {
        var user = await ProjectManageableUsers()
            .Select(user => new AdminUserDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            })
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
        {
            return ServiceResult<AdminUserDetailDto>.Failure(404, "Kullanıcı bulunamadı.");
        }

        return ServiceResult<AdminUserDetailDto>.Success(user);
    }

    public async Task<ServiceResult<AdminUserDetailDto>> UpdateAsync(
        int id,
        int currentAdminUserId,
        UpdateUserRequest request)
    {
        var errors = ValidateUpdateRequest(request);

        if (id == currentAdminUserId && !request.IsActive)
        {
            errors.Add("Admin kendi hesabını pasif hale getiremez.");
        }

        if (errors.Count > 0)
        {
            return ServiceResult<AdminUserDetailDto>.Failure(400, errors.ToArray());
        }

        var isAdminUser = await IsAdminUserAsync(id);

        if (isAdminUser)
        {
            return ServiceResult<AdminUserDetailDto>.Failure(404, "Kullanıcı bulunamadı.");
        }

        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return ServiceResult<AdminUserDetailDto>.Failure(404, "Kullanıcı bulunamadı.");
        }

        var normalizedEmail = _userManager.NormalizeEmail(request.Email.Trim());
        var emailOwner = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail && item.Id != id);

        if (emailOwner is not null)
        {
            return ServiceResult<AdminUserDetailDto>.Failure(400, "Bu email adresi zaten kullanılıyor.");
        }

        var email = request.Email.Trim();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = NormalizeOptional(request.PhoneNumber);
        user.IsActive = request.IsActive;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return ServiceResult<AdminUserDetailDto>.Failure(
                400,
                result.Errors.Select(error => error.Description).ToArray());
        }

        return await GetByIdAsync(id);
    }

    private IQueryable<ApplicationUser> ProjectManageableUsers()
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(user => !_dbContext.UserRoles.Any(userRole =>
                userRole.UserId == user.Id
                && _dbContext.Roles.Any(role =>
                    role.Id == userRole.RoleId
                    && role.Name == Roles.Admin)));
    }

    private async Task<bool> IsAdminUserAsync(int userId)
    {
        return await (
            from userRole in _dbContext.UserRoles
            join role in _dbContext.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId && role.Name == Roles.Admin
            select userRole
        ).AnyAsync();
    }

    private static List<string> ValidateUpdateRequest(UpdateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            errors.Add("Ad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            errors.Add("Soyad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email zorunludur.");
        }
        else if (!EmailValidator.IsValidEmail(request.Email))
        {
            errors.Add(EmailValidator.ErrorMessage);
        }

        if (request.FirstName.Trim().Length > 100)
        {
            errors.Add("Ad en fazla 100 karakter olabilir.");
        }

        if (request.LastName.Trim().Length > 100)
        {
            errors.Add("Soyad en fazla 100 karakter olabilir.");
        }

        if (!PhoneNumberValidator.IsValidOptionalPhoneNumber(request.PhoneNumber))
        {
            errors.Add(PhoneNumberValidator.ErrorMessage);
        }

        return errors;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
