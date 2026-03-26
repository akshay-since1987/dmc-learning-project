# Workflow & State Machine

## Overview

The proposal lifecycle has **two distinct phases**:

1. **Preparation Phase** — JE-owned, multi-step form filling with task delegation
2. **Approval Phase** — formal chain with disclaimers, signatures, and PDFs

---

## Proposal Stages (Enum: `ProposalStage`)

```
Draft                    -- Tab 1 created, proposal number generated
FieldVisitAssigned       -- Field visit assigned to a JE
FieldVisitInProgress     -- Assigned JE is doing inspection
FieldVisitCompleted      -- Field visit signed & saved
EstimateDraft            -- JE preparing estimate (Tab 3)
EstimateSentForApproval  -- Sent to AE/SE/CE for review
EstimateReturnedWithQuery -- AE/SE/CE sent back with query
EstimateApproved         -- AE/SE/CE approved the estimate
TSDraft                  -- TS role filling Tab 4
TSPending                -- Sent to AE/SE/CE for signing
TSApproved               -- AE/SE/CE signed estimate + TS order
PramaFilling             -- JE compiling PRAMA (Tab 5)
BudgetFilling            -- JE filling Budget (Tab 6)
Parked                   -- Insufficient funds — waiting
ReadyForSubmission       -- All 6 tabs complete, JE can submit
Submitted                -- JE submitted — enters formal chain
AtCityEngineer           -- Formal approval: City Engineer
AtAccountOfficer         -- Formal approval: Account Officer
AtDyCommissioner         -- Formal approval: Deputy Commissioner
AtCommissioner           -- Formal approval: Commissioner (3L+)
AtStandingCommittee      -- Formal approval: Sthai Samiti (24-25L)
AtCollector              -- Formal approval: Collector (25L+)
Approved                 -- Final approval granted
PushedBack               -- Pushed back to JE (from any approver)
Cancelled                -- Cancelled (admin only)
```

---

## Phase 1: Preparation Flow

```
JE creates proposal (Tab 1)
│   Stage: Draft
│   Owner: Creator JE
│
├── Save Tab 1 → Proposal Number auto-generated
│
├── JE assigns field visit (self or another JE from same dept)
│   Stage: FieldVisitAssigned
│   Owner: Assigned JE
│   📧 Notification → Assigned JE
│
├── Assigned JE starts inspection
│   Stage: FieldVisitInProgress
│
├── Assigned JE fills Tab 2, signs & saves
│   Stage: FieldVisitCompleted
│   Owner: Back to Creator JE
│   📧 Notification → Creator JE
│   📄 Field Visit PDF generated
│
├── Creator JE may add MORE field visits (separate records)
│   (Each follows the same assign → inspect → complete cycle)
│
├── Creator JE prepares Estimate (Tab 3)
│   Stage: EstimateDraft
│   Owner: Creator JE
│
├── JE uploads estimate PDF, signs, sends to AE/SE/CE
│   Stage: EstimateSentForApproval
│   Owner: Selected AE/SE/CE
│   📧 Notification → AE/SE/CE
│
├── AE/SE/CE reviews estimate
│   ├── Return with Query:
│   │   Stage: EstimateReturnedWithQuery
│   │   Owner: Creator JE
│   │   📧 Notification → Creator JE
│   │   (JE corrects → resends → EstimateSentForApproval again)
│   │
│   └── Approve (with disclaimer + signature):
│       Stage: EstimateApproved
│       Owner: Creator JE
│       📄 Estimate approval PDF generated
│       📧 Notification → Creator JE
│
├── TS fills Technical Sanction (Tab 4)
│   Stage: TSDraft → TSPending
│   Owner: TS role → AE/SE/CE
│   (TS fills form → AE/SE/CE signs both Estimate + TS Order)
│
├── AE/SE/CE signs TS
│   Stage: TSApproved
│   Owner: Back to Creator JE
│   📄 TS PDF generated
│   📧 Notification → Creator JE (with TS No, Date, Amount)
│
├── Creator JE fills PRAMA (Tab 5)
│   Stage: PramaFilling
│   Owner: Creator JE
│   (Auto-fetches P1–P4 data; JE adds references & notes)
│
├── Creator JE fills Budget (Tab 6)
│   Stage: BudgetFilling
│   Owner: Creator JE
│   System computes: Balance = Available Fund − Estimated Cost
│   ├── Balance ≥ 0 → Stage: ReadyForSubmission
│   └── Balance < 0 → Stage: Parked
│       📧 Notification → Creator JE + Account Officer
│       (Manual unpark by Account Officer when funds available)
│       (On unpark → resumes at BudgetFilling, owner notified)
│
└── JE submits final form
    Stage: Submitted
    📄 Consolidated PDF generated (all 6 tabs, signed)
    → Enters Formal Approval Chain
```

---

## Phase 2: Formal Approval Chain

The chain length depends on the estimated cost:

