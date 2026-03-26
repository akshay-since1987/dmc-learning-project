# Master Tables Catalog

All master tables follow the pattern:
- `Id` (UNIQUEIDENTIFIER PK)
- `PalikaId` (UNIQUEIDENTIFIER FK → Palikas — for tenant scoping)
- `Name_En` / `Name_Mr` (bilingual)
- `Code` (optional short code)
- `IsActive` (BIT, default 1)
- `IsDeleted` (BIT, default 0, soft-delete)
- `CreatedAt`, `UpdatedAt` (DATETIME2)

Masters are maintained by the **Lotus** super-admin and by the relevant
department officers where noted.

**Multi-tenant:** Masters are scoped per Palika. SiteConditions and Priorities
are the only global/shared masters (no PalikaId).

---

## 0. Palikas (Municipal Corporations / Nagar Palikas)

Root tenant entity. All zones, prabhags, departments, users, and proposals
belong to exactly one Palika.

| Name (En) | Name (Mr) | ShortCode | Type |
|-----------|-----------|-----------|------|
| Dhule Municipal Corporation | धुळे महानगरपालिका | DMC | MahanagarPalika |

*Note: Additional palikas (Nagar Palikas like Shirpur, Dondaicha, etc.) can be
added at runtime by Lotus.*

---

## 1. Departments

Department that submits the proposal.

| Seed Data (En) | Seed Data (Mr) | Code |
|-----------------|----------------|------|
| Water Supply | पाणी पुरवठा | WS |
| Drainage | मलनिस्सारण | DR |
| Roads | रस्ते | RD |
| Electrical | विद्युत | EL |
| Building Repair | इमारत दुरुस्ती | BR |
| Garden | उद्यान | GD |
| Solid Waste Management | घनकचरा व्यवस्थापन | SWM |
| Town Planning | नगर रचना | TP |
| Fire Brigade | अग्निशमन | FB |
| Health | आरोग्य | HL |
| Education | शिक्षण | ED |
| General Administration | सामान्य प्रशासन | GA |
| Revenue | महसूल | RV |
| Accounts | लेखा | AC |

*Note: Extendable — Lotus can add more departments at runtime.*

---

## 2. Department Work Categories

Category of work performed by a department (child of Department).

| Parent Dept | Category (En) | Category (Mr) |
|-------------|---------------|----------------|
| Water Supply | Pipeline Repair | जलवाहिनी दुरुस्ती |
| Water Supply | New Connection | नवीन जोडणी |
| Water Supply | Pump House Maintenance | पंपहाऊस देखभाल |
| Drainage | Drain Cleaning | नाला सफाई |
| Drainage | New Drain Construction | नवीन नाला बांधकाम |
| Roads | Pothole Repair | खड्डे दुरुस्ती |
| Roads | New Road Construction | नवीन रस्ता बांधकाम |
| Roads | Cement Concrete Road | सिमेंट कॉंक्रीट रस्ता |
| Electrical | Street Light Repair | पथदिवे दुरुस्ती |
| Electrical | New Street Light Installation | नवीन पथदिवे बसवणे |
| Building Repair | Roof Repair | छत दुरुस्ती |
| Building Repair | Wall Plastering | भिंत प्लास्टर |
| Garden | Tree Planting | वृक्षारोपण |
| Garden | Garden Maintenance | बाग देखभाल |

*Note: This is seed data only. Each department will have its own categories
added over time.*

**Schema:**
```
DeptWorkCategories
├── Id                  UNIQUEIDENTIFIER PK
├── DepartmentId        UNIQUEIDENTIFIER FK → Departments
├── Name_En             NVARCHAR(200)
├── Name_Mr             NVARCHAR(200)
├── Code                NVARCHAR(20) NULL
├── IsActive            BIT DEFAULT 1
├── IsDeleted           BIT DEFAULT 0
├── CreatedAt           DATETIME2
├── UpdatedAt           DATETIME2
```

---

## 3. Zones (क्षेत्रीय कार्यालय / Prabhag Samiti Karyalaya)

Administrative zone offices within a Municipal Corporation. Each zone
covers multiple Prabhags and has a dedicated zonal office.

| # | Zone (En) | Zone (Mr) | Office Name (Mr) | Code | Prabhags Covered |
|---|-----------|-----------|-------------------|------|------------------|
| 1 | Zone 1 — Devpur | प्रभाग समिती क्र. १ — देवपूर | देवपूर (नवरंग टाकी) | Z1 | 1, 2, 4, 5, 6, 7 |
| 2 | Zone 2 — Subhash Putala | प्रभाग समिती क्र. २ — सुभाष पुतळा | सुभाष पुतळा | Z2 | 3, 11, 12, 15 |
| 3 | Zone 3 — Agra Road | प्रभाग समिती क्र. ३ — आग्रा रोड | आग्रा रोड | Z3 | 13, 14, 16, 18, 19 |
| 4 | Zone 4 — Lenin Chowk | प्रभाग समिती क्र. ४ — लेनिन चौक | लेनिन चौक | Z4 | 8, 9, 10, 17 |

