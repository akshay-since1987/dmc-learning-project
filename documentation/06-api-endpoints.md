# API Endpoints

## Base URL

```
http://localhost:5108/api
```

## Authentication

All endpoints except `POST /api/auth/send-otp` and `POST /api/auth/verify-otp`
require a valid JWT Bearer token in the `Authorization` header.

---

## 1. Auth

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/send-otp` | Send OTP to mobile | Public |
| POST | `/api/auth/verify-otp` | Verify OTP (+ password for Lotus) → JWT | Public |
| POST | `/api/auth/refresh-token` | Refresh expired access token | Token |
| GET | `/api/auth/me` | Get current user profile | Token |
| PUT | `/api/auth/me` | Update current user profile | Token |
| POST | `/api/auth/me/signature` | Upload PNG signature | Token |

---

## 2. Proposals (Tab 1)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/proposals` | List proposals (filtered by role/access) | JE, Auditor, Commissioner, Lotus |
| GET | `/api/proposals/my` | List JE's own proposals (paginated) | JE |
| GET | `/api/proposals/{id}` | Get full proposal (all tabs data) | Owner, Creator, Approvers in chain, Auditor, Lotus |
| POST | `/api/proposals` | Create new proposal (Tab 1) → auto-generate number | JE |
| PUT | `/api/proposals/{id}` | Update proposal Tab 1 data | Owner (JE in Draft/PushedBack) |
| DELETE | `/api/proposals/{id}` | Soft-delete draft proposal | Owner (JE in Draft only) |
| GET | `/api/proposals/{id}/pdfs` | List all generated PDFs for this proposal | Anyone in loop |
| GET | `/api/proposals/{id}/pdfs/{pdfId}/download` | Download a specific PDF | Anyone in loop |
| GET | `/api/proposals/{id}/documents` | List uploaded documents | Anyone in loop |
| POST | `/api/proposals/{id}/documents` | Upload a document (any tab) | Current owner |
| DELETE | `/api/proposals/{id}/documents/{docId}` | Remove uploaded document | Current owner |
| GET | `/api/proposals/stats` | Dashboard statistics | Token |

---

## 3. Field Visits (Tab 2)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/proposals/{id}/field-visits` | List all field visits for a proposal | Anyone in loop |
| GET | `/api/proposals/{id}/field-visits/{visitId}` | Get single visit detail | Anyone in loop |
| POST | `/api/proposals/{id}/field-visits` | Create & assign new field visit | Creator JE |
| PUT | `/api/proposals/{id}/field-visits/{visitId}` | Update field visit data | Assigned JE |
| POST | `/api/proposals/{id}/field-visits/{visitId}/complete` | Sign & save (complete visit) | Assigned JE |
| POST | `/api/proposals/{id}/field-visits/{visitId}/photos` | Upload site photos | Assigned JE |
| DELETE | `/api/proposals/{id}/field-visits/{visitId}/photos/{photoId}` | Remove photo | Assigned JE |
| GET | `/api/field-visits/assigned` | List visits assigned to current JE | JE |

---

## 4. Estimates (Tab 3)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/proposals/{id}/estimate` | Get estimate details | Anyone in loop |
| POST | `/api/proposals/{id}/estimate` | Create/update estimate (upload PDF, sign) | Creator JE |
| POST | `/api/proposals/{id}/estimate/send-for-approval` | Send to AE/SE/CE | Creator JE |
| POST | `/api/proposals/{id}/estimate/approve` | Approve estimate (disclaimer + sign) | AE, SE, CityEngineer |
| POST | `/api/proposals/{id}/estimate/return-with-query` | Return with query note | AE, SE, CityEngineer |
| GET | `/api/estimates/pending` | List estimates pending my approval | AE, SE, CityEngineer |

---

## 5. Technical Sanctions (Tab 4)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/proposals/{id}/technical-sanction` | Get TS details | Anyone in loop |
| POST | `/api/proposals/{id}/technical-sanction` | Create/update TS data | TS |
| POST | `/api/proposals/{id}/technical-sanction/sign` | AE/SE/CE signs both Estimate + TS | AE, SE, CityEngineer |
| GET | `/api/technical-sanctions/pending` | List TS work pending | TS, AE, SE, CityEngineer |

---

