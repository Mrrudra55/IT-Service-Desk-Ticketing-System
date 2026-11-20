# IT Service Desk Ticketing System — Project Overview

## 1. Executive Summary
The **IT Service Desk Ticketing System** is a full-stack, enterprise-grade helpdesk platform designed to centralize and automate internal technical support operations within an organization. It bridges employees encountering hardware, software, network, or account problems with specialized IT support personnel.

The platform provides:
- Transparent issue reporting and tracking for employees.
- Prioritized queue management and triage for IT technicians.
- Automated SLA (Service Level Agreement) deadline calculations.
- Comprehensive activity audit timelines and two-way communication threads.
- Real-time managerial dashboards and Excel-compatible CSV reporting.

---

## 2. Business Problem & Solution

### The Challenge
Without a centralized helpdesk, support inquiries sent through scattered emails, chat messages, and verbal requests suffer from:
- Lost or forgotten requests.
- Lack of accountability and tracking.
- Zero visibility into SLA resolution times.
- No historical record or audit trail of troubleshooting steps taken.

### The Solution
A web-based service desk with strict role separation:
- **Employee**: Can report issues, track real-time status, reply to technician questions, and approve resolution.
- **IT Agent**: Claims unassigned tickets, investigates issues, updates statuses, and adheres to SLA targets.
- **Administrator**: Oversees organization performance, configures problem categories, balances agent workload, and manages user credentials and roles.

---

## 3. Technology Stack Summary

| Layer | Technology | Version | Purpose |
| :--- | :--- | :--- | :--- |
| **Backend Framework** | ASP.NET Core Web API | .NET 11 | RESTful service endpoints |
| **ORM / Data Access** | Entity Framework Core | 11.0.0 | Code-First database mapping & migrations |
| **Database** | SQLite | 3.x | Lightweight, zero-configuration relational database |
| **Authentication** | JWT (JSON Web Tokens) | RFC 7519 | Stateless Bearer authentication |
| **Password Security** | BCrypt.Net-Next | 4.2.0 | Cryptographic salted password hashing |
| **Frontend Framework** | React + Vite | 18.3.1 / 6.x | Single Page Application (SPA) |
| **Styling** | Tailwind CSS | 3.4.17 | Modern utility-first responsive styling |
| **HTTP Client** | Axios | 1.7.9 | Request & response interceptors |
| **Icons** | Lucide React | 0.475.0 | Clean SVG icon library |
| **API Docs** | Swagger / OpenAPI | 3.0 | Interactive API testing sandbox |

---

## 4. Key Performance Metrics
- **Zero Plaintext Passwords**: All credentials stored as BCrypt hashes.
- **Sub-Second API Response Times**: Optimized SQLite queries with foreign key indexing.
- **Automated Seeding**: Instantly ready-to-use with pre-seeded Admin, Agent, and Employee test accounts.
- **100% Responsive UI**: Seamlessly usable across mobile, tablet, and desktop viewports.

