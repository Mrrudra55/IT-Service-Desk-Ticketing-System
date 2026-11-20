namespace ITServiceDesk.DTOs.Auth;

public class AuthResponseDto
{
    // The signed JWT Bearer token
    public string Token { get; set; } = string.Empty;

    // Token expiration time in UTC
    public DateTime ExpiresAt { get; set; }

    // User profile information
    public UserProfileDto User { get; set; } = null!;
}

