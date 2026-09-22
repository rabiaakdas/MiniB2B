using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Users;
using MiniB2B.Application.Interfaces;
using MiniB2B.Application.Validation;
using MiniB2B.Infrastructure.Data;
using MiniB2B.Infrastructure.Identity;

namespace MiniB2B.Infrastructure.Services;

public class UserAccountService : IUserAccountService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAccountService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<ServiceResult<UserAccountDto>> GetCurrentAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return ServiceResult<UserAccountDto>.Failure(404, "Kullanıcı hesabı bulunamadı.");
        }

        return ServiceResult<UserAccountDto>.Success(ToDto(user));
    }

    public async Task<ServiceResult<UserAccountDto>> UpdateCurrentAsync(int userId, UpdateUserAccountRequest request)
    {
        var errors = ValidateAccountRequest(request.FirstName, request.LastName, request.Email, request.PhoneNumber);

        if (errors.Count > 0)
        {
            return ServiceResult<UserAccountDto>.Failure(400, errors.ToArray());
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return ServiceResult<UserAccountDto>.Failure(404, "Kullanıcı hesabı bulunamadı.");
        }

        var email = request.Email.Trim();
        var normalizedEmail = _userManager.NormalizeEmail(email);
        var emailOwner = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.NormalizedEmail == normalizedEmail && item.Id != userId);

        if (emailOwner is not null)
        {
            return ServiceResult<UserAccountDto>.Failure(400, "Bu email adresi zaten kullanılıyor.");
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = NormalizeOptional(request.PhoneNumber);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return ServiceResult<UserAccountDto>.Failure(
                400,
                result.Errors.Select(error => error.Description).ToArray());
        }

        return ServiceResult<UserAccountDto>.Success(ToDto(user));
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangeUserPasswordRequest request)
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

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return ServiceResult<bool>.Failure(404, "Kullanıcı hesabı bulunamadı.");
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

    private static UserAccountDto ToDto(ApplicationUser user)
    {
        return new UserAccountDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber
        };
    }

    private static List<string> ValidateAccountRequest(string firstName, string lastName, string email, string? phoneNumber)
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
