using System.ComponentModel.DataAnnotations;

namespace ITServiceDesk.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedTicketsCount { get; set; }
    public int AssignedTicketsCount { get; set; }
}

public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Employee|Admin)$", ErrorMessage = "Role must be Employee or Admin.")]
    public string Role { get; set; } = "Employee";
}

public class UpdateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserRoleDto
{
    [Required]
    [RegularExpression("^(Employee|Admin)$", ErrorMessage = "Role must be Employee or Admin.")]
    public string Role { get; set; } = "Employee";
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}

public class UserFilterDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
