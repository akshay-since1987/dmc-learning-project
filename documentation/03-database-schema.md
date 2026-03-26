# Database Schema

## Overview

Fresh database: `dmc-ProposalManagement` on `.\SQLEXPRESS`.
All user-facing text fields have dual columns: `_En` (English) + `_Mr` (Marathi).
All business entities have `IsDeleted BIT DEFAULT 0` for soft deletes.

**Multi-tenant:** The system supports multiple Municipal Corporations (Mahanagarpalika)
and Nagar Palikas via the `Palikas` table. Nearly all master and transactional
tables carry a `PalikaId` FK for tenant scoping.

---

## 0. Palikas (Municipal Corporations / Nagar Palikas)

Root tenant entity. Every zone, prabhag, department, user, and proposal
belongs to exactly one Palika.

```sql
Palikas
├── Id                    UNIQUEIDENTIFIER PK DEFAULT NEWID()
├── Name_En               NVARCHAR(300) NOT NULL
├── Name_Mr               NVARCHAR(300)
├── ShortCode             NVARCHAR(20) NOT NULL UNIQUE   -- DMC, NPC, etc.
├── Type                  NVARCHAR(50) NOT NULL           -- MahanagarPalika | NagarPalika | NagarPanchayat
├── LogoUrl               NVARCHAR(500) NULL
├── Address_En            NVARCHAR(500) NULL
├── Address_Mr            NVARCHAR(500) NULL
├── ContactPhone          NVARCHAR(20) NULL
├── Website               NVARCHAR(300) NULL
├── PrimaryLanguage       NVARCHAR(5) DEFAULT 'en'
├── AlternateLanguage     NVARCHAR(5) DEFAULT 'mr'
├── ProposalNumberPrefix  NVARCHAR(20) NOT NULL           -- e.g. 'DMC' → DMC/2026/00001
├── CurrentFinancialYear  NVARCHAR(20) NOT NULL           -- e.g. '2025-2026'
├── SmsGatewayProvider    NVARCHAR(100) NULL
├── SmsGatewayApiKey      NVARCHAR(500) NULL              -- Encrypted at rest
├── OtpExpiryMinutes      INT DEFAULT 5
├── OtpMaxAttempts        INT DEFAULT 3
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2 NOT NULL
├── UpdatedAt             DATETIME2 NOT NULL
```

> **Note:** This replaces the old `CorporationSettings` singleton table.
> All per-corporation config lives here so each Palika has its own settings.

---

## 1. Auth & Users

### Users

```sql
Users
├── Id                    UNIQUEIDENTIFIER PK DEFAULT NEWID()
├── FullName_En           NVARCHAR(200) NOT NULL
├── FullName_Mr           NVARCHAR(200)
├── MobileNumber          NVARCHAR(15) NOT NULL UNIQUE
├── Email                 NVARCHAR(200) NULL
├── PasswordHash          NVARCHAR(500) NULL          -- Lotus users only (OTP + password)
├── Role                  NVARCHAR(50) NOT NULL        -- JE | TS | AE | SE | CityEngineer | AccountOfficer | DyCommissioner | Commissioner | StandingCommittee | Collector | Auditor | Lotus
├── DepartmentId          UNIQUEIDENTIFIER FK NULL     → Departments.Id
├── DesignationId         UNIQUEIDENTIFIER FK NULL     → Designations.Id
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── SignaturePath          NVARCHAR(500) NULL           -- Path to uploaded PNG signature
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2 NOT NULL
├── UpdatedAt             DATETIME2 NOT NULL
```

### OtpRequests

```sql
OtpRequests
├── Id                    BIGINT IDENTITY PK
├── MobileNumber          NVARCHAR(15) NOT NULL
├── OtpHash               NVARCHAR(500) NOT NULL
├── Purpose               NVARCHAR(50) NOT NULL        -- Login | PasswordReset
├── ExpiresAt             DATETIME2 NOT NULL
├── IsUsed                BIT DEFAULT 0
├── AttemptCount          INT DEFAULT 0
├── CreatedAt             DATETIME2 NOT NULL
```

### RefreshTokens

