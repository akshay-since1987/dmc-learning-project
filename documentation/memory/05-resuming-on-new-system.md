# Resuming Work on a New System

## Prerequisites
1. **.NET SDK 10.0.102+** — `dotnet --version` to verify
2. **SQL Server Express** — instance `.\SQLEXPRESS` with Windows Auth
3. **Node.js** (LTS) — for future npm build tasks
4. **Git** — for repository management
5. **VS Code** with GitHub Copilot extension

## Quick Start
```bash
# 1. Clone the repo
git clone https://github.com/akshay-since1987/dmc-learning-project.git ProposalManagement
cd ProposalManagement

# 2. Create the database
# Open SQL Server Management Studio or sqlcmd and run:
# CREATE DATABASE [dmc-v2-ProposalMgmt]

# 3. Configure secrets (see SECRETS.md at repo root)
# Copy appsettings template and fill in values

# 4. Run EF Core migrations / let the app create tables
cd v2/backend/src/ProposalManagement.Api
dotnet run --urls http://localhost:5108

# 5. Seed test users (if not auto-seeded)
# Run v2/database/*.sql scripts against dmc-v2-ProposalMgmt

# 6. Access the app
# Open http://localhost:5108 in browser
# Login: Mobile 9999999999, OTP 123456 (Lotus admin)
```

## Restoring the Copilot Agent
The Vidur agent definition is at `.github/agents/Vidur.agent.md`.
When using GitHub Copilot in VS Code:
1. Open the ProposalManagement folder as workspace
2. Copilot automatically loads the agent from `.github/agents/`
3. Use `@Vidur` in Copilot Chat to engage the project-specific agent

## Restoring Copilot Memory
After cloning, create these Copilot memory files to restore context:
- `/memories/repo/proposal-management.md` — key facts, test users, connection info
- `/memories/preferences.md` — working style preferences

The content for these files is documented in the existing `documentation/memory/` folder.

## Key Files to Read First
1. `documentation/memory/04-implementation-status.md` — what's done, what's remaining
2. `documentation/memory/02-architecture-decisions.md` — patterns and conventions
3. `documentation/memory/03-working-style.md` — how we work together
4. `SECRETS.md` — environment configuration guide
5. `.github/agents/Vidur.agent.md` — full agent system prompt (2000+ lines)

## API Endpoints Reference
See `documentation/06-api-endpoints.md` for the full list of 57+ endpoints.

## Database Schema Reference
See `documentation/03-database-schema.md` for all 26 tables.

## What to Work on Next
Refer to "Known Remaining Work" in `documentation/memory/04-implementation-status.md`.
Priority order:
1. Admin Masters CRUD page (backend done, need frontend JS page)
2. i18n JSON files (en.json, mr.json)
3. Unit tests
4. PDF generation with QuestPDF
5. CI/CD GitHub Actions
