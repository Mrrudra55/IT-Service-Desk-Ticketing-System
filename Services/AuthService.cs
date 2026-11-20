using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ITServiceDesk.Data;
using ITServiceDesk.DTOs.Auth;
using ITServiceDesk.Interfaces;
using ITServiceDesk.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ITServiceDesk.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Normalize and check if email is already in use
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (existingUser)
        {
            throw new InvalidOperationException("An account with this email address already exists.");
        }

        // 2. Public registration creates strictly Employee accounts
        var assignedRole = "Employee";

        // 3. Hash password using BCrypt with automatic salting
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 4. Create and persist user entity
        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = assignedRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // 5. Generate JWT token
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> RegisterAdminAsync(AdminRegisterRequestDto request)
    {
        // 1. Validate Secret Admin Registration Key
        var expectedSecret = _configuration["AdminSecretKey"] ?? "ADMIN_SECRET_2026";
        if (string.IsNullOrWhiteSpace(request.AdminSecretKey) || request.AdminSecretKey.Trim() != expectedSecret.Trim())
        {
            throw new UnauthorizedAccessException("Invalid administrator registration security key. Authorization failed.");
        }

        // 2. Normalize and check if email is already in use
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (existingUser)
        {
            throw new InvalidOperationException("An account with this email address already exists.");
        }

        // 3. Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 4. Create and persist user entity with Admin role
        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // 5. Generate JWT token
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // 1. Locate user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email address or password.");
        }

        // 2. Check if account is active
        if (!user.IsActive)
        {
            throw new InvalidOperationException("This account has been deactivated. Please contact your IT administrator.");
        }

        // 3. Verify password hash using BCrypt
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email address or password.");
        }

        // 4. Generate JWT token
        return GenerateAuthResponse(user);
    }

    public async Task<UserProfileDto> GetCurrentUserProfileAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return MapToProfileDto(user);
    }

    // --- Helper Methods ---

    private AuthResponseDto GenerateAuthResponse(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var minutes) ? minutes : 1440;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // Claims embedded inside the token payload
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(securityToken);

        return new AuthResponseDto
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
            User = MapToProfileDto(user)
        };
    }

    private static UserProfileDto MapToProfileDto(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
