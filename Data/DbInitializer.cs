using ITServiceDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace ITServiceDesk.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Automatically applies pending migrations if database isn't fully updated
        await context.Database.MigrateAsync();

        // 1. Seed Ticket Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<TicketCategory>
            {
                new TicketCategory
                {
                    Name = "Hardware",
                    Description = "Physical computers, monitors, keyboards, mice, and peripherals"
                },
                new TicketCategory
                {
                    Name = "Software",
                    Description = "Operating systems, office productivity tools, installation issues"
                },
                new TicketCategory
                {
                    Name = "Network & Internet",
                    Description = "Wi-Fi connections, VPN access, LAN cables, speed issues"
                },
                new TicketCategory
                {
                    Name = "Account & Access",
                    Description = "Password resets, account lockouts, permission requests"
                },
                new TicketCategory
                {
                    Name = "Email & Communications",
                    Description = "Outlook, email delivery, Teams, mailbox storage quotas"
                },
                new TicketCategory
                {
                    Name = "Printer & Peripherals",
                    Description = "Office network printers, scanner configurations, paper jams"
                },
                new TicketCategory
                {
                    Name = "Other",
                    Description = "General technical inquiries and miscellaneous IT requests"
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // 2. Convert any legacy 'Agent' users in the database to 'Admin'
        var legacyAgents = await context.Users.Where(u => u.Role == "Agent").ToListAsync();
        if (legacyAgents.Any())
        {
            foreach (var agent in legacyAgents)
            {
                agent.Role = "Admin";
            }
            await context.SaveChangesAsync();
        }

        // 3. Seed Default Users (Admin and Employee only)
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Name = "System Administrator",
                Email = "admin@servicedesk.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var employeeUser = new User
            {
                Name = "Emily Employee",
                Email = "employee@servicedesk.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123!"),
                Role = "Employee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(adminUser, employeeUser);
            await context.SaveChangesAsync();

            // 4. Seed Sample Initial Ticket for Demonstration
            var hardwareCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Hardware");
            if (hardwareCategory != null)
            {
                var sampleTicket = new Ticket
                {
                    TicketNumber = $"TCK-{DateTime.UtcNow:yyyyMM}-0001",
                    Title = "External monitor not displaying output",
                    Description = "My dual monitor setup stopped receiving HDMI signal after this morning's Windows update.",
                    CategoryId = hardwareCategory.Id,
                    Status = "InProgress",
                    Priority = "High",
                    CreatedByUserId = employeeUser.Id,
                    AssignedToUserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    DueAt = DateTime.UtcNow.AddHours(21)
                };

                await context.Tickets.AddAsync(sampleTicket);
                await context.SaveChangesAsync();

                // Add Ticket History record
                var historyRecord = new TicketHistory
                {
                    TicketId = sampleTicket.Id,
                    ChangedByUserId = adminUser.Id,
                    Action = "Assigned",
                    OldValue = "Unassigned",
                    NewValue = adminUser.Name,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                };

                // Add initial comment
                var sampleComment = new Comment
                {
                    TicketId = sampleTicket.Id,
                    UserId = adminUser.Id,
                    Content = "Hello Emily, I've taken ownership of this ticket. Have you tried unplugging and reseating the HDMI cable on the dock?",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                };

                // Add notification for the employee
                var sampleNotification = new Notification
                {
                    UserId = employeeUser.Id,
                    TicketId = sampleTicket.Id,
                    Title = "Ticket Update",
                    Message = $"{adminUser.Name} (Admin) has been assigned to your ticket TCK-{DateTime.UtcNow:yyyyMM}-0001",
                    Type = "Assignment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                };

                await context.TicketHistories.AddAsync(historyRecord);
                await context.Comments.AddAsync(sampleComment);
                await context.Notifications.AddAsync(sampleNotification);
                await context.SaveChangesAsync();
            }
        }
    }
}