```sql
RefreshTokens
├── Id                    UNIQUEIDENTIFIER PK
├── UserId                UNIQUEIDENTIFIER FK NOT NULL  → Users.Id
├── Token                 NVARCHAR(500) NOT NULL
├── ExpiresAt             DATETIME2 NOT NULL
├── CreatedAt             DATETIME2 NOT NULL
├── RevokedAt             DATETIME2 NULL
```

---

## 2. Master Tables

All masters follow the pattern: `Id`, `Name_En`, `Name_Mr`, `Code` (optional),
`IsActive`, `IsDeleted`, `CreatedAt`, `UpdatedAt`.

### Departments

```sql
Departments
├── Id                    UNIQUEIDENTIFIER PK
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── Name_En               NVARCHAR(200) NOT NULL
├── Name_Mr               NVARCHAR(200)
├── Code                  NVARCHAR(20) NULL
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### DeptWorkCategories

```sql
DeptWorkCategories
├── Id                    UNIQUEIDENTIFIER PK
├── DepartmentId          UNIQUEIDENTIFIER FK NULL     → Departments.Id (NULL = generic)
├── Name_En               NVARCHAR(200) NOT NULL
├── Name_Mr               NVARCHAR(200)
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### Designations

```sql
Designations
├── Id                    UNIQUEIDENTIFIER PK
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── Name_En               NVARCHAR(200) NOT NULL
├── Name_Mr               NVARCHAR(200)
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### Zones (क्षेत्रीय कार्यालय / Prabhag Samiti Karyalaya)

Administrative zone offices. Each zone covers multiple Prabhags.

```sql
Zones
├── Id                    UNIQUEIDENTIFIER PK
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── Name_En               NVARCHAR(200) NOT NULL
├── Name_Mr               NVARCHAR(200)
├── Code                  NVARCHAR(20) NULL
├── OfficeName_En         NVARCHAR(300) NULL             -- e.g. "Devpur (Navrang Tanki)"
├── OfficeName_Mr         NVARCHAR(300) NULL             -- e.g. "देवपूर (नवरंग टाकी)"
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### Prabhags (प्रभाग — child of Zone)

Electoral/administrative divisions within a zone. Each Prabhag has
multiple corporator seats (नगरसेवक). DMC has 19 Prabhags, 74 seats.

```sql
Prabhags
├── Id                    UNIQUEIDENTIFIER PK
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── ZoneId                UNIQUEIDENTIFIER FK NOT NULL  → Zones.Id
├── Number                INT NOT NULL                   -- Prabhag number (1-19 for DMC)
├── Name_En               NVARCHAR(200) NOT NULL
├── Name_Mr               NVARCHAR(200)
├── CorporatorSeats       INT DEFAULT 4                  -- Number of elected members
├── Population            INT NULL                       -- Census / estimated population
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### RequestSources

```sql
RequestSources
├── Id                    UNIQUEIDENTIFIER PK
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── Name_En               NVARCHAR(200) NOT NULL       -- Citizen, MLA, MP, Commissioner
├── Name_Mr               NVARCHAR(200)
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### SiteConditions

```sql
SiteConditions
├── Id                    UNIQUEIDENTIFIER PK
├── Name_En               NVARCHAR(100) NOT NULL       -- Worse, Bad, OK, Good, Better
├── Name_Mr               NVARCHAR(100)
├── SortOrder             INT DEFAULT 0
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### WorkExecutionMethods

```sql
WorkExecutionMethods
├── Id                    UNIQUEIDENTIFIER PK
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── Name_En               NVARCHAR(200) NOT NULL       -- Tendering options
├── Name_Mr               NVARCHAR(200)
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### FundTypes

