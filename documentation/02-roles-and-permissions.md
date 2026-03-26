# Roles & Permissions

## All Roles

| #  | Role                 | Code               | Auth Method    | Scope                                            |
| -- | -------------------- | ------------------- | -------------- | ------------------------------------------------ |
| 1  | Junior Engineer      | `JE`                | OTP            | Creates proposals, field visits, estimates, PRAMA, budget |
| 2  | Technical Sanctioner | `TS`                | OTP            | Provides technical sanction (Tab 4)              |
| 3  | Assistant Engineer   | `AE`                | OTP            | Estimate approval tier                           |
| 4  | Sub Engineer         | `SE`                | OTP            | Estimate approval tier                           |
| 5  | City Engineer        | `CityEngineer`      | OTP            | Estimate approval + signs TS order + formal chain |
| 6  | Account Officer      | `AccountOfficer`    | OTP            | Budget verification in formal approval chain     |
| 7  | Deputy Commissioner  | `DyCommissioner`    | OTP            | Approval authority (all proposals)               |
| 8  | Commissioner         | `Commissioner`      | OTP            | Approval authority (3L+)                         |
| 9  | Standing Committee   | `StandingCommittee` | OTP            | Approval authority (24–25L)                      |
| 10 | Collector            | `Collector`         | OTP            | Approval authority (25L+)                        |
| 11 | Auditor              | `Auditor`           | OTP            | Read-only oversight on all proposals + audit     |
| 12 | Lotus (Super Admin)  | `Lotus`             | OTP + Password | Unrestricted CRUD on all entities                |

> **Multiple officers per role**: The system supports multiple users with the same
> role (e.g., 3 JEs, 2 Deputy Commissioners). Approval dropdowns allow selecting
> the specific officer.

---

## Permission Matrix — 6-Tab Form

### Tab Ownership & Editability

| Tab | Primary Owner | Can Also Edit | Everyone Else |
| --- | ------------- | ------------- | ------------- |
| 1. Proposal | Creator JE | — | View-only |
| 2. Field Visit | Assigned JE (self or another from dept) | — | View-only (including creator JE if different) |
| 3. Estimate | Creator JE (prepares) → AE/SE/CE (approves) | AE/SE/CE can return with query | View-only |
| 4. TS | TS role (fills) → AE/SE/CE (signs) | — | View-only |
| 5. PRAMA | Creator JE | — | View-only |
| 6. Budget | Creator JE | — | View-only |

### Tab Visibility Rules

- **All 6 tabs are always visible** in the tab bar for everyone in the loop.
- **Locked tabs** show data read-only with a lock icon.
- **The current owner's tab** is editable (unlocked).
- **Future tabs** (not yet reached in workflow) are visible but show "Not yet
  started" placeholder content.

---

## Formal Approval Chain — Read/Write Access

| Role | See All Proposals? | Can Approve? | Can Push Back? | Push Back Target |
| ---- | ------------------ | ------------ | -------------- | ---------------- |
| City Engineer | Only proposals at their stage | Yes (with disclaimer) | Yes | Always to JE |
| Account Officer | Only proposals at their stage | Yes (with disclaimer) | Yes | Always to JE |
| Dy Commissioner | Only proposals at their stage | Yes (with disclaimer) | Yes | Always to JE |
| Commissioner | Only proposals at their stage (3L+) | Yes (with disclaimer) | Yes | Always to JE |
| Standing Committee | Only proposals at their stage (24-25L) | Yes (with disclaimer) | Yes | Always to JE |
| Collector | Only proposals at their stage (25L+) | Yes (with disclaimer) | Yes | Always to JE |
| Auditor | **ALL proposals** (read-only) | No | No | — |
| Lotus | **ALL proposals** (full CRUD) | Via admin override | Via admin override | Any |

---

## Estimate Approval (Pre-Submission) — Permissions

| Role | What They Do |
| ---- | ------------ |
| JE | Uploads estimate PDF, signs (PNG), sends to AE/SE/City Engineer |
| AE | Reviews estimate; can approve (with disclaimer) or return with query to JE |
| SE | Same as AE |
| City Engineer | Same as AE; additionally signs both Estimate + TS Order after TS is done |

---

## Data Access Rules Summary

1. **Creator JE** always retains read access to all 6 tabs of their proposal,
   even after ownership transfers.
2. **Assigned JE** (for field visit) can only edit Tab 2 of the specific
   proposal assigned to them.
3. **Approvers** see all 6 tabs read-only; their own approval section (disclaimer
   + opinion + signature) is editable only when it's their turn.
4. **Auditor** has global read-only access to all proposals and proposal-related
   audit trail entries.
5. **Lotus** bypasses all access rules.
6. **Push-back** always returns to creator JE regardless of which stage pushes back.
7. **Parked proposals**: owner at time of parking retains read access; Account
   Officer can manually unpark when funds are available.
