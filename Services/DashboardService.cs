using ITServiceDesk.Data;
using ITServiceDesk.DTOs.Dashboard;
using ITServiceDesk.DTOs.Tickets;
using ITServiceDesk.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServiceDesk.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int userId)
    {
        var userTickets = _context.Tickets.Where(t => t.CreatedByUserId == userId);

        var total = await userTickets.CountAsync();
        var open = await userTickets.CountAsync(t => t.Status == "Open");
        var inProgress = await userTickets.CountAsync(t => t.Status == "InProgress");
        var resolved = await userTickets.CountAsync(t => t.Status == "Resolved" || t.Status == "Closed");

        var recentTickets = await userTickets
            .Include(t => t.Category)
            .Include(t => t.AssignedToUser)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : "General",
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.Name : "Unassigned",
                CreatedAt = t.CreatedAt,
                DueAt = t.DueAt
            })
            .ToListAsync();

        return new EmployeeDashboardDto
        {
            TotalTickets = total,
            OpenTickets = open,
            InProgressTickets = inProgress,
            ResolvedTickets = resolved,
            RecentTickets = recentTickets
        };
    }

    public async Task<AgentDashboardDto> GetAgentDashboardAsync(int agentId)
    {
        // For backwards compatibility or Admin quick dashboard
        var myTickets = _context.Tickets.Where(t => t.AssignedToUserId == agentId);
        var unassignedQuery = _context.Tickets
            .Where(t => t.AssignedToUserId == null && t.Status != "Closed" && t.Status != "Resolved");

        var now = DateTime.UtcNow;

        var assignedCount = await myTickets.CountAsync();
        var unassignedCount = await unassignedQuery.CountAsync();
        var myOpen = await myTickets.CountAsync(t => t.Status == "Open");
        var myInProgress = await myTickets.CountAsync(t => t.Status == "InProgress");
        var myResolved = await myTickets.CountAsync(t => t.Status == "Resolved" || t.Status == "Closed");

        var overdueCount = await myTickets
            .CountAsync(t => t.DueAt.HasValue && t.DueAt.Value < now && t.Status != "Resolved" && t.Status != "Closed");

        var assignedList = await myTickets
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : "General",
                CreatedByUserName = t.CreatedByUser != null ? t.CreatedByUser.Name : "Unknown",
                CreatedAt = t.CreatedAt,
                DueAt = t.DueAt
            })
            .ToListAsync();

        var unassignedList = await unassignedQuery
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .OrderByDescending(t => t.Priority == "Critical" ? 4 : t.Priority == "High" ? 3 : t.Priority == "Medium" ? 2 : 1)
            .ThenBy(t => t.CreatedAt)
            .Take(5)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : "General",
                CreatedByUserName = t.CreatedByUser != null ? t.CreatedByUser.Name : "Unknown",
                CreatedAt = t.CreatedAt,
                DueAt = t.DueAt
            })
            .ToListAsync();

        return new AgentDashboardDto
        {
            AssignedToMeCount = assignedCount,
            UnassignedQueueCount = unassignedCount,
            MyOpenTicketsCount = myOpen,
            MyInProgressTicketsCount = myInProgress,
            MyResolvedTicketsCount = myResolved,
            OverdueTicketsCount = overdueCount,
            AssignedTickets = assignedList,
            UnassignedTickets = unassignedList
        };
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var now = DateTime.UtcNow;

        var totalUsers = await _context.Users.CountAsync();
        var totalTickets = await _context.Tickets.CountAsync();
        var openTickets = await _context.Tickets.CountAsync(t => t.Status == "Open");
        var inProgressTickets = await _context.Tickets.CountAsync(t => t.Status == "InProgress");
        var resolvedTickets = await _context.Tickets.CountAsync(t => t.Status == "Resolved" || t.Status == "Closed");
        var criticalTickets = await _context.Tickets.CountAsync(t => t.Priority == "Critical" && t.Status != "Resolved" && t.Status != "Closed");
        var overdueTickets = await _context.Tickets.CountAsync(t => t.DueAt.HasValue && t.DueAt.Value < now && t.Status != "Resolved" && t.Status != "Closed");

        // Calculate Average Resolution Time in hours
        var resolvedList = await _context.Tickets
            .Where(t => t.ResolvedAt.HasValue)
            .Select(t => new { t.CreatedAt, ResolvedAt = t.ResolvedAt!.Value })
            .ToListAsync();

        double avgHours = 0;
        if (resolvedList.Any())
        {
            avgHours = Math.Round(resolvedList.Average(t => (t.ResolvedAt - t.CreatedAt).TotalHours), 1);
        }

        // Breakdown by Category
        var categoryData = await _context.Categories
            .Select(c => new ChartItemDto
            {
                Label = c.Name,
                Count = c.Tickets.Count
            })
            .ToListAsync();

        // Breakdown by Priority
        var priorityData = await _context.Tickets
            .GroupBy(t => t.Priority)
            .Select(g => new ChartItemDto
            {
                Label = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // Breakdown by Status
        var statusData = await _context.Tickets
            .GroupBy(t => t.Status)
            .Select(g => new ChartItemDto
            {
                Label = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // Admin Workload (All active administrators handling tickets)
        var admins = await _context.Users
            .Where(u => u.Role == "Admin")
            .Select(u => new AgentWorkloadDto
            {
                AgentId = u.Id,
                AgentName = u.Name,
                AgentEmail = u.Email,
                AssignedTicketsCount = u.AssignedTickets.Count(t => t.Status != "Closed" && t.Status != "Resolved"),
                ResolvedTicketsCount = u.AssignedTickets.Count(t => t.Status == "Resolved" || t.Status == "Closed")
            })
            .ToListAsync();

        // Recent 5 Tickets
        var recent = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CategoryName = t.Category != null ? t.Category.Name : "General",
                CreatedByUserName = t.CreatedByUser != null ? t.CreatedByUser.Name : "Unknown",
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.Name : "Unassigned",
                CreatedAt = t.CreatedAt,
                DueAt = t.DueAt
            })
            .ToListAsync();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalTickets = totalTickets,
            OpenTickets = openTickets,
            InProgressTickets = inProgressTickets,
            ResolvedTickets = resolvedTickets,
            CriticalTickets = criticalTickets,
            OverdueTickets = overdueTickets,
            AverageResolutionHours = avgHours,
            TicketsByCategory = categoryData,
            TicketsByPriority = priorityData,
            TicketsByStatus = statusData,
            AgentWorkload = admins,
            RecentTickets = recent
        };
    }
}
