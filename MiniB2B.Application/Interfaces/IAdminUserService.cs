using MiniB2B.Application.DTOs.Common;
using MiniB2B.Application.DTOs.Users;

namespace MiniB2B.Application.Interfaces;

public interface IAdminUserService
{
    Task<PagedResponse<AdminUserListItemDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<ServiceResult<AdminUserDetailDto>> GetByIdAsync(int id);
    Task<ServiceResult<AdminUserDetailDto>> UpdateAsync(int id, int currentAdminUserId, UpdateUserRequest request);
}
