using ITServiceDesk.DTOs.Dashboard;

namespace ITServiceDesk.Interfaces;

public interface IDashboardService
{
    // Real metrics and stats for Employee Dashboard
    Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int userId);

    // Workload, unassigned queue, and SLA stats for Agent Dashboard
    Task<AgentDashboardDto> GetAgentDashboardAsync(int agentId);

    // Organization-wide analytics, breakdowns, and workload for Admin Dashboard
    Task<AdminDashboardDto> GetAdminDashboardAsync();
}

