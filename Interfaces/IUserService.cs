using ITServiceDesk.DTOs.Common;
using ITServiceDesk.DTOs.Users;

namespace ITServiceDesk.Interfaces;

public interface IUserService
{
    // Paginated and searchable list of users (Admin only)
    Task<PagedResultDto<UserDto>> GetUsersAsync(UserFilterDto filter);

    // List active administrators (used for assigning tickets dropdown)
    Task<List<UserDto>> GetAssignableAgentsAsync();

    // User details by ID
    Task<UserDto> GetUserByIdAsync(int id);

    // Create a new user (Admin only)
    Task<UserDto> CreateUserAsync(CreateUserDto dto);

    // Update user details (Name, Email)
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto dto);

    // Change a user's role (Admin only)
    Task<UserDto> UpdateUserRoleAsync(int id, UpdateUserRoleDto dto, int currentAdminId);

    // Enable/Disable user account (Admin only)
    Task<UserDto> UpdateUserStatusAsync(int id, UpdateUserStatusDto dto, int currentAdminId);
}
