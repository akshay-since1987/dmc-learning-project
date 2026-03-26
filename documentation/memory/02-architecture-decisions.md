# Architecture Decisions & Patterns

## Clean Architecture Layers
1. **Domain** — Entities (27), Enums (16), Value Objects. Zero dependencies.
2. **Application** — CQRS handlers (MediatR), FluentValidation, interfaces, DTOs. References Domain only.
3. **Infrastructure** — EF Core, JWT service, CurrentUser, external integrations. References Application.
4. **Api** — Controllers (thin, delegate to MediatR), middleware, static file serving. References all.

## Key NuGet Versions (Critical)
- **MediatR 12.4.1** — v12 uses `next()` NOT `next(cancellationToken)` in pipeline behaviours
- **EF Core 10.0.5** (SqlServer + Tools)
- **FluentValidation 11.11.0**
- **BCrypt.Net-Next 4.0.3** (in Application + Api projects)
- Infrastructure needs `<FrameworkReference Include="Microsoft.AspNetCore.App" />` for IHttpContextAccessor
- Application needs Configuration.Abstractions + Configuration.Binder 10.0.5 for IConfiguration

## CQRS Pattern
- **Commands** mutate state → return `Result<T>` (never throw for business failures)
- **Queries** are read-only → return DTOs (never expose entities)
- Pipeline: `ValidationBehaviour` → Handler (commands) | `PerformanceBehaviour` → Handler (queries)
- Every command has a matching FluentValidation `Validator` class
- Controllers are max ~5 lines per action (delegate to MediatR)

## Authentication Flow
1. User sends mobile number → `POST /api/auth/send-otp`
2. OTP sent via SMS (simulated as `123456` in dev)
3. User verifies → `POST /api/auth/verify-otp` → returns JWT access token (60 min)
4. JWT stored in `localStorage` on frontend
5. Lotus users require password + OTP (two-factor)

## Soft Deletes
- All business entities have `IsDeleted` flag
- Global query filter: `.HasQueryFilter(e => !e.IsDeleted)`
- `SoftDeleteInterceptor` sets `IsDeleted = true` instead of actual DELETE
- Lotus can see deleted records via `.IgnoreQueryFilters()`

## Audit Trail
- `AuditableEntityInterceptor` captures Create/Update timestamps
- Dedicated AuditTrail table — INSERT-only (no UPDATE/DELETE at DB level)
- Lotus/Commissioner/Auditor can read; Auditor filtered to Proposal/Workflow/Document modules

## Workflow (12-Role Amount-Based Routing)
```
Draft → Submitted → AtJE → AtTS → AtCityEngineer → AtAccountOfficer →
  if ≤3L  → AtDyCommissioner → Approved
  if ≤24L → AtDyCommissioner → AtCommissioner → Approved
  if ≤25L → AtDyCommissioner → AtCommissioner → AtStandingCommittee → Approved
  if >25L → AtDyCommissioner → AtCommissioner → AtStandingCommittee → AtCollector → Approved
```
- Push-back always returns to creator JE with mandatory reasons
- Multi-tenant via PalikaId; all entities scoped

## Bilingual (Dual-Column)
- Every user-facing text has `FieldName_En` + `FieldName_Alt` (Marathi)
- Translation API via Google Translate proxy
- Frontend: `data-i18n="key"` attributes, language switcher in header
