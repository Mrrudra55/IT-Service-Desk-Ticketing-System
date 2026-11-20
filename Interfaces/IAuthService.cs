using ITServiceDesk.DTOs.Auth;

namespace ITServiceDesk.Interfaces;

public interface IAuthService
{
    // Registers a new employee account, hashes password, and returns a JWT token
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    // Registers a new administrator account after validating the secret security key
    Task<AuthResponseDto> RegisterAdminAsync(AdminRegisterRequestDto request);

    // Validates credentials, checks account status, and generates a JWT token
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    // Retrieves current authenticated user's profile details
    Task<UserProfileDto> GetCurrentUserProfileAsync(int userId);
}