*Source: dhulecorporation.org/dmc/about-corporation — official DMC data.*

**Schema:**
```
Zones
├── Id                  UNIQUEIDENTIFIER PK
├── PalikaId            UNIQUEIDENTIFIER FK → Palikas
├── Name_En             NVARCHAR(200)
├── Name_Mr             NVARCHAR(200)
├── Code                NVARCHAR(20)
├── OfficeName_En       NVARCHAR(300)       -- Zonal office location
├── OfficeName_Mr       NVARCHAR(300)
├── IsActive            BIT DEFAULT 1
├── IsDeleted           BIT DEFAULT 0
├── CreatedAt           DATETIME2
├── UpdatedAt           DATETIME2
```

---

## 4. Wards

Administrative wards within a zone. Child of Zone.

| Parent Zone | Ward (En) | Ward (Mr) | Ward No |
|-------------|-----------|-----------|---------|
| Zone 1 | Ward 1 | वॉर्ड १ | 1 |
| Zone 1 | Ward 2 | वॉर्ड २ | 2 |
| Zone 2 | Ward 3 | वॉर्ड ३ | 3 |
| Zone 2 | Ward 4 | वॉर्ड ४ | 4 |
| Zone 3 | Ward 5 | वॉर्ड ५ | 5 |
| Zone 4 | Ward 6 | वॉर्ड ६ | 6 |

*Note: Actual ward data to be provided by DMC. Above is placeholder.*

**Schema:**
```
Wards
├── Id                  UNIQUEIDENTIFIER PK
├── ZoneId              UNIQUEIDENTIFIER FK → Zones
├── Number              INT
├── Name_En             NVARCHAR(200)
├── Name_Mr             NVARCHAR(200)
├── IsActive            BIT DEFAULT 1
├── IsDeleted           BIT DEFAULT 0
├── CreatedAt           DATETIME2
├── UpdatedAt           DATETIME2
```

**UI behaviour:** Zone dropdown → Ward dropdown (filtered by selected zone) →
Area (free text) → Address (free text)

---

## 5. Source of Request

How/who originated the work request.

| Source (En) | Source (Mr) |
|-------------|-------------|
| Citizen | नागरिक |
| MLA | आमदार |
| MP | खासदार |
| Commissioner | आयुक्त |

*Note: Extendable master. Lotus can add more sources.*

---

## 6. Site Conditions

Condition of the site at the time of field visit.

| Condition (En) | Condition (Mr) | Severity Order |
|----------------|----------------|----------------|
| Worse | अत्यंत खराब | 1 (most severe) |
| Bad | खराब | 2 |
| OK | ठीक | 3 |
| Good | चांगले | 4 |
| Better | उत्तम | 5 (least severe) |

---

## 7. Work Execution Methods

How the work will be executed (tendering/procurement method).

| Method (En) | Method (Mr) | Description |
|-------------|-------------|-------------|
| Direct Purchase | थेट खरेदी | For small works below threshold |
| Quotation | दरपत्रक | 3+ quotations collected |
| Limited Tender | मर्यादित निविदा | Invited bidders only |
| Open Tender | खुली निविदा | Public tender |
| e-Tender | ई-निविदा | Electronic tendering via govt portal |
| Rate Contract | दर करार | Pre-negotiated rates |
| Departmental Work | खातेनिहाय काम | Work done by dept's own staff |

---

## 8. Fund Types

Type/source of funding.

| Fund Type (En) | Fund Type (Mr) | Code |
|----------------|----------------|------|
| Municipal Own Fund | महानगरपालिका स्वनिधी | MNP |
| State Grant | राज्य अनुदान | STATE |
| Central Grant | केंद्र अनुदान | CENTRAL |
| DPDC | जिल्हा नियोजन | DPDC |
| 15th Finance Commission | १५ वा वित्त आयोग | 15FC |
| CSR Fund | CSR निधी | CSR |
| MP/MLA Fund | खासदार/आमदार निधी | MPMLA |

---

## 9. Budget Heads

Pre-loaded by the Accounts department. Each budget head has an allocated
amount per financial year per department.

| Budget Head Code | Name (En) | Name (Mr) | Typical Dept |
|------------------|-----------|-----------|--------------|
| 4210-01 | Capital Works - Water Supply | भांडवली कामे - पाणीपुरवठा | Water Supply |
| 4215-01 | Capital Works - Drainage | भांडवली कामे - मलनिस्सारण | Drainage |
| 4059-01 | Capital Works - Roads | भांडवली कामे - रस्ते | Roads |
| 2215-01 | Revenue Works - Drainage | महसुली कामे - मलनिस्सारण | Drainage |
| 2059-01 | Revenue Works - Roads | महसुली कामे - रस्ते | Roads |

*Note: Full budget head list to be imported from the DMC Accounts department.
This is representative seed data only.*

