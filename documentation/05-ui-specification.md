# UI Specification & Mockups

## Design System

- **Framework**: Bootstrap 5.3 (CSS + JS bundle)
- **Accessibility**: WCAG 2.1 AA mandatory on every page
- **Languages**: Bilingual (English + Marathi), switchable via header toggle
- **Theme**: Clean government-style — blue primary (#0d6efd), white backgrounds,
  subtle grey borders, card-based layouts
- **Typography**: System font stack (Segoe UI / Noto Sans Devanagari for Marathi)
- **Responsive**: Mobile-first, works on tablet (primary use case) and desktop

---

## Global Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  HEADER                                                         │
│  ┌──────┐  धुळे महानगरपालिका                    🔔 3  🌐 मराठी │
│  │ Logo │  Dhule Municipal Corporation            [User ▼]      │
│  └──────┘                                                       │
├──────────┬──────────────────────────────────────────────────────┤
│ SIDEBAR  │  MAIN CONTENT AREA                                   │
│          │                                                      │
│ 📋 Dashboard │                                                  │
│ 📝 Proposals │                                                  │
│ 📊 My Tasks  │                                                  │
│ 🔔 Notifs    │                                                  │
│ ⚙️ Settings  │                                                  │
│          │                                                      │
│ [LOTUS]  │  (Role-based: only Lotus sees Lotus section)         │
│ 👥 Users │                                                      │
│ 📦 Masters │                                                    │
│ 📋 Audit  │                                                     │
│          │                                                      │
├──────────┴──────────────────────────────────────────────────────┤
│  FOOTER: © 2026 Dhule Municipal Corporation                     │
└─────────────────────────────────────────────────────────────────┘
```

### Header Components

| Element | Description |
|---------|-------------|
| Logo | Corporation emblem (configurable) |
| Title | Corporation name in Marathi + English |
| 🔔 Bell | Notification icon with unread count badge (red circle) |
| 🌐 Language | Toggle: English ↔ मराठी |
| User menu | Dropdown: Profile, Change Signature, Logout |

### Sidebar Navigation (role-dependent)

| Menu Item | JE | TS | AE/SE/CE | AccOfficer | DyCom | Comm | SC | Collector | Auditor | Lotus |
|-----------|----|----|----------|------------|-------|------|----|-----------|---------|-------|
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Create Proposal | ✅ | — | — | — | — | — | — | — | — | ✅ |
| My Proposals | ✅ | — | — | — | — | — | — | — | — | ✅ |
| Assigned Tasks | ✅ | ✅ | ✅ | — | — | — | — | — | — | ✅ |
| Pending Approvals | — | — | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ |
| All Proposals | — | — | — | — | — | ✅ | — | — | ✅ | ✅ |
| Parked Proposals | — | — | — | ✅ | — | — | — | — | — | ✅ |
| Notifications | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Audit Trail | — | — | — | — | — | ✅ | — | — | ✅ | ✅ |
| Users (Lotus) | — | — | — | — | — | — | — | — | — | ✅ |
| Masters (Lotus) | — | — | — | — | — | — | — | — | — | ✅ |
| Budget Heads (Lotus) | — | — | — | — | — | — | — | — | — | ✅ |
| Settings (Lotus) | — | — | — | — | — | — | — | — | — | ✅ |

---

## Page: Login

```
┌─────────────────────────────────────────┐
│                                         │
│         ┌──────────┐                    │
│         │   Logo   │                    │
│         └──────────┘                    │
│                                         │
│     धुळे महानगरपालिका                   │
│     Dhule Municipal Corporation         │
│                                         │
│     ┌───────────────────────────┐       │
│     │  Mobile Number            │       │
│     │  +91 ___________          │       │
│     └───────────────────────────┘       │
│                                         │
│     [Send OTP]                          │
│                                         │
│     ┌───────────────────────────┐       │
│     │  Enter OTP: _ _ _ _ _ _   │       │
│     └───────────────────────────┘       │
│                                         │
│     (For Lotus users only:)             │
│     ┌───────────────────────────┐       │
│     │  Password: ___________    │       │
│     └───────────────────────────┘       │
│                                         │
│     [Login]                             │
│                                         │
└─────────────────────────────────────────┘
```

---

## Page: Dashboard

Role-specific cards showing counts and quick actions.

### JE Dashboard

```
┌─────────────────────────────────────────────────────────────────┐
│  Dashboard                                                       │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  📝 Draft    │  │  🔄 In Progress│  │ ⏸️ Parked   │          │
│  │     5        │  │     12        │  │     2        │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  ↩️ Pushed   │  │  ✅ Approved  │  │  📋 Total    │          │
│  │  Back: 3     │  │     28        │  │     50        │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                   │
│  Recent Activity                                                  │
│  ┌────────────────────────────────────────────────────┐         │
│  │ DMC/2026/00042 — Road repair Prabhag 5 — AtCityEng │         │
│  │ DMC/2026/00041 — Drainage — FieldVisitCompleted    │         │
│  │ DMC/2026/00038 — PushedBack by DyCom              │         │
│  └────────────────────────────────────────────────────┘         │
│                                                                   │
│  [+ Create New Proposal]                                         │
└─────────────────────────────────────────────────────────────────┘
```

### Approver Dashboard (CE / Account / DyCom / Comm / SC / Collector)

```
┌─────────────────────────────────────────────────────────────────┐
│  Dashboard                                                       │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  ⏳ Pending  │  │  ✅ Approved  │  │  ↩️ Pushed   │          │
│  │  Approval: 8 │  │  By Me: 42   │  │  Back: 5     │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                   │
│  Pending for My Approval                                         │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ # │ Proposal No    │ Work Title        │ Amount │ Date   │   │
│  │ 1 │ DMC/2026/00042 │ Road repair W5    │ ₹4.5L  │ 20 Mar │   │
│  │ 2 │ DMC/2026/00039 │ Drainage clean    │ ₹1.2L  │ 18 Mar │   │
│  │ 3 │ DMC/2026/00035 │ Water pipeline    │ ₹22L   │ 15 Mar │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Page: Proposal List

```
┌─────────────────────────────────────────────────────────────────┐
│  My Proposals                                    [+ New Proposal] │
│                                                                   │
│  🔍 [Search by title/number____________]  [Dept ▼] [Stage ▼]    │
│                                          [Priority ▼] [Filter]   │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ # │ Number         │ Title          │ Dept    │ Stage     ││  │
│  │   │                │                │         │           ││  │
│  │ 1 │ DMC/2026/00042 │ Road repair W5 │ PWD     │ AtCE   🟡││  │
│  │ 2 │ DMC/2026/00041 │ Drainage clean │ Water   │ Draft  ⚪││  │
│  │ 3 │ DMC/2026/00038 │ Bridge work    │ PWD     │ Parked 🟠││  │
│  │ 4 │ DMC/2026/00035 │ Water pipeline │ Water   │ Aprvd  🟢││  │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ← 1 2 3 ... 10 →                                 Showing 1-10  │
└─────────────────────────────────────────────────────────────────┘
```

Stage badge colours:
- ⚪ Draft, PushedBack — grey
- 🔵 FieldVisit*, Estimate*, TS* — blue
- 🟡 At* (approval stages) — yellow
- 🟠 Parked — orange
- 🟢 Approved — green
- 🔴 Cancelled — red

---

## Page: Proposal Form — 6-Tab View

This is the core page. Shows all 6 tabs in a horizontal tab bar.

### Tab Bar

```
┌─────────────────────────────────────────────────────────────────┐
│  DMC/2026/00042    Stage: AtCityEngineer    Owner: Shri Patil   │
│                                                                   │
│  ┌──────────┬────────────┬──────────┬──────┬────────┬────────┐  │
│  │ Proposal │ Field Visit│ Estimate │  TS  │ PRAMA  │ Budget │  │
│  │    ✅    │    ✅      │   ✅     │  ✅  │  ✅    │  ✅   │  │
│  │          │            │          │      │        │  🔒    │  │
│  └──────────┴────────────┴──────────┴──────┴────────┴────────┘  │
│                                                                   │
│  [...Tab Content Area...]                                        │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

Tab indicators:
- ✅ Completed (data filled)
- ✏️ Editable (current user owns this tab)
- 🔒 Locked (view-only for this user)
- ⏳ Not started (placeholder)
- 🟠 Parked

---

### Tab 1 — Proposal (✏️ when owned by JE)

```
┌─────────────────────────────────────────────────────────────────┐
│  प्रस्ताव / Proposal                                            │
│                                                                   │
│  Department *         Dept Work Category *                       │
│  [▼ Select Dept____] [▼ Select Category__]                      │
│                                                                   │
│  ── Work Location ──────────────────────────────────────         │
│  Zone *              Prabhag No *         Area                   │
│  [▼ Select Zone__]  [▼ Prabhag (filtered)] [________]           │
│                                                                   │
│  Location Upload     [📎 Choose File]  map.png ✕                │
│                                                                   │
│  Location Address *                                              │
│  ┌─────────────────────────────────────────────────┐            │
│  │  Enter manually...                               │            │
│  └─────────────────────────────────────────────────┘            │
│                                                                   │
│  Work Title * (English)                                          │
│  [Road repair near Nashik Naka junction_____________]            │
│  कामाचे शीर्षक * (मराठी)                                        │
│  [नाशिक नाका जंक्शन जवळ रस्ता दुरुस्ती______________]          │
│                                                                   │
│  Work Description * (English)                                    │
│  ┌─────────────────────────────────────────────────┐            │
│  │  Detailed description of the proposed work...    │            │
│  │                                                   │            │
│  └─────────────────────────────────────────────────┘            │
│  कामाचे वर्णन * (मराठी)                                         │
│  ┌─────────────────────────────────────────────────┐            │
│  │  प्रस्तावित कामाचे तपशीलवार वर्णन...              │            │
│  └─────────────────────────────────────────────────┘            │
│                                                                   │
│  ── Source of Request ──────────────────────────────────         │
│  Source *                                                        │
│  [▼ Citizen / MLA / MP / Commissioner]                          │
│                                                                   │
│  ── Requestor Details ──────────────────────────────────        │
│  Name                 Mobile              Address                │
│  [_____________]      [_____________]     [_____________]        │
│  Designation          Organisation                               │
│  [_____________]      [_____________]                            │
│                                                                   │
│  Priority *                                                      │
│  [▼ High / Medium / Low]                                        │
│                                                                   │
│  ── Documents ──────────────────────────────────────────         │
│  Doc Name: [_________]  [📎 Choose File]  [+ Add More]          │
│  ┌──────────────────────────────────────┐                       │
│  │  📄 site_photo.jpg     2.1 MB   ✕    │                       │
│  │  📄 request_letter.pdf  500 KB   ✕    │                       │
│  └──────────────────────────────────────┘                       │
│                                                                   │
│  [Save Draft]  [Save & Generate Proposal No →]                  │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

### Tab 2 — Field Visit (✏️ when owned by assigned JE)

```
┌─────────────────────────────────────────────────────────────────┐
│  स्थळ भेट / Field Visit                                        │
│                                                                   │
│  ── Visit History ──────────────────────────────────────        │
│  ┌────────────────────────────────────────────────────┐         │
│  │ # │ Date       │ Inspector    │ Condition │ Status │         │
│  │ 1 │ 15-Mar-26  │ Rajesh JE    │ Bad       │ ✅ Done│         │
│  │ 2 │ 20-Mar-26  │ Suresh JE    │ OK        │ ✏️ New │         │  
│  └────────────────────────────────────────────────────┘         │
│  [+ Assign New Field Visit]                                      │
│                                                                   │
│  ── Current Visit (#2) ────────────────────────────────         │
│                                                                   │
│  Upload Inspection PDF    [📎 Choose File]                      │
│                                                                   │
│  Inspection By:    Suresh Kumar (auto-filled)                    │
│  Inspection Date:  [📅 20-Mar-2026]                              │
│                                                                   │
│  Site Condition *  [▼ Worse / Bad / OK / Good / Better]         │
│                                                                   │
│  Problem Description * (English)                                 │
│  ┌─────────────────────────────────────────────────┐            │
│  │  Road surface severely damaged with potholes... │            │
│  └─────────────────────────────────────────────────┘            │
│  समस्येचे वर्णन * (मराठी)                                       │
│  ┌─────────────────────────────────────────────────┐            │
│  │  रस्त्याची पृष्ठभाग खड्ड्यांनी गंभीरपणे खराब... │            │
│  └─────────────────────────────────────────────────┘            │
│                                                                   │
│  Measurements (Optional)                                         │
│  ┌─────────────────────────────────────────────────┐            │
│  │  Length: 200m, Width: 8m, Depth of potholes...  │            │
│  └─────────────────────────────────────────────────┘            │
│                                                                   │
│  ── GPS Location ────────────────────                            │
│  Latitude:  [20.9042___]    Longitude: [74.7749___]             │
│  [📍 Get Current Location]                                      │
│                                                                   │
│  Remark                                                          │
│  [Immediate attention required____________________]              │
│                                                                   │
│  ── Site Photos ────────────────────                             │
│  [📎 Upload Photos (multiple)]                                  │
│  ┌──────┐ ┌──────┐ ┌──────┐                                    │
│  │ 📷   │ │ 📷   │ │ 📷   │                                    │
│  │photo1│ │photo2│ │photo3│                                    │
│  │  ✕   │ │  ✕   │ │  ✕   │                                    │
│  └──────┘ └──────┘ └──────┘                                    │
│                                                                   │
│  Recommendation *                                                │
│  ┌─────────────────────────────────────────────────┐            │
│  │  Recommend immediate road resurfacing...         │            │
│  └─────────────────────────────────────────────────┘            │
│                                                                   │
│  [Sign & Save ✍️]                                               │
│  (PNG signature will be applied)                                 │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

### Tab 3 — Estimate (✏️ when owned by JE; approval section when AE/SE/CE)

```
┌─────────────────────────────────────────────────────────────────┐
│  अंदाजपत्रक / Estimate                                         │
│                                                                   │
│  ── Prepared by JE ────────────────────────────────────         │
│                                                                   │
│  Upload Estimate Copy (PDF) *   [📎 Choose File]                │
│  📄 estimate_prabhag5_road.pdf  1.4 MB                          │
│                                                                   │
│  Estimated Cost *: ₹ [4,50,000_____]                            │
│                                                                   │
│  JE Signature:  ✍️ [Apply Signature]                            │
│                 ┌────────────┐                                   │
│                 │  (signed)  │                                   │
│                 └────────────┘                                   │
│                                                                   │
│  Send to: [▼ AE / SE / City Engineer]  →  [▼ Select officer]   │
│  [Send for Approval →]                                           │
│                                                                   │
│  ── Approval Section (visible to AE/SE/CE) ────────────         │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  Status: ⏳ Pending Approval                        │        │
│  │                                                      │        │
│  │  ┌────────────────────────────────────────────────┐ │        │
│  │  │  DISCLAIMER (Marathi text - read only)          │ │        │
│  │  │  प्रस्तावित कामगिरी मी तपासली असून...           │ │        │
│  │  └────────────────────────────────────────────────┘ │        │
│  │                                                      │        │
│  │  ☐ मी सहमत आहे / I agree *                         │        │
│  │                                                      │        │
│  │  Other Opinion:                                      │        │
│  │  ┌──────────────────────────────────────────┐       │        │
│  │  │                                           │       │        │
│  │  └──────────────────────────────────────────┘       │        │
│  │                                                      │        │
│  │  Name: [▼ Select officer_____]                      │        │
│  │  Signature: ✍️ [Apply Signature]                    │        │
│  │                                                      │        │
│  │  [✅ Approve]     [↩️ Return with Query]            │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
│  ── Query History (if any returns) ────────                      │
│  ┌────────────────────────────────────────────────────┐         │
│  │ 18-Mar: AE Patil — "Please revise line item 3"    │         │
│  │ 19-Mar: JE Rajesh — Revised and resent             │         │
│  └────────────────────────────────────────────────────┘         │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

### Tab 4 — Technical Sanction (✏️ when owned by TS; signing by AE/SE/CE)

```
┌─────────────────────────────────────────────────────────────────┐
│  तांत्रिक मंजुरी / Technical Sanction                           │
│                                                                   │
│  Upload TS Document (PDF)           [📎 Choose File]            │
│  — OR —                                                          │
│  Upload Outside Approval Letter     [📎 Choose File]            │
│                                                                   │
│  Description (English)                                           │
│  [Technical sanction for road resurfacing work_________]         │
│  वर्णन (मराठी)                                                   │
│  [रस्ता पुनर्पृष्ठभागाच्या कामासाठी तांत्रिक मंजुरी___]        │
│                                                                   │
│  TS Number *:     [TS/DMC/2026/0015_____]                       │
│  TS Date *:       [📅 22-Mar-2026]                               │
│  TS Amount *:     ₹ [4,50,000__________]                        │
│                                                                   │
│  ── Sanctioned By ────────────────────────                       │
│  Name:         [Shri. V.K. Deshmukh_____]                       │
│  Department:   [Public Works___________]                        │
│  Designation:  [Executive Engineer______]                       │
│                                                                   │
│  ── AE/SE/City Engineer Signs Both ─────────────                │
│  (Signs Estimate + TS Order together)                            │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  Signing Officer: [▼ Select AE/SE/CE_____]          │        │
│  │  Signature: ✍️ [Apply Signature]                    │        │
│  │  [✅ Sign & Complete]                               │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

### Tab 5 — PRAMA (✏️ when owned by creator JE)

```
┌─────────────────────────────────────────────────────────────────┐
│  प्रशासकीय मान्यता / PRAMA (Administrative Approval Note)       │
│                                                                   │
│  Date:         26-Mar-2026 (auto)                                │
│  Proposal No:  DMC/2026/00042 (auto)                             │
│  Department:   Public Works Dept (fetched from Tab 1)            │
│  Dept User:    [▼ Select dept user_____]                        │
│                                                                   │
│  Fund Type *:  [▼ MNP / State / Central / DPDC]                │
│  Fund Head *:  [▼ (filtered by Fund Type)______]                │
│  Fund Year:    [▼ 2025-2026]                                    │
│                                                                   │
│  ── Auto-Fetched Data ──────────────────────────────────        │
│                                                                   │
│  Proposal Title:   Road repair near Nashik Naka (from Tab 1)    │
│                                                                   │
│  संदर्भ / References *                                           │
│  ┌─────────────────────────────────────────────────┐            │
│  │  (JE can add references, edit, save & update)    │            │
│  │  महासभा ठराव क्र. ४२/२०२५ दि. १२/०१/२०२६...   │            │
│  └─────────────────────────────────────────────────┘            │
│                                                                   │
│  प्रभाग:  Prabhag 5 — Nashik Naka (from Tab 1)                   │
│                                                                   │
│  Proposal Detail:                                                │
│  ┌─────────────────────────────────────────────────┐            │
│  │  (Fetched from Tab 1, plus JE can add more)      │            │
│  └─────────────────────────────────────────────────┘            │
│                                                                   │
│  ── Estimate (from Tab 3) ──────────────────────────            │
│  अंदाजी खर्च: ₹ 4,50,000                                       │
│  📄 [View Estimate PDF]                                         │
│                                                                   │
│  ── Field Visit (from Tab 2) ──────────────────────             │
│  Field Visit Done: ✅ Yes (2 visits)                             │
│  📄 [View Visit 1 PDF]  📄 [View Visit 2 PDF]                  │
│  📷 [View Photos]                                               │
│                                                                   │
│  ── Technical Sanction (from Tab 4) ──────────────              │
│  TS Date:    22-Mar-2026                                         │
│  TS Number:  TS/DMC/2026/0015                                    │
│  TS Amount:  ₹ 4,50,000                                         │
│  📄 [View TS Document]                                          │
│                                                                   │
│  [Save PRAMA]                                                    │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

### Tab 6 — Budget (✏️ when owned by creator JE)

```
┌─────────────────────────────────────────────────────────────────┐
│  अर्थसंकल्प / Budget & Compliance                               │
│                                                                   │
│  ── Work Execution ──────────────────────                        │
│  कामगिरी कशी ठरलेली *                                           │
│  Work Execution Method: [▼ Tendering option________]            │
│  कामाकडे / Duration:   [___] days                               │
│  Tender Verified: [▼ Yes / No]                                  │
│                                                                   │
│  ── Budget Details ──────────────────────────────────           │
│  लेखाशीर्ष / Budget Head:  [▼ (auto from dept)_____]           │
│                                                                   │
│  ┌─────────────────────────────────────┐                        │
│  │ Allocated Fund:        ₹ 1,00,00,000│ (from master)          │
│  │ Currently Available:   ₹   19,00,000│ (from master)          │
│  │ Old Expenditure:       ₹   90,00,000│ (manual input)         │
│  │ Estimated Cost:        ₹    4,50,000│ (from Tab 3)           │
│  │ ─────────────────────────────────── │                        │
│  │ Balance:               ₹   14,50,000│ ✅                     │
│  │                                      │                        │
│  │ (If negative: ⚠️ INSUFFICIENT FUNDS │                        │
│  │  Proposal will be PARKED)            │                        │
│  └─────────────────────────────────────┘                        │
│                                                                   │
│  Account Serial No: [▼ Select______]                            │
│                                                                   │
│  ── Compliance ──────────────────────────                        │
│  ┌─────────────────────────────────────────────────┐            │
│  │  Layout plan, orders, compliance notes...        │            │
│  └─────────────────────────────────────────────────┘            │
│                                                                   │
│  ── Approval Authority (Auto-determined) ──────────             │
│  ┌──────────────────────────────────────────┐                   │
│  │  Estimated Cost: ₹ 4,50,000              │                   │
│  │  Slab: 3L – 24L                          │                   │
│  │  Final Authority: Commissioner             │                   │
│  │  Chain: CE → Account → DyCom → Commissioner│                   │
│  └──────────────────────────────────────────┘                   │
│                                                                   │
│  [Save Budget]   [Submit for Approval →]                        │
│  (Submit generates consolidated PDF and enters formal chain)     │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Page: Approval Console (shown to formal chain approvers)

```
┌─────────────────────────────────────────────────────────────────┐
│  Approval Console — DMC/2026/00042                               │
│                                                                   │
│  ── Consolidated PDFs from Prior Stages ─────────               │
│  📄 JE Consolidated PDF (6 tabs signed)        [View] [Download]│
│  📄 City Engineer Approval PDF                  [View] [Download]│
│  📄 Account Officer Approval PDF                [View] [Download]│
│                                                                   │
│  ── All 6 Tabs (Read-Only) ───────────                          │
│  [Tab bar — all locked, view-only]                               │
│                                                                   │
│  ── My Approval Section ─────────────────────────               │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  DISCLAIMER                                          │        │
│  │  ┌────────────────────────────────────────────────┐ │        │
│  │  │  (Role-specific Marathi text — read only)       │ │        │
│  │  │  प्रस्तावित कामगिरी मी तपासली असून...           │ │        │
│  │  │  ...                                            │ │        │
│  │  └────────────────────────────────────────────────┘ │        │
│  │                                                      │        │
│  │  ☐ मी सहमत आहे / I agree *                         │        │
│  │                                                      │        │
│  │  अन्य अभिप्राय / Other Opinion:                    │        │
│  │  ┌──────────────────────────────────────────┐       │        │
│  │  │                                           │       │        │
│  │  └──────────────────────────────────────────┘       │        │
│  │                                                      │        │
│  │  नाव / Name:  [▼ Select your name____]             │        │
│  │  स्वाक्षरी / Signature: ✍️ [Apply Signature]       │        │
│  │                                                      │        │
│  │  ┌───────────────┐  ┌──────────────────────┐       │        │
│  │  │  ✅ Approve    │  │  ↩️ Push Back to JE  │       │        │
│  │  └───────────────┘  └──────────────────────┘       │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
│  ── Push Back (shown when Push Back clicked) ──                  │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  Reason / Note * (mandatory):                        │        │
│  │  ┌──────────────────────────────────────────┐       │        │
│  │  │  Explain why you are pushing back...      │       │        │
│  │  └──────────────────────────────────────────┘       │        │
│  │  [Confirm Push Back]  [Cancel]                      │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Page: Notifications

```
┌─────────────────────────────────────────────────────────────────┐
│  🔔 Notifications                              [Mark All Read]  │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ 🔵 Field Visit assigned to you                    5 min ago│ │
│  │    DMC/2026/00042 — Road repair Prabhag 5                  │ │
│  ├────────────────────────────────────────────────────────────┤ │
│  │ 🔵 Proposal pushed back by DyCom                 2 hrs ago│ │
│  │    DMC/2026/00038 — "Please revise budget head"            │ │
│  ├────────────────────────────────────────────────────────────┤ │
│  │ ⚪ Estimate approved by City Engineer              1 day ago│ │
│  │    DMC/2026/00041 — Drainage cleaning                      │ │
│  ├────────────────────────────────────────────────────────────┤ │
│  │ ⚪ Proposal approved (Final)                      2 days ago│ │
│  │    DMC/2026/00035 — Water pipeline                         │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                   │
│  ← 1 2 3 →                                                      │
└─────────────────────────────────────────────────────────────────┘
```

🔵 = unread, ⚪ = read. Click any notification → navigates to relevant proposal.

---

## Page: Parked Proposals (Account Officer view)

```
┌─────────────────────────────────────────────────────────────────┐
│  ⏸️ Parked Proposals                                            │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ # │ Proposal No    │ Title        │ Budget Head │ Deficit│   │
│  │ 1 │ DMC/2026/00038 │ Bridge work  │ PWD-Road    │ -₹2.1L│   │
│  │ 2 │ DMC/2026/00033 │ Wall repair  │ PWD-Build   │ -₹80K │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
│  Select proposal → [Unpark] (only if budget head now has funds) │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Assign Field Visit Modal

```
┌─────────────────────────────────────────┐
│  Assign Field Visit                      │
│                                          │
│  Assign to:                              │
│  ○ Self (Rajesh Kumar, JE)               │
│  ○ Another JE from department:           │
│    [▼ Select JE from dept list____]      │
│                                          │
│  [Assign]  [Cancel]                      │
└─────────────────────────────────────────┘
```

---

## Colour Palette

| Use | Colour | Bootstrap Class |
|-----|--------|----------------|
| Primary actions | #0d6efd | `btn-primary` |
| Approved/Success | #198754 | `btn-success`, `badge bg-success` |
| Push-back/Warning | #ffc107 | `btn-warning`, `badge bg-warning` |
| Parked | #fd7e14 | `badge bg-orange` (custom) |
| Error/Cancel | #dc3545 | `btn-danger`, `badge bg-danger` |
| Neutral/Draft | #6c757d | `badge bg-secondary` |
| Info/In-progress | #0dcaf0 | `badge bg-info` |
| Backgrounds | #f8f9fa | `bg-light` |
| Card borders | #dee2e6 | Bootstrap default |
