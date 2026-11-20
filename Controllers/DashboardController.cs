using System.Security.Claims;
using ITServiceDesk.DTOs.Dashboard;
using ITServiceDesk.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServiceDesk.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Retrieves personalized metrics and recent tickets for the employee dashboard.
    /// </summary>
    [HttpGet("employee")]
    public async Task<ActionResult<EmployeeDashboardDto>> GetEmployeeDashboard()
    {
        var (userId, _) = GetCurrentUser();
        var data = await _dashboardService.GetEmployeeDashboardAsync(userId);
        return Ok(data);
    }

    /// <summary>
    /// Retrieves workload metrics, unassigned queue, and SLA stats for administrators.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("agent")]
    public async Task<ActionResult<AgentDashboardDto>> GetAgentDashboard()
    {
        var (userId, _) = GetCurrentUser();
        var data = await _dashboardService.GetAgentDashboardAsync(userId);
        return Ok(data);
    }

    /// <summary>
    /// Retrieves organization-wide metrics, breakdowns by category/priority/status, and admin workloads.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard()
    {
        var data = await _dashboardService.GetAdminDashboardAsync();
        return Ok(data);
    }

    private (int UserId, string UserRole) GetCurrentUser()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(idClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid authentication token.");
        }

        return (userId, roleClaim ?? "Employee");
    }
}