**Schema:**
```
BudgetHeads
├── Id                  UNIQUEIDENTIFIER PK
├── Code                NVARCHAR(50) UNIQUE
├── Name_En             NVARCHAR(300)
├── Name_Mr             NVARCHAR(300)
├── DepartmentId        UNIQUEIDENTIFIER FK → Departments (NULL = applicable to all)
├── FundTypeId          UNIQUEIDENTIFIER FK → FundTypes
├── AllocatedAmount     DECIMAL(18,2)       -- Total budget for current FY
├── FinancialYear       NVARCHAR(20)        -- e.g. "2025-26"
├── IsActive            BIT DEFAULT 1
├── IsDeleted           BIT DEFAULT 0
├── CreatedAt           DATETIME2
├── UpdatedAt           DATETIME2
```

**Budget validation:** When JE selects a budget head in Tab 6:
- `AvailableFund = AllocatedAmount - SUM(approved proposals against this head)`
- `Balance = AvailableFund - EstimatedCost`
- If `Balance < 0` → proposal gets **PARKED** (not rejected)

---

## 10. Designations

Officer designations used for user profiles and display.

| Designation (En) | Designation (Mr) |
|-------------------|-------------------|
| Junior Engineer | कनिष्ठ अभियंता |
| Technical Sanctioner | तांत्रिक मंजुरीदार |
| Assistant Engineer | सहाय्यक अभियंता |
| Sub Engineer | उपअभियंता |
| City Engineer | शहर अभियंता |
| Chief Account Officer | मुख्य लेखाधिकारी |
| Deputy Commissioner | उपायुक्त |
| Commissioner | आयुक्त |
| Auditor | लेखापरीक्षक |

---

## 11. Priority Levels

Priority assigned to a proposal by the JE.

| Priority (En) | Priority (Mr) | Display Colour |
|----------------|----------------|----------------|
| High | उच्च | `danger` (red) |
| Medium | मध्यम | `warning` (amber) |
| Low | कमी | `info` (blue) |

---

## 12. Document Types

Types of documents that can be uploaded/generated against a proposal.

| Type Code | Display Name (En) | Display Name (Mr) | Upload Tab |
|-----------|--------------------|--------------------|----|
| SITE_PHOTO | Site Photo | स्थळ फोटो | Tab 2 |
| FIELD_VISIT_REPORT | Field Visit Report | स्थळ भेट अहवाल | Tab 2 |
| ESTIMATE_SHEET | Estimate Sheet | अंदाजपत्रक | Tab 3 |
| RATE_ANALYSIS | Rate Analysis | दर विश्लेषण | Tab 3 |
| TS_ORDER | Technical Sanction Order | तांत्रिक मंजुरी आदेश | Tab 4 |
| ADMIN_NOTE | Administrative Note | प्रशासकीय टिपणी | Tab 5 |
| BUDGET_CERT | Budget Certificate | अर्थसंकल्प प्रमाणपत्र | Tab 6 |
| APPROVAL_ORDER | Approval Order (generated) | मंजुरी आदेश | Post-approval |
| OTHER | Other | इतर | Any |

> Document Types are **global** (no PalikaId) — they apply uniformly across
> all palikas.

---

## Master CRUD API Pattern

All masters follow a uniform REST pattern with **tenant scoping**:

```
GET    /api/masters/{entity}              → Paginated list (active only, filtered by user's PalikaId)
GET    /api/masters/{entity}/{id}         → Single record (must belong to user's Palika)
POST   /api/masters/{entity}              → Create (PalikaId auto-set from current user)
PUT    /api/masters/{entity}/{id}         → Update
DELETE /api/masters/{entity}/{id}         → Soft-delete

GET    /api/masters/{entity}/lookup       → Minimal id+name list for dropdowns (tenant-filtered)
```

**Lotus** gets unrestricted master CRUD including restore of soft-deleted
records via `/api/lotus/masters/{entity}/{id}/restore`. Lotus can
also manage masters across palikas.

---

## PalikaId Scoping Rules

| Scoped per Palika (has PalikaId FK) | Global / Shared (no PalikaId) |
|--------------------------------------|-------------------------------|
| Departments | SiteConditions |
| DeptWorkCategories (via Dept) | Priorities (enum, no table) |
| Zones | Document Types |
| Prabhags | |
| Designations | |
| RequestSources | |
| WorkExecutionMethods | |
| FundTypes | |
| BudgetHeads | |
| Users | |
| Proposals | |
| Notifications | |
| AuditTrail | |

---

## Cascading Dropdowns in UI

Several masters have parent-child relationships that drive cascading dropdowns:

| Parent | Child | UI Behaviour |
|--------|-------|--------------|
| Palika | Zone | User's palika → auto-filter zones |
| Zone | Prabhag | Select zone → filter prabhags |
| Palika | Department | User's palika → auto-filter departments |
| Department | DeptWorkCategory | Select dept → filter categories |
| Department + FundType + FY | BudgetHead | Select dept + fund type → filter budget heads |
