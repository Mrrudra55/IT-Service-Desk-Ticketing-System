using ITServiceDesk.Data;
using ITServiceDesk.DTOs.Categories;
using ITServiceDesk.DTOs.Comments;
using ITServiceDesk.DTOs.Common;
using ITServiceDesk.DTOs.Tickets;
using ITServiceDesk.Interfaces;
using ITServiceDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace ITServiceDesk.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;

    public TicketService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<TicketResponseDto>> GetTicketsAsync(
        TicketFilterDto filter, int currentUserId, string currentUserRole)
    {
        var query = _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .AsNoTracking()
            .AsQueryable();

        // 1. Role-based security boundary
        if (currentUserRole == "Employee")
        {
            // Employees are strictly restricted to tickets they submitted
            query = query.Where(t => t.CreatedByUserId == currentUserId);
        }

        // 2. Filters
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(t => 
                t.TicketNumber.ToLower().Contains(search) || 
                t.Title.ToLower().Contains(search) || 
                t.Description.ToLower().Contains(search));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(t => t.Status.ToLower() == filter.Status.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filter.Priority))
        {
            query = query.Where(t => t.Priority.ToLower() == filter.Priority.Trim().ToLower());
        }

        if (filter.AssignedToUserId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId.Value);
        }

        if (filter.UnassignedOnly == true)
        {
            query = query.Where(t => t.AssignedToUserId == null);
        }

        // 3. Sorting
        query = (filter.SortBy?.ToLower()) switch
        {
            "oldest" => query.OrderBy(t => t.CreatedAt),
            "priority" => query.OrderByDescending(t => 
                t.Priority == "Critical" ? 4 : 
                t.Priority == "High" ? 3 : 
                t.Priority == "Medium" ? 2 : 1),
            "deadline" => query.OrderBy(t => t.DueAt ?? DateTime.MaxValue),
            _ => query.OrderByDescending(t => t.CreatedAt) // default: newest
        };

        // 4. Pagination
        var totalCount = await query.CountAsync();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 50 ? 50 : filter.PageSize);

        var tickets = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : "Uncategorized",
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser != null ? t.CreatedByUser.Name : "Unknown",
                CreatedByUserEmail = t.CreatedByUser != null ? t.CreatedByUser.Email : "",
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.Name : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                DueAt = t.DueAt,
                ResolvedAt = t.ResolvedAt,
                CommentsCount = t.Comments.Count
            })
            .ToListAsync();

        return new PagedResultDto<TicketResponseDto>
        {
            Items = tickets,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TicketDetailDto> GetTicketByIdAsync(int id, int currentUserId, string currentUserRole)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .Include(t => t.Histories)
                .ThenInclude(h => h.ChangedByUser)
            .Include(t => t.Attachments)
                .ThenInclude(a => a.UploadedByUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID #{id} was not found.");
        }

        // Enforce employee ownership
        if (currentUserRole == "Employee" && ticket.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this ticket.");
        }

        return new TicketDetailDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category?.Name ?? "Uncategorized",
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByUserName = ticket.CreatedByUser?.Name ?? "Unknown",
            CreatedByUserEmail = ticket.CreatedByUser?.Email ?? "",
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser?.Name,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            DueAt = ticket.DueAt,
            ResolvedAt = ticket.ResolvedAt,
            CommentsCount = ticket.Comments.Count,
            Comments = ticket.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    UserId = c.UserId,
                    AuthorName = c.User?.Name ?? "Unknown",
                    AuthorRole = c.User?.Role ?? "Employee",
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToList(),
            Histories = ticket.Histories
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new TicketHistoryDto
                {
                    Id = h.Id,
                    TicketId = h.TicketId,
                    ChangedByUserId = h.ChangedByUserId,
                    ChangedByUserName = h.ChangedByUser?.Name ?? "System",
                    ChangedByUserRole = h.ChangedByUser?.Role ?? "System",
                    Action = h.Action,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    CreatedAt = h.CreatedAt
                })
                .ToList(),
            Attachments = ticket.Attachments
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new DTOs.Attachments.AttachmentDto
                {
                    Id = a.Id,
                    TicketId = a.TicketId,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                    UploadedByUserId = a.UploadedByUserId,
                    UploadedByUserName = a.UploadedByUser?.Name ?? "Unknown",
                    CreatedAt = a.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto, int currentUserId)
    {
        // 1. Verify category exists
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null || !category.IsActive)
        {
            throw new ArgumentException("The specified ticket category does not exist or is inactive.");
        }

        var now = DateTime.UtcNow;

        // 2. Calculate SLA Due Date based on priority
        var dueAt = dto.Priority switch
        {
            "Critical" => now.AddHours(4),
            "High" => now.AddHours(24),
            "Medium" => now.AddHours(48),
            _ => now.AddHours(72) // Low
        };

        // 3. Generate sequential human-readable Ticket Number: TCK-yyyyMM-XXXX
        var monthlyCount = await _context.Tickets
            .CountAsync(t => t.CreatedAt.Year == now.Year && t.CreatedAt.Month == now.Month);
        var ticketNumber = $"TCK-{now:yyyyMM}-{(monthlyCount + 1):D4}";

        var ticket = new Ticket
        {
            TicketNumber = ticketNumber,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            CategoryId = dto.CategoryId,
            Priority = dto.Priority,
            Status = "Open",
            CreatedByUserId = currentUserId,
            CreatedAt = now,
            DueAt = dueAt
        };

        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();

        // 4. Record audit history
        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            ChangedByUserId = currentUserId,
            Action = "TicketCreated",
            OldValue = null,
            NewValue = "Open",
            CreatedAt = now
        };
        await _context.TicketHistories.AddAsync(history);
        await _context.SaveChangesAsync();

        var creator = await _context.Users.FindAsync(currentUserId);

        return new TicketResponseDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CategoryId = ticket.CategoryId,
            CategoryName = category.Name,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByUserName = creator?.Name ?? "",
            CreatedByUserEmail = creator?.Email ?? "",
            CreatedAt = ticket.CreatedAt,
            DueAt = ticket.DueAt,
            CommentsCount = 0
        };
    }

    public async Task<TicketResponseDto> UpdateTicketAsync(
        int id, UpdateTicketDto dto, int currentUserId, string currentUserRole)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID #{id} was not found.");
        }

        // Ownership rules for editing:
        // Employees can only edit if status is still "Open" and they own it
        if (currentUserRole == "Employee")
        {
            if (ticket.CreatedByUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You cannot edit a ticket created by someone else.");
            }
            if (ticket.Status != "Open")
            {
                throw new InvalidOperationException("Tickets that are in progress or resolved can no longer be edited by employees.");
            }
        }

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null || !category.IsActive)
        {
            throw new ArgumentException("The specified ticket category is invalid.");
        }

        // Track Priority changes in history
        if (ticket.Priority != dto.Priority)
        {
            _context.TicketHistories.Add(new TicketHistory
            {
                TicketId = ticket.Id,
                ChangedByUserId = currentUserId,
                Action = "PriorityChanged",
                OldValue = ticket.Priority,
                NewValue = dto.Priority,
                CreatedAt = DateTime.UtcNow
            });
            ticket.Priority = dto.Priority;
        }

        // Track Category changes in history
        if (ticket.CategoryId != dto.CategoryId)
        {
            _context.TicketHistories.Add(new TicketHistory
            {
                TicketId = ticket.Id,
                ChangedByUserId = currentUserId,
                Action = "CategoryChanged",
                OldValue = ticket.Category?.Name,
                NewValue = category.Name,
                CreatedAt = DateTime.UtcNow
            });
            ticket.CategoryId = dto.CategoryId;
        }

        ticket.Title = dto.Title.Trim();
        ticket.Description = dto.Description.Trim();
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(ticket);
    }

    public async Task<TicketResponseDto> UpdateStatusAsync(
        int id, UpdateStatusDto dto, int currentUserId, string currentUserRole)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID #{id} was not found.");
        }

        // Employees can only close or reopen their own resolved tickets
        if (currentUserRole == "Employee")
        {
            if (ticket.CreatedByUserId != currentUserId || (dto.Status != "Closed" && dto.Status != "InProgress"))
            {
                throw new UnauthorizedAccessException("Employees may only close or reopen their own resolved tickets.");
            }
        }

        if (ticket.Status == dto.Status)
        {
            return MapToResponse(ticket);
        }

        var oldStatus = ticket.Status;
        ticket.Status = dto.Status;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (dto.Status == "Resolved")
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }
        else if (dto.Status == "InProgress" && oldStatus == "Resolved")
        {
            ticket.ResolvedAt = null; // Reopened
        }

        // Audit Trail
        _context.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticket.Id,
            ChangedByUserId = currentUserId,
            Action = "StatusChanged",
            OldValue = oldStatus,
            NewValue = dto.Status,
            CreatedAt = DateTime.UtcNow
        });

        // In-App Notification to ticket owner
        if (ticket.CreatedByUserId != currentUserId)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = ticket.CreatedByUserId,
                TicketId = ticket.Id,
                Title = "Ticket Status Updated",
                Message = $"Your ticket {ticket.TicketNumber} status has changed to '{dto.Status}'.",
                Type = "StatusChange",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return MapToResponse(ticket);
    }

    public async Task<TicketResponseDto> AssignTicketAsync(
        int id, AssignTicketDto dto, int currentUserId, string currentUserRole)
    {
        // Only Administrators can assign tickets
        if (currentUserRole != "Admin")
        {
            throw new UnauthorizedAccessException("Only Administrators have permission to assign tickets.");
        }

        var ticket = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID #{id} was not found.");
        }

        User? newAdmin = null;
        if (dto.AssignedToUserId.HasValue && dto.AssignedToUserId.Value > 0)
        {
            newAdmin = await _context.Users.FindAsync(dto.AssignedToUserId.Value);
            if (newAdmin == null || !newAdmin.IsActive || newAdmin.Role != "Admin")
            {
                throw new ArgumentException("The specified user cannot be assigned. Only active administrators can be assigned.");
            }
        }

        var oldAdminName = ticket.AssignedToUser?.Name ?? "Unassigned";
        var newAdminName = newAdmin?.Name ?? "Unassigned";

        ticket.AssignedToUserId = newAdmin?.Id;
        ticket.AssignedToUser = newAdmin;
        ticket.UpdatedAt = DateTime.UtcNow;

        // Auto move from "Open" to "InProgress" upon initial assignment
        if (ticket.Status == "Open" && newAdmin != null)
        {
            ticket.Status = "InProgress";
        }

        // Record assignment in audit history
        _context.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticket.Id,
            ChangedByUserId = currentUserId,
            Action = "Assigned",
            OldValue = oldAdminName,
            NewValue = newAdminName,
            CreatedAt = DateTime.UtcNow
        });

        // Notify newly assigned administrator
        if (newAdmin != null && newAdmin.Id != currentUserId)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = newAdmin.Id,
                TicketId = ticket.Id,
                Title = "New Ticket Assigned",
                Message = $"You have been assigned ticket {ticket.TicketNumber}: '{ticket.Title}'.",
                Type = "Assignment",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Notify submitter that an administrator has taken their ticket
        if (ticket.CreatedByUserId != currentUserId && newAdmin != null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = ticket.CreatedByUserId,
                TicketId = ticket.Id,
                Title = "Ticket Assigned",
                Message = $"{newAdmin.Name} (Admin) is now handling your ticket {ticket.TicketNumber}.",
                Type = "Assignment",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return MapToResponse(ticket);
    }

    public async Task DeleteTicketAsync(int id, int currentUserId, string currentUserRole)
    {
        // Only Administrators can permanently delete a ticket
        if (currentUserRole != "Admin")
        {
            throw new UnauthorizedAccessException("Only administrators are permitted to delete tickets.");
        }

        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket with ID #{id} was not found.");
        }

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<List<CategoryResponseDto>> GetCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ActiveTicketsCount = c.Tickets.Count(t => t.Status != "Closed" && t.Status != "Resolved")
            })
            .ToListAsync();
    }

    // --- Helper Mapping Method ---
    private static TicketResponseDto MapToResponse(Ticket ticket)
    {
        return new TicketResponseDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category?.Name ?? "Uncategorized",
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByUserName = ticket.CreatedByUser?.Name ?? "Unknown",
            CreatedByUserEmail = ticket.CreatedByUser?.Email ?? "",
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser?.Name,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            DueAt = ticket.DueAt,
            ResolvedAt = ticket.ResolvedAt,
            CommentsCount = ticket.Comments?.Count ?? 0
        };
    }
}