```sql
FundTypes
├── Id                    UNIQUEIDENTIFIER PK
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── Name_En               NVARCHAR(200) NOT NULL       -- MNP, State, Central, DPDC
├── Name_Mr               NVARCHAR(200)
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### BudgetHeads

```sql
BudgetHeads
├── Id                    UNIQUEIDENTIFIER PK
├── PalikaId              UNIQUEIDENTIFIER FK NOT NULL  → Palikas.Id
├── DepartmentId          UNIQUEIDENTIFIER FK NOT NULL  → Departments.Id
├── FundTypeId            UNIQUEIDENTIFIER FK NOT NULL  → FundTypes.Id
├── Code                  NVARCHAR(50) NOT NULL
├── Name_En               NVARCHAR(300) NOT NULL
├── Name_Mr               NVARCHAR(300)
├── FinancialYear         NVARCHAR(20) NOT NULL         -- e.g. "2025-2026"
├── AllocatedAmount       DECIMAL(18,2) NOT NULL        -- Total allocated for the year
├── CurrentAvailable      DECIMAL(18,2) NOT NULL        -- Currently available (decremented on approval)
├── IsActive              BIT DEFAULT 1
├── IsDeleted             BIT DEFAULT 0
├── CreatedAt             DATETIME2
├── UpdatedAt             DATETIME2
```

### Priorities (Enum — stored as NVARCHAR in Proposals, no master table)

Values: `High`, `Medium`, `Low`

---

## 3. Proposals (Tab 1)

```sql
Proposals
├── Id                          UNIQUEIDENTIFIER PK DEFAULT NEWID()
├── ProposalNumber              NVARCHAR(50) NOT NULL UNIQUE  -- Auto: DMC/2026/00001
├── PalikaId                    UNIQUEIDENTIFIER FK NOT NULL   → Palikas.Id
├── ProposalDate                DATE NOT NULL DEFAULT GETDATE()
├── DepartmentId                UNIQUEIDENTIFIER FK NOT NULL   → Departments.Id
├── DeptWorkCategoryId          UNIQUEIDENTIFIER FK NOT NULL   → DeptWorkCategories.Id
├── CreatedById                 UNIQUEIDENTIFIER FK NOT NULL   → Users.Id (JE who created)
│
│   -- Location
├── ZoneId                      UNIQUEIDENTIFIER FK NOT NULL   → Zones.Id
├── PrabhagId                   UNIQUEIDENTIFIER FK NOT NULL   → Prabhags.Id
├── Area                        NVARCHAR(300) NULL
├── LocationAddress_En          NVARCHAR(500) NULL
├── LocationAddress_Mr          NVARCHAR(500) NULL
├── LocationMapPath             NVARCHAR(500) NULL             -- Uploaded map/image file
│
│   -- Work Details
├── WorkTitle_En                NVARCHAR(500) NOT NULL
├── WorkTitle_Mr                NVARCHAR(500)
├── WorkDescription_En          NVARCHAR(MAX) NOT NULL
├── WorkDescription_Mr          NVARCHAR(MAX)
│
│   -- Request Source
├── RequestSourceId             UNIQUEIDENTIFIER FK NULL       → RequestSources.Id
├── RequestorName               NVARCHAR(200) NULL
├── RequestorMobile             NVARCHAR(15) NULL
├── RequestorAddress            NVARCHAR(500) NULL
├── RequestorDesignation        NVARCHAR(200) NULL
├── RequestorOrganisation       NVARCHAR(200) NULL
│
├── Priority                    NVARCHAR(20) NOT NULL          -- High | Medium | Low
│
│   -- Workflow State
├── CurrentStage                NVARCHAR(50) NOT NULL          -- See Workflow doc
├── CurrentOwnerId              UNIQUEIDENTIFIER FK NULL       → Users.Id
├── PushBackCount               INT DEFAULT 0
├── ParkedAt                    DATETIME2 NULL                 -- Set when parked
├── ParkedAtStage               NVARCHAR(50) NULL              -- Stage when parked
│
│   -- Completion Tracking
├── CompletedTab                INT DEFAULT 1                  -- Highest completed tab (1-6)
│
│   -- System
├── IsDeleted                   BIT DEFAULT 0
├── CreatedAt                   DATETIME2 NOT NULL
├── UpdatedAt                   DATETIME2 NOT NULL
```

---

## 4. Proposal Documents (multi-file upload on any tab)

```sql
ProposalDocuments
├── Id                          UNIQUEIDENTIFIER PK
├── ProposalId                  UNIQUEIDENTIFIER FK NOT NULL   → Proposals.Id
├── TabNumber                   INT NOT NULL                   -- 1-6 (which tab uploaded from)
├── DocumentType                NVARCHAR(50) NOT NULL          -- LocationMap | SitePhoto | EstimateCopy | TechnicalSanctionDoc | OutsideApprovalLetter | FieldVisitReport | GeoTaggedPhoto | SupportingDoc | Other
├── DocName                     NVARCHAR(300) NULL             -- User-given label
├── FileName                    NVARCHAR(300) NOT NULL
├── FileSize                    BIGINT NOT NULL
├── ContentType                 NVARCHAR(100) NOT NULL
├── StoragePath                 NVARCHAR(500) NOT NULL
├── UploadedById                UNIQUEIDENTIFIER FK NOT NULL   → Users.Id
├── IsDeleted                   BIT DEFAULT 0
├── CreatedAt                   DATETIME2 NOT NULL
```

---

## 5. Field Visits (Tab 2) — Multiple per Proposal

```sql
FieldVisits
├── Id                          UNIQUEIDENTIFIER PK
├── ProposalId                  UNIQUEIDENTIFIER FK NOT NULL   → Proposals.Id
├── VisitNumber                 INT NOT NULL                   -- Sequential: 1, 2, 3...
├── AssignedToId                UNIQUEIDENTIFIER FK NOT NULL   → Users.Id (JE assigned)
├── AssignedById                UNIQUEIDENTIFIER FK NOT NULL   → Users.Id (JE who assigned)
│
├── InspectionById              UNIQUEIDENTIFIER FK NULL       → Users.Id (JE who did inspection)
├── InspectionDate              DATE NULL
├── SiteConditionId             UNIQUEIDENTIFIER FK NULL       → SiteConditions.Id
├── ProblemDescription_En       NVARCHAR(MAX) NULL
├── ProblemDescription_Mr       NVARCHAR(MAX) NULL
├── Measurements_En             NVARCHAR(MAX) NULL             -- Optional
├── Measurements_Mr             NVARCHAR(MAX) NULL
├── GpsLatitude                 DECIMAL(10,7) NULL
├── GpsLongitude                DECIMAL(10,7) NULL
├── Remark_En                   NVARCHAR(MAX) NULL
├── Remark_Mr                   NVARCHAR(MAX) NULL
├── Recommendation_En           NVARCHAR(MAX) NULL
├── Recommendation_Mr           NVARCHAR(MAX) NULL
├── UploadedPdfPath             NVARCHAR(500) NULL             -- Pre-existing inspection PDF
│
├── SignaturePath                NVARCHAR(500) NULL             -- PNG signature on save
├── Status                      NVARCHAR(20) NOT NULL          -- Assigned | InProgress | Completed
├── CompletedAt                 DATETIME2 NULL
│
├── IsDeleted                   BIT DEFAULT 0
├── CreatedAt                   DATETIME2 NOT NULL
├── UpdatedAt                   DATETIME2 NOT NULL
```

### FieldVisitPhotos (multiple photos per visit)

```sql
FieldVisitPhotos
├── Id                          UNIQUEIDENTIFIER PK
├── FieldVisitId                UNIQUEIDENTIFIER FK NOT NULL   → FieldVisits.Id
├── FileName                    NVARCHAR(300) NOT NULL
├── FileSize                    BIGINT NOT NULL
├── ContentType                 NVARCHAR(100) NOT NULL
├── StoragePath                 NVARCHAR(500) NOT NULL
├── Caption                     NVARCHAR(300) NULL
├── CreatedAt                   DATETIME2 NOT NULL
```

---

## 6. Estimates (Tab 3)

```sql
Estimates
├── Id                          UNIQUEIDENTIFIER PK
├── ProposalId                  UNIQUEIDENTIFIER FK NOT NULL UNIQUE → Proposals.Id
├── EstimatePdfPath             NVARCHAR(500) NULL             -- Uploaded estimate PDF
├── EstimatedCost               DECIMAL(18,2) NULL             -- Amount from estimate
├── PreparedById                UNIQUEIDENTIFIER FK NOT NULL   → Users.Id (JE)
├── PreparedSignaturePath       NVARCHAR(500) NULL             -- JE's PNG signature
│
│   -- Approval by AE/SE/City Engineer
├── SentToRole                  NVARCHAR(50) NULL              -- AE | SE | CityEngineer
├── SentToId                    UNIQUEIDENTIFIER FK NULL       → Users.Id
├── ApprovedById                UNIQUEIDENTIFIER FK NULL       → Users.Id
├── ApproverSignaturePath       NVARCHAR(500) NULL
├── ApproverDisclaimerAccepted  BIT DEFAULT 0
├── ApproverOpinion_En          NVARCHAR(MAX) NULL
├── ApproverOpinion_Mr          NVARCHAR(MAX) NULL
├── Status                      NVARCHAR(30) NOT NULL          -- Draft | SentForApproval | ReturnedWithQuery | Approved
├── ReturnQueryNote_En          NVARCHAR(MAX) NULL
├── ReturnQueryNote_Mr          NVARCHAR(MAX) NULL
│
├── ApprovedAt                  DATETIME2 NULL
├── IsDeleted                   BIT DEFAULT 0
├── CreatedAt                   DATETIME2 NOT NULL
├── UpdatedAt                   DATETIME2 NOT NULL
```

---

## 7. Technical Sanctions (Tab 4)

```sql
TechnicalSanctions
├── Id                          UNIQUEIDENTIFIER PK
├── ProposalId                  UNIQUEIDENTIFIER FK NOT NULL UNIQUE → Proposals.Id
├── TsNumber                    NVARCHAR(100) NULL
├── TsDate                      DATE NULL
├── TsAmount                    DECIMAL(18,2) NULL
├── Description_En              NVARCHAR(MAX) NULL
├── Description_Mr              NVARCHAR(MAX) NULL
│
│   -- Documents
├── TsPdfPath                   NVARCHAR(500) NULL             -- Uploaded TS PDF
├── OutsideApprovalLetterPath   NVARCHAR(500) NULL             -- OR external approval
│
│   -- Sanctioned By details
├── SanctionedByName            NVARCHAR(200) NULL
├── SanctionedByDept            NVARCHAR(200) NULL
├── SanctionedByDesignation     NVARCHAR(200) NULL
│
│   -- Prepared/Filled by TS role
├── PreparedById                UNIQUEIDENTIFIER FK NULL       → Users.Id (TS user)
│
│   -- Signed by AE/SE/City Engineer
├── SignedById                  UNIQUEIDENTIFIER FK NULL       → Users.Id
├── SignerSignaturePath         NVARCHAR(500) NULL
├── SignedAt                    DATETIME2 NULL
│
├── Status                      NVARCHAR(30) NOT NULL          -- Draft | Pending | Signed
├── IsDeleted                   BIT DEFAULT 0
├── CreatedAt                   DATETIME2 NOT NULL
├── UpdatedAt                   DATETIME2 NOT NULL
```

---

## 8. PRAMA Data (Tab 5)

```sql
PramaDetails
├── Id                          UNIQUEIDENTIFIER PK
├── ProposalId                  UNIQUEIDENTIFIER FK NOT NULL UNIQUE → Proposals.Id
│
│   -- Most fields are fetched from Tabs 1-4, but these are PRAMA-specific additions:
├── FundTypeId                  UNIQUEIDENTIFIER FK NULL       → FundTypes.Id
├── BudgetHeadId                UNIQUEIDENTIFIER FK NULL       → BudgetHeads.Id
├── FundApprovalYear            NVARCHAR(20) NULL              -- e.g. "2025-2026"
├── DeptUserName_En             NVARCHAR(200) NULL             -- Selected dept user
├── DeptUserName_Mr             NVARCHAR(200) NULL
├── References_En               NVARCHAR(MAX) NULL             -- संदर्भ — editable by JE
├── References_Mr               NVARCHAR(MAX) NULL
├── AdditionalDetails_En        NVARCHAR(MAX) NULL             -- Any extra notes
├── AdditionalDetails_Mr        NVARCHAR(MAX) NULL
│
├── IsDeleted                   BIT DEFAULT 0
├── CreatedAt                   DATETIME2 NOT NULL
├── UpdatedAt                   DATETIME2 NOT NULL
```

---

## 9. Budget Details (Tab 6)

```sql
BudgetDetails
├── Id                          UNIQUEIDENTIFIER PK
├── ProposalId                  UNIQUEIDENTIFIER FK NOT NULL UNIQUE → Proposals.Id
│
├── WorkExecutionMethodId       UNIQUEIDENTIFIER FK NULL       → WorkExecutionMethods.Id
├── WorkDurationDays            INT NULL
├── TenderVerificationDone      BIT DEFAULT 0
│
├── BudgetHeadId                UNIQUEIDENTIFIER FK NULL       → BudgetHeads.Id
├── AllocatedFund               DECIMAL(18,2) NULL             -- Snapshot from BudgetHeads at time of filling
├── CurrentAvailableFund        DECIMAL(18,2) NULL             -- Snapshot
├── OldExpenditure              DECIMAL(18,2) NULL             -- Manual entry
├── EstimatedCost               DECIMAL(18,2) NULL             -- From Estimate (Tab 3)
├── BalanceAmount               DECIMAL(18,2) NULL             -- Computed: Available - Estimated
│
├── AccountSerialNo             NVARCHAR(100) NULL
│
│   -- Compliance
├── ComplianceNotes_En          NVARCHAR(MAX) NULL
├── ComplianceNotes_Mr          NVARCHAR(MAX) NULL
│
│   -- Auto-determined approval authority
├── DeterminedApprovalSlab      NVARCHAR(50) NULL              -- Slab0to3L | Slab3to24L | Slab24to25L | Slab25LPlus
├── FinalAuthorityRole          NVARCHAR(50) NULL              -- DyCommissioner | Commissioner | StandingCommittee | Collector
│
├── IsDeleted                   BIT DEFAULT 0
├── CreatedAt                   DATETIME2 NOT NULL
├── UpdatedAt                   DATETIME2 NOT NULL
```

---

## 10. Approval Stage History (Formal Chain)

```sql
ProposalApprovals
├── Id                          BIGINT IDENTITY PK
├── ProposalId                  UNIQUEIDENTIFIER FK NOT NULL   → Proposals.Id (indexed)
├── StageRole                   NVARCHAR(50) NOT NULL          -- CityEngineer | AccountOfficer | DyCommissioner | Commissioner | StandingCommittee | Collector
├── Action                      NVARCHAR(30) NOT NULL          -- Approve | PushBack
│
│   -- Approver info
├── ActorId                     UNIQUEIDENTIFIER FK NOT NULL   → Users.Id
├── ActorName_En                NVARCHAR(200)
├── ActorName_Mr                NVARCHAR(200)
├── ActorDesignation_En         NVARCHAR(200)
├── ActorDesignation_Mr         NVARCHAR(200)
│
│   -- Disclaimer
├── DisclaimerText              NVARCHAR(MAX) NOT NULL         -- The exact Marathi disclaimer shown
├── DisclaimerAccepted          BIT NOT NULL                   -- Must be true for Approve
│
│   -- Opinion & Signature
├── Opinion_En                  NVARCHAR(MAX) NULL             -- अन्य अभिप्राय
├── Opinion_Mr                  NVARCHAR(MAX) NULL
├── SignaturePath               NVARCHAR(500) NULL             -- PNG path
│
│   -- Push-back specific
├── PushBackNote_En             NVARCHAR(MAX) NULL             -- Mandatory for PushBack
├── PushBackNote_Mr             NVARCHAR(MAX) NULL
│
│   -- PDF
├── ConsolidatedPdfPath         NVARCHAR(500) NULL             -- PDF generated at this stage
│
├── CreatedAt                   DATETIME2 NOT NULL (indexed)
```

---

## 11. Generated PDFs

```sql
GeneratedPdfs
├── Id                          UNIQUEIDENTIFIER PK
├── ProposalId                  UNIQUEIDENTIFIER FK NOT NULL   → Proposals.Id
├── PdfType                     NVARCHAR(50) NOT NULL          -- Tab1 | Tab2 | Tab3 | Tab4 | Tab5 | Tab6 | Consolidated | StageApproval | FinalCombined
├── TabNumber                   INT NULL                       -- 1-6 for tab-specific PDFs
├── StageRole                   NVARCHAR(50) NULL              -- For stage-specific PDFs
├── Title_En                    NVARCHAR(300) NULL
├── Title_Mr                    NVARCHAR(300) NULL
├── StoragePath                 NVARCHAR(500) NOT NULL
├── GeneratedById               UNIQUEIDENTIFIER FK NOT NULL   → Users.Id
├── FileSize                    BIGINT NULL
├── CreatedAt                   DATETIME2 NOT NULL
```

---

## 12. Notifications

```sql
Notifications
├── Id                          BIGINT IDENTITY PK
├── UserId                      UNIQUEIDENTIFIER FK NOT NULL   → Users.Id (recipient)
├── PalikaId                    UNIQUEIDENTIFIER FK NOT NULL   → Palikas.Id
├── ProposalId                  UNIQUEIDENTIFIER FK NULL       → Proposals.Id
├── Type                        NVARCHAR(50) NOT NULL          -- Assignment | Approval | PushBack | Parked | Unparked | FieldVisitAssigned | EstimateReturned | TSCompleted | General
├── Title_En                    NVARCHAR(300) NOT NULL
├── Title_Mr                    NVARCHAR(300)
├── Message_En                  NVARCHAR(1000) NOT NULL
├── Message_Mr                  NVARCHAR(1000)
├── IsRead                      BIT DEFAULT 0
├── ReadAt                      DATETIME2 NULL
├── CreatedAt                   DATETIME2 NOT NULL
```

---

## 13. Audit Trail (Append-Only, Immutable)

```sql
AuditTrail
├── Id                          BIGINT IDENTITY PK
├── Timestamp                   DATETIME2 NOT NULL (indexed)
├── PalikaId                    UNIQUEIDENTIFIER NULL          → Palikas.Id
├── UserId                      UNIQUEIDENTIFIER NULL
├── UserName                    NVARCHAR(200)
├── UserRole                    NVARCHAR(50)
├── IpAddress                   NVARCHAR(45)
├── UserAgent                   NVARCHAR(500)
├── Action                      NVARCHAR(50) NOT NULL          -- Create | Update | Delete | Login | Logout | Approve | PushBack | Submit | Upload | Download | Generate | Assign | Park | Unpark | FailedAuth
├── EntityType                  NVARCHAR(100) NOT NULL         -- Proposal | FieldVisit | Estimate | TechnicalSanction | Prama | Budget | User | Master | Notification | ...
├── EntityId                    NVARCHAR(100)
├── Description                 NVARCHAR(1000)
├── OldValues                   NVARCHAR(MAX) NULL             -- JSON diff
├── NewValues                   NVARCHAR(MAX) NULL             -- JSON diff
├── Module                      NVARCHAR(50) NOT NULL          -- Auth | Proposal | FieldVisit | Estimate | TS | Prama | Budget | Workflow | Lotus | Master | Document | System
├── Severity                    NVARCHAR(20) NOT NULL          -- Info | Warning | Critical
```

> **Permissions**: INSERT-only at DB level. No role can UPDATE or DELETE audit
> records. Viewable by Lotus, Commissioner, and Auditor.

---

## 14. Corporation Settings → REMOVED

Corporation/Palika settings are now stored directly in the `Palikas` table
(Section 0). The old `CorporationSettings` singleton table is no longer needed.

All per-corporation config fields (`ProposalNumberPrefix`, `CurrentFinancialYear`,
`SmsGatewayProvider`, `SmsGatewayApiKey`, `OtpExpiryMinutes`, `OtpMaxAttempts`,
`LogoUrl`, languages) live in the `Palikas` table.

---

## Index Strategy

| Table | Indexed Columns |
| ----- | --------------- |
| Palikas | ShortCode (unique) |
| Proposals | ProposalNumber (unique), PalikaId, CurrentStage, CurrentOwnerId, CreatedById, DepartmentId, CreatedAt |
| FieldVisits | ProposalId, AssignedToId, Status |
| Estimates | ProposalId (unique) |
| TechnicalSanctions | ProposalId (unique) |
| ProposalApprovals | ProposalId, StageRole, CreatedAt |
| Notifications | UserId + IsRead, PalikaId, ProposalId, CreatedAt |
| AuditTrail | Timestamp, PalikaId, EntityType + EntityId, UserId, Module |
| BudgetHeads | PalikaId + DepartmentId + FundTypeId + FinancialYear |
| Prabhags | PalikaId, ZoneId, Number |
| Zones | PalikaId |
| Users | MobileNumber (unique), PalikaId, Role, DepartmentId |
| Departments | PalikaId |
