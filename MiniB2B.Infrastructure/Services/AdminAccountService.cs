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

public class AdminAccountService : IAdminAccountService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminAccountService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<ServiceResult<AdminAccountDto>> GetCurrentAsync(int adminUserId)
    {
        var user = await _userManager.FindByIdAsync(adminUserId.ToString());

        if (user is null)
        {
            return ServiceResult<AdminAccountDto>.Failure(404, "Admin hesabı bulunamadı.");
        }

        return ServiceResult<AdminAccountDto>.Success(ToDto(user));
    }

    public async Task<ServiceResult<AdminAccountDto>> UpdateCurrentAsync(
        int adminUserId,
        UpdateAdminAccountRequest request)
    {
        var errors = ValidateAccountRequest(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        if (errors.Count > 0)
        {
            return ServiceResult<AdminAccountDto>.Failure(400, errors.ToArray());
        }

        var user = await _userManager.FindByIdAsync(adminUserId.ToString());

        if (user is null)
        {
            return ServiceResult<AdminAccountDto>.Failure(404, "Admin hesabı bulunamadı.");
        }

        var normalizedEmail = _userManager.NormalizeEmail(request.Email.Trim());
        var emailOwner = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail && item.Id != adminUserId);

        if (emailOwner is not null)
        {
            return ServiceResult<AdminAccountDto>.Failure(400, "Bu email adresi zaten kullanılıyor.");
        }

        var email = request.Email.Trim();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = NormalizeOptional(request.PhoneNumber);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return ServiceResult<AdminAccountDto>.Failure(
                400,
                result.Errors.Select(error => error.Description).ToArray());
        }

        return ServiceResult<AdminAccountDto>.Success(ToDto(user));
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(
        int adminUserId,
        ChangeAdminPasswordRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            errors.Add("Mevcut şifre zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            errors.Add("Yeni şifre zorunludur.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            errors.Add("Yeni şifreler eşleşmiyor.");
        }

        if (errors.Count > 0)
        {
            return ServiceResult<bool>.Failure(400, errors.ToArray());
        }

        var user = await _userManager.FindByIdAsync(adminUserId.ToString());

        if (user is null)
        {
            return ServiceResult<bool>.Failure(404, "Admin hesabı bulunamadı.");
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return ServiceResult<bool>.Failure(
                400,
                result.Errors.Select(error => error.Description).ToArray());
        }

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<AdminAccountDto>> CreateAdminAsync(CreateAdminRequest request)
    {
        var errors = ValidateAccountRequest(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Şifre zorunludur.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            errors.Add("Şifreler eşleşmiyor.");
        }

        if (errors.Count > 0)
        {
            return ServiceResult<AdminAccountDto>.Failure(400, errors.ToArray());
        }

        var normalizedEmail = _userManager.NormalizeEmail(request.Email.Trim());
        var emailExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail);

        if (emailExists)
        {
            return ServiceResult<AdminAccountDto>.Failure(400, "Bu email adresi zaten kullanılıyor.");
        }

        var email = request.Email.Trim();
        var admin = new ApplicationUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            UserName = email,
            PhoneNumber = NormalizeOptional(request.PhoneNumber),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(admin, request.Password);

        if (!createResult.Succeeded)
        {
            return ServiceResult<AdminAccountDto>.Failure(
                400,
                createResult.Errors.Select(error => error.Description).ToArray());
        }

        var roleResult = await _userManager.AddToRoleAsync(admin, Roles.Admin);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(admin);

            return ServiceResult<AdminAccountDto>.Failure(
                400,
                roleResult.Errors.Select(error => error.Description).ToArray());
        }

        return ServiceResult<AdminAccountDto>.Success(ToDto(admin), 201);
    }

    private static AdminAccountDto ToDto(ApplicationUser user)
    {
        return new AdminAccountDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber
        };
    }

    private static List<string> ValidateAccountRequest(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            errors.Add("Ad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            errors.Add("Soyad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email zorunludur.");
        }
        else if (!EmailValidator.IsValidEmail(email))
        {
            errors.Add(EmailValidator.ErrorMessage);
        }

        if (firstName.Trim().Length > 100)
        {
            errors.Add("Ad en fazla 100 karakter olabilir.");
        }

        if (lastName.Trim().Length > 100)
        {
            errors.Add("Soyad en fazla 100 karakter olabilir.");
        }

        if (!PhoneNumberValidator.IsValidOptionalPhoneNumber(phoneNumber))
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
