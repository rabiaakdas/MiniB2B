using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Users;

namespace MiniB2B.Application.Interfaces;

public interface IAdminAccountService
{
    Task<ServiceResult<AdminAccountDto>> GetCurrentAsync(int adminUserId);
    Task<ServiceResult<AdminAccountDto>> UpdateCurrentAsync(int adminUserId, UpdateAdminAccountRequest request);
    Task<ServiceResult<bool>> ChangePasswordAsync(int adminUserId, ChangeAdminPasswordRequest request);
    Task<ServiceResult<AdminAccountDto>> CreateAdminAsync(CreateAdminRequest request);
}
