# Architecture Documentation

## 1. Architectural Style: Decoupled N-Tier Client-Server

The application follows a **Decoupled Client-Server Architecture** featuring an ASP.NET Core RESTful Web API backend and an independent React Single Page Application (SPA) frontend.

```
+-------------------------------------------------------+
|                    React 18 SPA                       |
|   (Vite + Tailwind CSS + React Router + Axios)        |
+---------------------------+---------------------------+
                            | HTTP / JSON (JWT Bearer)
                            v
+-------------------------------------------------------+
|              ASP.NET Core Web API (Kestrel)           |
|                                                       |
|  [Middleware Pipeline]                                |
|    -> ExceptionMiddleware (Global error format)       |
|    -> CORS Policy (Allow frontend origins)            |
|    -> Authentication Middleware (JWT Validation)      |
|    -> Authorization Middleware (Role enforcement)     |
|                                                       |
|  [Controllers Layer]                                  |
|    -> AuthController, TicketsController, etc.         |
|                                                       |
|  [Service Layer (Business Rules & SLA Engine)]        |
|    -> AuthService, TicketService, AttachmentService   |
|                                                       |
|  [Data Access Layer (EF Core 11)]                     |
|    -> AppDbContext, LINQ Queries, Fluent API Config   |
+---------------------------+---------------------------+
                            | SQLite Connection
                            v
+-------------------------------------------------------+
|                     app.db (SQLite)                   |
|  (Users, Categories, Tickets, Comments, Histories)   |
+-------------------------------------------------------+
```

---

## 2. Key Architecture Design Patterns

### A. Dependency Injection (DI)
All services (`IAuthService`, `ITicketService`, `ICommentService`, `INotificationService`, `IDashboardService`, `IUserService`, `IAttachmentService`) are registered in the built-in ASP.NET Core IoC container with a **Scoped** lifetime (`builder.Services.AddScoped<...>()`). 

This ensures that each HTTP request gets its own instance of services and DbContext, preventing race conditions while conserving memory.

### B. Data Transfer Objects (DTO) Pattern
Database entities (`Ticket`, `User`, `Comment`) are never returned directly to API responses:
- **Prevents Circular Reference Crashes**: EF Core navigation properties (`Ticket.Comments` pointing to `Comment.Ticket`) cause infinite JSON recursion loops if serialized directly.
- **Prevents Over-Posting Vulnerabilities**: Users cannot tamper with protected database fields (such as `TicketNumber` or `PasswordHash`).
- **Encapsulation**: Exposes only fields needed by the specific UI view.

### C. Repository & Unit of Work (via EF Core DbContext)
Entity Framework Core's `AppDbContext` inherently acts as both a **Unit of Work** and **Repository**. Changes across multiple tables (e.g., creating a ticket and logging an audit history entry) are executed within a single transactional `SaveChangesAsync()` call.

### D. Single Responsibility Controllers
Controllers are kept extremely thin. They simply:
1. Extract current user identity claims (`User.FindFirst(ClaimTypes.NameIdentifier)`).
2. Validate incoming request DTOs.
3. Delegate business processing to the corresponding service.
4. Return appropriate HTTP status codes (200 OK, 201 Created, 204 NoContent, 400 BadRequest, 401 Unauthorized, 403 Forbidden, 404 NotFound).

---

## 3. Security Architecture

1. **Password Hashing**: Utilizes `BCrypt.Net-Next` with automatic random cryptographic salting (work factor 11).
2. **Stateless Authentication**: Uses RFC 7519 standard JSON Web Tokens (JWT) signed with HMAC-SHA256 (`HmacSha256`).
3. **Role Authorization**: Enforced on the backend through `[Authorize(Roles = "Admin,Agent")]`. The frontend also guards routes, but all backend endpoints independently reject unauthorized requests.
4. **Path Traversal & Safe Uploads**:
   - Uploaded files are renamed using GUIDs (`Guid.NewGuid().ToString("N") + extension`).
   - Extensions are strictly whitelisted.
   - Files are stored in an isolated `Uploads/` directory and streamed securely.