## 6. PRAMA (Tab 5)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/proposals/{id}/prama` | Get PRAMA details (auto-fetched + manual) | Anyone in loop |
| PUT | `/api/proposals/{id}/prama` | Save/update PRAMA data | Creator JE |

---

## 7. Budget (Tab 6)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/proposals/{id}/budget` | Get budget details | Anyone in loop |
| PUT | `/api/proposals/{id}/budget` | Save/update budget data | Creator JE |
| POST | `/api/proposals/{id}/submit` | Submit — generates consolidated PDF, enters chain | Creator JE |
| POST | `/api/proposals/{id}/unpark` | Manually unpark proposal | AccountOfficer, Lotus |

---

## 8. Workflow / Approvals (Formal Chain)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/proposals/{id}/approve` | Approve at current stage (disclaimer + sign) | Current stage owner |
| POST | `/api/proposals/{id}/pushback` | Push back to JE with mandatory note | Current stage owner |
| GET | `/api/proposals/{id}/approval-history` | Full approval timeline | Anyone in loop |
| GET | `/api/approvals/pending` | List proposals pending my approval | Approver roles |

---

## 9. Notifications

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/notifications` | List notifications (paginated) | Token |
| GET | `/api/notifications/unread-count` | Get unread count | Token |
| POST | `/api/notifications/{id}/read` | Mark as read | Token |
| POST | `/api/notifications/read-all` | Mark all as read | Token |

---

## 10. Masters

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/masters/departments` | List departments | Token |
| GET | `/api/masters/dept-work-categories` | List work categories (filterable by dept) | Token |
| GET | `/api/masters/zones` | List zones | Token |
| GET | `/api/masters/prabhags` | List prabhags (filterable by zone) | Token |
| GET | `/api/masters/designations` | List designations | Token |
| GET | `/api/masters/request-sources` | List request sources | Token |
| GET | `/api/masters/site-conditions` | List site conditions | Token |
| GET | `/api/masters/work-execution-methods` | List tendering methods | Token |
| GET | `/api/masters/fund-types` | List fund types | Token |
| GET | `/api/masters/budget-heads` | List budget heads (filterable by dept + fund type + year) | Token |
| GET | `/api/masters/priorities` | List priorities (static: High/Med/Low) | Token |
| GET | `/api/masters/users-by-role` | List users by role (+ optional dept filter) | Token |

---

## 11. Audit Trail

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/audit` | Query audit trail (paginated, filterable) | Commissioner, Auditor, Lotus |

---

## 12. Lotus Admin

All Lotus endpoints under `/api/lotus/` — require `[Authorize(Roles = "Lotus")]`.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET/POST/PUT/DELETE | `/api/lotus/users` | Full CRUD on users |
| POST | `/api/lotus/users/{id}/signature` | Upload user signature |
| GET/POST/PUT/DELETE | `/api/lotus/departments` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/dept-work-categories` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/zones` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/prabhags` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/designations` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/request-sources` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/site-conditions` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/work-execution-methods` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/fund-types` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/budget-heads` | Full CRUD |
| GET/POST/PUT/DELETE | `/api/lotus/palikas` | Full CRUD on Municipal Corporations |
| GET/PUT | `/api/lotus/palikas/{id}/settings` | Palika-specific settings (SMS, OTP, language, logo) |
| GET | `/api/lotus/audit` | Full audit trail (all modules) |
| GET | `/api/lotus/proposals` | All proposals (including soft-deleted) |
| PUT | `/api/lotus/proposals/{id}/force-stage` | Force change proposal stage |
| POST | `/api/lotus/proposals/{id}/unpark` | Force unpark |

---

## Common Response Patterns

### Success

```json
{
  "success": true,
  "data": { ... },
  "message": null
}
```

### Paginated List

```json
{
  "success": true,
  "data": {
    "items": [ ... ],
    "pageIndex": 1,
    "pageSize": 20,
    "totalCount": 142,
    "totalPages": 8
  }
}
```

### Error

```json
{
  "success": false,
  "data": null,
  "message": "Proposal not found",
  "errors": ["ProposalId: Invalid proposal ID"]
}
```

### Validation Error (400)

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "WorkTitle_En: Work title is required",
    "DepartmentId: Please select a department"
  ]
}
```
