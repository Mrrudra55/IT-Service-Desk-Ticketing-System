using ITServiceDesk.DTOs.Tickets;

namespace ITServiceDesk.DTOs.Dashboard;

public class ChartItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AgentWorkloadDto
{
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public int AssignedTicketsCount { get; set; }
    public int ResolvedTicketsCount { get; set; }
}

public class EmployeeDashboardDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public List<TicketResponseDto> RecentTickets { get; set; } = new List<TicketResponseDto>();
}

public class AgentDashboardDto
{
    public int AssignedToMeCount { get; set; }
    public int UnassignedQueueCount { get; set; }
    public int MyOpenTicketsCount { get; set; }
    public int MyInProgressTicketsCount { get; set; }
    public int MyResolvedTicketsCount { get; set; }
    public int OverdueTicketsCount { get; set; }
    public List<TicketResponseDto> AssignedTickets { get; set; } = new List<TicketResponseDto>();
    public List<TicketResponseDto> UnassignedTickets { get; set; } = new List<TicketResponseDto>();
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int CriticalTickets { get; set; }
    public int OverdueTickets { get; set; }
    public double AverageResolutionHours { get; set; }

    public List<ChartItemDto> TicketsByCategory { get; set; } = new List<ChartItemDto>();
    public List<ChartItemDto> TicketsByPriority { get; set; } = new List<ChartItemDto>();
    public List<ChartItemDto> TicketsByStatus { get; set; } = new List<ChartItemDto>();
    public List<AgentWorkloadDto> AgentWorkload { get; set; } = new List<AgentWorkloadDto>();
    public List<TicketResponseDto> RecentTickets { get; set; } = new List<TicketResponseDto>();
}

