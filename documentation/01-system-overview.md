# System Overview — Proposal Management System V2

## Product

**धुळे महानगरपालिका — प्रशासकीय मान्यतेसाठी कार्यालयीन टिपणी**
(Administrative Approval Office Note — Dhule Municipal Corporation)

## Purpose

A web-based proposal management system initially built for the Dhule Municipal
Corporation that digitises the end-to-end lifecycle of public works proposals —
from creation by a Junior Engineer through field inspection, cost estimation,
technical sanction, administrative approval compilation, budget verification, and
multi-stage authority-based approval with digital signatures.

> **Multi-Tenant**: The system is designed to support multiple Municipal
> Corporations (महानगरपालिका) or Nagar Palikas (नगरपालिका) within a single
> deployment. Every scoped entity carries a `PalikaId` foreign key to the
> `MunicipalCorporations` table. Dhule Municipal Corporation is the initial
> tenant.

## Key Capabilities

1. **6-Tab Multi-Part Form** — each tab is a specialised section of the proposal
   with independent ownership that transfers between roles as the proposal
   progresses.
2. **Ownership-Based Editing** — only the current form owner can edit; all others
   see read-only. Ownership transfers automatically as workflow advances.
3. **Field Visit Tracking** — multiple inspection records per proposal with GPS,
   photos, measurements, and recommendations.
4. **Estimate & Technical Sanction** — estimate upload with engineering approval
   chain; technical sanction as separate tracked step.
5. **PRAMA (प्रशासकीय मान्यता)** — auto-compiled administrative approval note
   pulling data from all prior tabs.
6. **Budget Verification** — pre-loaded budget heads with automatic balance
   calculation. Proposals with insufficient funds are **parked** (not declined).
7. **Amount-Based Approval Routing** — the approval chain length is determined
   by estimated cost (0–3L through 25L+).
8. **Mandatory Disclaimers** — every approver must accept a role-specific
   Marathi legal disclaimer before signing.
9. **PDF Generation** — per-tab PDFs on sign/save; consolidated signed PDF
   generated on each ownership transfer; master combined PDF at final approval.
10. **In-App Notifications** — real-time bell notifications on every ownership
    change, push-back, approval, assignment, and park/unpark event.
11. **Audit Trail** — immutable, append-only log of every action.
12. **Lotus Super-Admin** — unrestricted CRUD on all entities.

## Tech Stack

| Layer             | Technology                                              |
| ----------------- | ------------------------------------------------------- |
| Backend API       | ASP.NET Core 8 Web API (minimal hosting) + static files |
| Architecture      | Clean Architecture + CQRS (MediatR)                     |
| ORM               | Entity Framework Core 8 (Code-First, SQL Server)        |
| Database          | SQL Server 2022 Express                                 |
| Auth              | OTP via SMS gateway → JWT (access + refresh)            |
| Signatures        | PNG upload (DSC planned for future)                     |
| Frontend          | Plain HTML5 + CSS3 + Vanilla JS (ES2022 modules)       |
| UI Framework      | Bootstrap 5.3 (CSS + JS bundle)                         |
| Accessibility     | WCAG 2.1 AA mandatory                                   |
| i18n              | Dual-column (_En/_Alt) + JSON label files               |
| PDF Generation    | QuestPDF / PdfSharp                                     |
| Notifications     | In-app (SignalR or polling), Email & SMS (future)       |
| Containerisation  | Docker + docker-compose                                 |
| CI/CD             | GitHub Actions                                          |

## Database

- **Name**: `dmc-ProposalManagement` (on `.\SQLEXPRESS`)
- **Auth**: Windows Integrated Security
- **Connection**: `Server=.\SQLEXPRESS;Database=dmc-ProposalManagement;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;`

## Solution Structure

```
ProposalManagement/
├── backend/                          # All .NET projects
│   ├── ProposalManagement.sln
│   └── src/
│       ├── ProposalManagement.Domain/
│       ├── ProposalManagement.Application/
│       ├── ProposalManagement.Infrastructure/
│       └── ProposalManagement.Api/
│           └── wwwroot/              # Built frontend files
├── src/                              # Frontend source (HTML/CSS/JS)
│   ├── pages/
│   ├── css/
│   ├── js/
│   ├── i18n/
│   └── vendor/
├── database/                         # SQL scripts
├── documentation/                    # THIS folder
└── docker/
```

## Bilingual Support

Every user-facing text field has dual columns: `FieldName_En` + `FieldName_Mr`
(English + Marathi). Static UI labels use JSON i18n files (`en.json`, `mr.json`).
