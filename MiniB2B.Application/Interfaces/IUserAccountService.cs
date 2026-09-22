using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Users;

namespace MiniB2B.Application.Interfaces;

public interface IUserAccountService
{
    Task<ServiceResult<UserAccountDto>> GetCurrentAsync(int userId);
    Task<ServiceResult<UserAccountDto>> UpdateCurrentAsync(int userId, UpdateUserAccountRequest request);
    Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangeUserPasswordRequest request);
}
