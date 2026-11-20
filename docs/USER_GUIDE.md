# User & Administrator Guide

## 1. Getting Started

1. Open your browser and navigate to **`http://localhost:5173`**.
2. If not already signed in, you will be directed to the **Sign In** screen.
3. For demonstration, use the convenient **1-Click Quick Demo** buttons at the bottom of the login form to immediately fill:
   - **Admin** (`admin@servicedesk.local`)
   - **Agent** (`agent@servicedesk.local`)
   - **Employee** (`employee@servicedesk.local`)

---

## 2. Employee Workflow (Reporting & Tracking Issues)

### Submitting a Support Ticket
1. Click **"Report New Problem"** from your Dashboard or **"Create Ticket"** in the sidebar.
2. Select an appropriate category (e.g., *Hardware*, *Software*, *Network*).
3. Choose the urgency level (*Low*, *Medium*, *High*, or *Critical*).
4. Enter a descriptive title and detailed explanation of the issue.
5. Click **"Submit Support Request"**.
6. The system will assign a unique identifier (e.g., `TCK-202611-0001`) and compute the SLA resolution target.

### Uploading Screenshots & Diagnostics
1. Open your ticket from the **Support Tickets** list.
2. Click on the **"Attachments"** tab.
3. Click **"Choose File"** to select a screenshot, photo, or error log file (up to 10MB).
4. The file will upload and become accessible to the support technician immediately.

### Communicating with Support
1. On your ticket detail page, type a message into the **Discussion** box and click **"Post Reply"**.
2. Whenever an IT technician replies or changes your ticket status, the notification bell in the top right will illuminate with an unread badge.

---

## 3. IT Agent Workflow (Resolving Technical Inquiries)

### Triage & Claiming Unassigned Tickets
1. Go to your **Agent Dashboard**.
2. View the **Unassigned Triage Queue** table. Tickets with *Critical* and *High* urgency are automatically sorted to the top.
3. Click **"Investigate"** on a ticket.
4. In the right panel, click **"Assign to Myself"** (this automatically moves the status to *In Progress* and registers your ownership in the audit timeline).

### Working with Tickets
- **Update Workflow State**: Use the buttons on the right to transition the ticket between *In Progress*, *On Hold*, and *Resolved*.
- **Post Diagnostics**: Reply in the discussion thread with troubleshooting instructions or status updates for the employee.
- **SLA Tracking**: Monitor the SLA deadline in the right panel. If a ticket exceeds its deadline, an attention-grabbing red alert banner appears across the top.

---

## 4. Administrator Guide (System Governance)

### Monitoring Performance
- Open the **Admin Command Center** dashboard.
- Review total active tickets, critical issues, and average resolution time.
- Check the **Category Distribution** bar chart to identify which department generates the most inquiries.
- Check the **Support Team Workload** table to ensure tickets are distributed evenly across technicians.

### Exporting Reports
- Click the **"Export CSV"** button on the top right of the Admin Dashboard.
- A comprehensive spreadsheet (`.csv`) will download with full details on every ticket, resolution times in hours, and SLA compliance.

### Managing Users & Roles
- Click **"User Management"** in the sidebar.
- Search for any employee or filter by role.
- **Promote to Agent**: Change an employee's role to *Agent* using the inline dropdown.
- **Deactivate Accounts**: Click the **"Deactivate"** button to immediately disable access without deleting ticket history.

