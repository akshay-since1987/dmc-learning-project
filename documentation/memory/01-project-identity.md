# Project Identity — Proposal Management System

## Product
**धुळे महानगरपालिका — प्रशासकीय मान्यतेसाठी कार्यालयीन टिपणी**
(Administrative Approval Office Note for Dhule Municipal Corporation)

## Purpose
OTP-based government proposal system with multi-stage DSC-signed approval workflow,
push-back with mandatory reasons, full audit trail, bilingual support (English + Marathi),
and a super-admin Lotus Module.

## Repository
- **GitHub Remote**: `https://github.com/akshay-since1987/dmc-learning-project.git`
- **Branch**: `main`
- **Project Root**: `C:\Projects\ProposalManagement`

## Tech Stack Summary
| Layer              | Technology                                           |
| ------------------ | ---------------------------------------------------- |
| Backend API        | ASP.NET Core 10 (.NET 10.0.102) — minimal hosting    |
| Architecture       | Clean Architecture + CQRS (MediatR 12.4.1)          |
| ORM                | Entity Framework Core 10.0.5 (Code-First, SQL Server)|
| Database           | SQL Server Express (local dev) / SQL Server 2022     |
| Auth               | OTP (simulated in dev, SMS in prod) → JWT            |
| Frontend           | Plain HTML5 + CSS3 + vanilla JS (ES2022 modules)    |
| UI Framework       | Bootstrap 5.3.3 (CDN)                                |
| Build Tooling      | npm (for future minification)                        |
| Containerization   | Docker + docker-compose                              |

## Folder Structure
```
ProposalManagement/
├── v2/backend/                # Active .NET Clean Architecture code
│   └── src/
│       ├── ProposalManagement.Domain/
│       ├── ProposalManagement.Application/
│       ├── ProposalManagement.Infrastructure/
│       └── ProposalManagement.Api/
│           └── wwwroot/       # Frontend SPA (static files)
├── ___backend/                # V1 reference/backup code
├── documentation/             # Specs, guides, memory
├── database/                  # SQL migration/seed scripts
├── src/                       # Legacy V1 frontend source
├── .github/agents/            # Vidur AI agent definition
├── Dockerfile                 # Multi-stage Docker build
└── docker-compose.yml         # SQL Server + App services
```
