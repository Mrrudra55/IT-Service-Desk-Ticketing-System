using ITServiceDesk.DTOs.Categories;
using ITServiceDesk.DTOs.Common;
using ITServiceDesk.DTOs.Tickets;

namespace ITServiceDesk.Interfaces;

public interface ITicketService
{
    // Retrieve paginated and filtered tickets based on the user's role and identity
    Task<PagedResultDto<TicketResponseDto>> GetTicketsAsync(TicketFilterDto filter, int currentUserId, string currentUserRole);

    // Retrieve detailed ticket view including comments and history
    Task<TicketDetailDto> GetTicketByIdAsync(int id, int currentUserId, string currentUserRole);

    // Submit a new ticket (generates ticket number, calculates SLA due date, records history)
    Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto, int currentUserId);

    // Update ticket details (Title, Description, Category, Priority)
    Task<TicketResponseDto> UpdateTicketAsync(int id, UpdateTicketDto dto, int currentUserId, string currentUserRole);

    // Transition ticket workflow status (Open -> InProgress -> Resolved, etc.)
    Task<TicketResponseDto> UpdateStatusAsync(int id, UpdateStatusDto dto, int currentUserId, string currentUserRole);

    // Assign or reassign ticket to an IT agent
    Task<TicketResponseDto> AssignTicketAsync(int id, AssignTicketDto dto, int currentUserId, string currentUserRole);

    // Delete ticket (Admin only)
    Task DeleteTicketAsync(int id, int currentUserId, string currentUserRole);

    // Retrieve active ticket categories for dropdowns
    Task<List<CategoryResponseDto>> GetCategoriesAsync();
}