```
                    ┌─────────────────────────────────────────────────────────┐
                    │               APPROVAL CHAIN                            │
                    │                                                         │
Submitted ──────────┤                                                         │
                    │  AtCityEngineer ─── approve ──→ AtAccountOfficer        │
                    │       │                              │                  │
                    │       │ pushback                     │ approve          │
                    │       ↓                              ↓                  │
                    │    PushedBack                AtDyCommissioner            │
                    │    (→ JE)                         │                     │
                    │                            approve │ pushback           │
                    │                                    ↓    ↓              │
                    │                              ┌─────────────┐           │
                    │                              │  0-3L: DONE │           │
                    │                              │  (Approved)  │           │
                    │                              └─────────────┘           │
                    │                                    │                    │
                    │                          (if > 3L) │                    │
                    │                                    ↓                    │
                    │                            AtCommissioner               │
                    │                                    │                    │
                    │                              ┌─────────────┐           │
                    │                              │ 3-24L: DONE │           │
                    │                              │  (Approved)  │           │
                    │                              └─────────────┘           │
                    │                                    │                    │
                    │                        (if > 24L)  │                    │
                    │                                    ↓                    │
                    │                          AtStandingCommittee            │
                    │                                    │                    │
                    │                              ┌─────────────┐           │
                    │                              │24-25L: DONE │           │
                    │                              │  (Approved)  │           │
                    │                              └─────────────┘           │
                    │                                    │                    │
                    │                         (if > 25L) │                    │
                    │                                    ↓                    │
                    │                              AtCollector                │
                    │                                    │                    │
                    │                              ┌─────────────┐           │
                    │                              │  25L+: DONE │           │
                    │                              │  (Approved)  │           │
                    │                              └─────────────┘           │
                    └─────────────────────────────────────────────────────────┘
```

### Approval Slab Routing

| Estimated Cost | Full Chain | Final Authority |
|---------------|-----------|----------------|
| ₹0 – ₹3,00,000 | CE → Account → **DyCom** | Deputy Commissioner |
| ₹3,00,001 – ₹24,00,000 | CE → Account → DyCom → **Commissioner** | Commissioner |
| ₹24,00,001 – ₹25,00,000 | CE → Account → DyCom → Commissioner → **Sthai Samiti** | Standing Committee |
| ₹25,00,001+ | CE → Account → DyCom → Commissioner → Sthai Samiti → **Collector** | Collector |

---

## Approval Stage — Common Pattern

Every formal approver follows this sequence:

```
1. View all 6 tabs (read-only) + all prior signed PDFs
2. Read role-specific Marathi disclaimer
3. Check mandatory disclaimer checkbox
4. Optionally type अन्य अभिप्राय (other opinion)
5. Select their name from dropdown
6. Apply PNG signature
7. Click [Approve] or [Push Back]
   - Approve: Stage advances; consolidated PDF generated; next owner notified
   - Push Back: Mandatory note; returns to JE as PushedBack; JE notified
```

---

## Push-Back Rules

| Rule | Detail |
|------|--------|
| Who can push back | Only the current form owner |
| Target | Always returns to Creator JE |
| Requires | Mandatory note/reason from the current owner |
| Does NOT require | Disclaimer checkbox |
| After push-back | Proposal stage = `PushedBack`, `PushBackCount` incremented |
| JE re-submission | JE corrects issue → re-submits → enters approval chain from begin |

---

## Parking Rules

| Rule | Detail |
|------|--------|
| Trigger | Balance (Available − Estimated) is negative when JE fills Tab 6 |
| Effect | Proposal moves to `Parked` state |
| Who is notified | Creator JE + Account Officer |
| Resume | Manual — Account Officer adds funds to budget head, manually unparks |
| Resume target | Proposal returns to exact stage it was parked at (`BudgetFilling`) |
| Who is notified on unpark | The current owner gets notification |

---

## PDF Generation on Ownership Transfer

```
Tab 1 complete → (no PDF yet, just data)
Tab 2 Sign & Save → 📄 Field Visit PDF
Tab 3 Approved → 📄 Estimate Approval PDF
Tab 4 Signed → 📄 TS PDF
Tab 5 complete → 📄 PRAMA PDF
Tab 6 complete → 📄 Budget PDF

JE Submits → 📄 Consolidated PDF (all 6 tabs combined, JE-signed)

City Engineer approves → 📄 CE Approval PDF → Consolidated = JE + CE
Account Officer approves → 📄 AO Approval PDF → Consolidated = JE + CE + AO
DyCom approves → 📄 DyCom PDF → Consolidated = JE + CE + AO + DyCom
Commissioner approves → 📄 Commissioner PDF → add to consolidated
Standing Committee approves → 📄 StCom PDF → add to consolidated
Collector approves → 📄 Collector PDF → add to consolidated

Each new owner receives ALL previously signed PDFs before making their decision.
```

---

## Stage Ownership Map

| Stage | Owner |
|-------|-------|
| Draft | Creator JE |
| FieldVisitAssigned | Assigned JE |
| FieldVisitInProgress | Assigned JE |
| FieldVisitCompleted | Creator JE |
| EstimateDraft | Creator JE |
| EstimateSentForApproval | Selected AE/SE/CE |
| EstimateReturnedWithQuery | Creator JE |
| EstimateApproved | Creator JE |
| TSDraft | TS user |
| TSPending | AE/SE/CE |
| TSApproved | Creator JE |
| PramaFilling | Creator JE |
| BudgetFilling | Creator JE |
| Parked | Creator JE (frozen) |
| ReadyForSubmission | Creator JE |
| Submitted | (system — transitioning) |
| AtCityEngineer | City Engineer |
| AtAccountOfficer | Account Officer |
| AtDyCommissioner | Deputy Commissioner |
| AtCommissioner | Commissioner |
| AtStandingCommittee | Standing Committee member |
| AtCollector | Collector |
| Approved | — (final) |
| PushedBack | Creator JE |
| Cancelled | — (admin) |
