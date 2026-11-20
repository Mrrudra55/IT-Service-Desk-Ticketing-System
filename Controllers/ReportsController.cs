using System.Text;
using ITServiceDesk.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITServiceDesk.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Exports all organizational tickets and performance metrics as an Excel-compatible CSV spreadsheet (Admin only).
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportTicketsCsv()
    {
        var tickets = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .OrderByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var sb = new StringBuilder();

        // CSV Header Row
        sb.AppendLine("Ticket Number,Title,Category,Priority,Status,Created By,Assigned Admin,Created Date,Resolved Date,Resolution Hours,SLA Deadline,SLA Breached");

        foreach (var t in tickets)
        {
            var isOverdue = t.DueAt.HasValue && t.DueAt.Value < DateTime.UtcNow && t.Status != "Resolved" && t.Status != "Closed";
            double? resolutionHours = null;
            if (t.ResolvedAt.HasValue)
            {
                resolutionHours = Math.Round((t.ResolvedAt.Value - t.CreatedAt).TotalHours, 1);
            }

            var line = string.Join(",",
                EscapeCsv(t.TicketNumber),
                EscapeCsv(t.Title),
                EscapeCsv(t.Category?.Name ?? "Uncategorized"),
                EscapeCsv(t.Priority),
                EscapeCsv(t.Status),
                EscapeCsv(t.CreatedByUser?.Name ?? "Unknown"),
                EscapeCsv(t.AssignedToUser?.Name ?? "Unassigned"),
                t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                t.ResolvedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                resolutionHours?.ToString() ?? "",
                t.DueAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                isOverdue ? "YES" : "NO"
            );

            sb.AppendLine(line);
        }

        var fileName = $"IT_Service_Desk_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        return File(bytes, "text/csv", fileName);
    }

    private static string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return $"\"{field}\"";
    }
}
