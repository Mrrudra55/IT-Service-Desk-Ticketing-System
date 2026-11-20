using ITServiceDesk.Data;
using ITServiceDesk.DTOs.Common;
using ITServiceDesk.DTOs.Users;
using ITServiceDesk.Interfaces;
using ITServiceDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace ITServiceDesk.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<UserDto>> GetUsersAsync(UserFilterDto filter)
    {
        var query = _context.Users
            .Include(u => u.CreatedTickets)
            .Include(u => u.AssignedTickets)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            query = query.Where(u => u.Role.ToLower() == filter.Role.Trim().ToLower());
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                CreatedTicketsCount = u.CreatedTickets.Count,
                AssignedTicketsCount = u.AssignedTickets.Count
            })
            .ToListAsync();

        return new PagedResultDto<UserDto>
        {
            Items = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<UserDto>> GetAssignableAgentsAsync()
    {
        return await _context.Users
            .Where(u => u.Role == "Admin" && u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                AssignedTicketsCount = u.AssignedTickets.Count(t => t.Status != "Closed" && t.Status != "Resolved")
            })
            .ToListAsync();
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.CreatedTickets)
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new KeyNotFoundException($"User #{id} was not found.");
        }

        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            throw new InvalidOperationException("A user with this email address already exists.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User #{id} was not found.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.Id != id))
        {
            throw new InvalidOperationException("Email address is already in use by another account.");
        }

        user.Name = dto.Name.Trim();
        user.Email = normalizedEmail;

        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserRoleAsync(int id, UpdateUserRoleDto dto, int currentAdminId)
    {
        if (id == currentAdminId && dto.Role != "Admin")
        {
            throw new InvalidOperationException("Administrators cannot demote their own account.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User #{id} was not found.");
        }

        user.Role = dto.Role;
        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserStatusAsync(int id, UpdateUserStatusDto dto, int currentAdminId)
    {
        if (id == currentAdminId && !dto.IsActive)
        {
            throw new InvalidOperationException("Administrators cannot deactivate their own account.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User #{id} was not found.");
        }

        user.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            CreatedTicketsCount = user.CreatedTickets?.Count ?? 0,
            AssignedTicketsCount = user.AssignedTickets?.Count ?? 0
        };
    }
}
