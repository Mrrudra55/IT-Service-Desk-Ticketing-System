# IT Service Desk Ticketing System

A full-stack, enterprise-grade IT Service Desk and Helpdesk application developed with **ASP.NET Core Web API**, **Entity Framework Core**, **SQLite**, and a modern **React + Vite + Tailwind CSS** frontend.

---

## 🚀 Key Features

### 1. Two-Role Access Architecture (Admin & Employee)
- **Employee Portal**: Report technical problems, monitor ticket progress, reply in comment threads, attach diagnostic files/screenshots, and confirm resolution or reopen tickets.
- **Administrator Command Center**:
  - Full system oversight and executive control.
  - Triage unassigned queue and assign tickets to administrator specialists.
  - Update workflow lifecycle states (Open → In Progress → Resolved → Closed).
  - Manage users (promote, activate/deactivate accounts).
  - Export comprehensive ticket reports to Excel-compatible CSV.

### 2. Distinct & Secure Registration Flows
- **Employee Registration** (`/register`):
  - Dedicated public self-service registration strictly granting the `Employee` role.
- **Administrator Registration** (`/register-admin`):
  - Separate, protected administrative registration flow.
  - Requires the organizational **Admin Security Key** (`ADMIN_SECRET_2026` configured in backend `appsettings.json`) to prevent unauthorized role escalation.

### 3. SLA & Ticket Lifecycle Engine
- Automatically generates sequential, human-readable ticket numbers (e.g., `TCK-202611-0001`).
- Dynamically calculates SLA resolution deadlines based on urgency:
  - **Critical**: 4 hours
  - **High**: 24 hours
  - **Medium**: 48 hours
  - **Low**: 72 hours
- Real-time SLA breach/overdue detection.

### 4. Complete Audit Timeline & Communication
- Every action (Creation, Priority Change, Category Change, Assignment, Status Transition) is automatically recorded in an immutable `TicketHistories` audit log.
- Full two-way discussion threads and file attachment management between employees and administrators.

---

## 🛠️ Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Backend** | C#, ASP.NET Core Web API (.NET 8/9/11), Entity Framework Core, SQLite |
| **Security** | JWT (JSON Web Tokens) with HMAC-SHA256, BCrypt Password Hashing, Role-Based Authorization, Admin Secret Passkey Verification |
| **API Docs** | Swagger UI / OpenAPI 3.0 |
| **Frontend** | React 18, Vite, Tailwind CSS, React Router v6, Axios, Lucide Icons |

---

## 🏃 Quick Start Guide

### 🚀 Option A: Run Both in 1 Command (Recommended)

In your PowerShell terminal:
```powershell
.\run.ps1
```
*(Or in Command Prompt / File Explorer, double-click **`start.bat`**!)*

This automatically launches both the **Backend API** and the **React Frontend** in their own windows simultaneously.

---

### Option B: Run Manually in Separate Terminals

#### 1. Start the Backend API (Terminal 1)
```bash
dotnet run
```
- API starts at: `http://localhost:5276`
- Interactive Swagger UI: `http://localhost:5276/swagger`

#### 2. Start the React Frontend (Terminal 2)
```bash
cd ITServiceDesk.Client
npm run dev
```
- Frontend application opens at: **`http://localhost:5173`**

---

## 🔑 Default Pre-Seeded Accounts

| Role | Email | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin@servicedesk.local` | `Admin@123!` | Full control, triage, assignments, user admin, reports |
| **Employee** | `employee@servicedesk.local` | `Employee@123!` | Create tickets, comment, attachments, close/reopen own |

*(Tip: On the login screen, you can also use the 1-click **Quick Demo** buttons to instantly fill credentials for Admin or Employee!)*

### Administrator Registration Secret Key
- **Default Key**: `ADMIN_SECRET_2026` (Configured in `appsettings.json`)
