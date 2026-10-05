using UserManagementApi.DTOs;

namespace UserManagementApi.Services;

public interface IUserService
{
    Task<PagedResult<UserResponse>> GetAllAsync(int page, int pageSize, string? search);
    Task<UserResponse> GetByIdAsync(Guid id);
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request);
    Task DeleteAsync(Guid id);
}
