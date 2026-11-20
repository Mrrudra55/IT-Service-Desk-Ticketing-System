# RESTful API Reference Documentation

Base URL: `http://localhost:5276/api` (or proxied via Vite at `/api`)
Interactive Swagger UI: `http://localhost:5276/swagger`

All protected endpoints require the HTTP Header:
```
Authorization: Bearer <your_jwt_token>
```

---

## 1. Authentication Endpoints (`/api/auth`)

### `POST /api/auth/register`
Creates a new user account.
- **Access**: Public
- **Request Body**:
  ```json
  {
    "name": "Jane Doe",
    "email": "jane@company.local",
    "password": "Password123!",
    "role": "Employee"
  }
  ```
- **Response**: `200 OK` with `AuthResponseDto` (`token`, `expiresAt`, `user`).

### `POST /api/auth/login`
Authenticates a user and returns a signed JWT token.
- **Access**: Public
- **Request Body**:
  ```json
  {
    "email": "admin@servicedesk.local",
    "password": "Admin@123!"
  }
  ```
- **Response**: `200 OK` with `AuthResponseDto`.

### `GET /api/auth/me`
Retrieves currently logged-in user profile from the token claims.
- **Access**: Authenticated (`[Authorize]`)
- **Response**: `200 OK` with `UserProfileDto`.

---

## 2. Tickets Endpoints (`/api/tickets`)

### `GET /api/tickets`
Returns a paginated, filtered list of tickets.
- **Query Parameters**:
  - `page`: Page number (default: 1)
  - `pageSize`: Items per page (default: 10, max: 50)
  - `search`: Keyword matching number, title, or description
  - `categoryId`: Filter by category ID
  - `status`: `Open`, `InProgress`, `OnHold`, `Resolved`, `Closed`
  - `priority`: `Low`, `Medium`, `High`, `Critical`
  - `unassignedOnly`: `true` to fetch triage queue
- **Access**:
  - **Employee**: Strictly returns tickets created by them.
  - **Agent / Admin**: Returns all matching tickets.

### `GET /api/tickets/{id}`
Returns full ticket details including comment thread, attachments, and audit timeline.
- **Access**: Ticket owner, Agent, or Admin.

### `POST /api/tickets`
Submits a new ticket. Generates sequential number and calculates SLA due date.
- **Request Body**:
  ```json
  {
    "title": "Dual monitor not receiving signal",
    "description": "HDMI port on dock stopped outputting display after reboot.",
    "categoryId": 1,
    "priority": "High"
  }
  ```
- **Response**: `201 Created` with `TicketResponseDto`.

### `PUT /api/tickets/{id}`
Updates ticket details (Title, Description, Category, Priority).
- **Access**: Ticket owner (if still Open), Agent, or Admin.

### `PATCH /api/tickets/{id}/status`
Transitions ticket workflow status.
- **Request Body**: `{"status": "Resolved"}`
- **Access**: Agent or Admin (or Employee closing/reopening own ticket).

### `PATCH /api/tickets/{id}/assign`
Assigns an IT Agent to the ticket.
- **Request Body**: `{"assignedToUserId": 2}`
- **Access**: Agent or Admin only.

### `DELETE /api/tickets/{id}`
Permanently deletes a ticket and its cascading data.
- **Access**: Admin only.

---

## 3. Comments Endpoints (`/api/tickets/{ticketId}/comments`)

### `GET /api/tickets/{ticketId}/comments`
Retrieves chronological discussion thread.

### `POST /api/tickets/{ticketId}/comments`
Posts a new comment to the ticket thread.
- **Request Body**:
  ```json
  {
    "content": "Access point firmware updated. Please test again."
  }
  ```

---

## 4. Attachments Endpoints

### `POST /api/tickets/{ticketId}/attachments`
Uploads a file attachment (multipart/form-data).
- **Form Data**: `file` (binary, max 10MB)

### `GET /api/attachments/{id}/download`
Streams and downloads the physical file.

### `DELETE /api/attachments/{id}`
Deletes an attachment (Uploader or Admin only).

---

## 5. Dashboards & Reports Endpoints

### `GET /api/dashboard/employee`
Returns personal ticket counts and recent submissions.

### `GET /api/dashboard/agent`
Returns assigned tickets count, unassigned queue, overdue alerts, and workload.

### `GET /api/dashboard/admin`
Returns organization-wide metrics, category distributions, status breakdowns, and agent performance.

### `GET /api/reports/export`
Exports all tickets and SLA performance metrics as an Excel-compatible CSV file.

---

## 6. User Administration Endpoints (`/api/users`)

### `GET /api/users`
Paginated user directory with search and role filters. (Admin only)

### `GET /api/users/agents`
Returns active IT agents for assignment dropdowns. (Agent & Admin)

### `PATCH /api/users/{id}/role`
Changes user role (`Employee`, `Agent`, `Admin`). (Admin only)

### `PATCH /api/users/{id}/active`
Enables or disables a user account. (Admin only)

