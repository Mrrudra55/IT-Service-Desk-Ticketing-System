using System.ComponentModel.DataAnnotations;

namespace ITServiceDesk.DTOs.Tickets;

public class CreateTicketDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Detailed description is required.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters long.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Priority must be Low, Medium, High, or Critical.")]
    public string Priority { get; set; } = "Medium";
}

