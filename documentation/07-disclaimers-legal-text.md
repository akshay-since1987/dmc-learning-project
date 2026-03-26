# Disclaimers & Legal Text

## Overview

Every approver in the formal approval chain must accept a **mandatory
role-specific Marathi disclaimer** before they can approve a proposal. The
checkbox is enforced both in the UI (button disabled until checked) and in the
backend (API rejects approval if `DisclaimerAccepted = false`).

---

## Disclaimer Text by Role

### 1. City Engineer (शहर अभियंता)

**Marathi:**
> शहर अभियंता यांचे अभिप्राय - प्रस्तावित कामगिरी मी तपासली असून या
> कामगिरीद्वारे कोणत्याही कायदेशीर तरतुदी/वित्तीय तरतुदी/शासन निर्णय/शासन
> निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे उल्लंघन होत नाही.
> या कामगिरीद्वारे कोणत्याही कायदेशीर तरतुदी/वित्तीय तरतुदी/शासन निर्णय/शासन
> निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे उल्लंघन होत असल्याचे
> निदर्शनास आल्यास त्यास मी जबाबदार राहील.

**English Translation (for reference only — not shown in UI):**
> City Engineer's opinion — I have inspected the proposed work and confirm that
> this work does not violate any legal provisions, financial provisions,
> government decisions, government directives, court orders, or court directives.
> If any such violation is later found through this work, I shall be personally
> responsible.

**Key characteristics:**
- Personal accountability clause
- Covers: legal, financial, government, judicial compliance
- Self-responsibility if violations found later

**Fields after disclaimer:**
- शहर अभियंता यांचे अन्य अभिप्राय: `[free text]`
- शहर अभियंता - नाव: `[select dropdown]`
- शहर अभियंता स्वाक्षरी: `[DSC/PNG signature]`

---

### 2. Chief Account Officer (मुख्य लेखाधिकारी)

**Marathi:**
> मुख्य लेखाधिकारी यांचे अभिप्राय - प्रस्तावित कामगिरी मी तपासली असून या
> कामगिरीसाठी प्रस्तावित लेखाशीर्ष योग्य आहे. या प्रस्तावातून कोणत्याही
> लेखाविषयक बाबींचे उल्लंघन होणार नाही.

**English Translation:**
> Chief Account Officer's opinion — I have verified the proposed work and confirm
> that the proposed budget head is appropriate for this work. No accounting
> provisions will be violated through this proposal.

**Key characteristics:**
- Budget head correctness verification
- Accounting compliance focus
- No personal liability clause (unlike CE/DyCom)

**Fields after disclaimer:**
- मुख्य लेखाधिकारी यांचे अन्य अभिप्राय: `[free text]`
- मुख्य लेखाधिकारी - नाव: `[select dropdown]`
- मुख्य लेखाधिकारी स्वाक्षरी: `[DSC/PNG signature]`

---

### 3. Deputy Commissioner (उपायुक्त)

**Marathi:**
> उपायुक्त यांचे अभिप्राय - प्रस्तावित कामगिरी मी तपासली असून या
> कामगिरीद्वारे कोणत्याही कायदेशीर तरतुदी/वित्तीय तरतुदी/शासन निर्णय/शासन
> निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे उल्लंघन होत नाही.
> या कामगिरीद्वारे कोणत्याही कायदेशीर तरतुदी/वित्तीय तरतुदी/शासन निर्णय/शासन
> निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे उल्लंघन होत असल्याचे
> निदर्शनास आल्यास त्यास मी जबाबदार राहील.

**English Translation:**
> Deputy Commissioner's opinion — I have inspected the proposed work and confirm
> that this work does not violate any legal provisions, financial provisions,
> government decisions, government directives, court orders, or court directives.
> If any such violation is later found through this work, I shall be personally
> responsible.

**Key characteristics:**
- Same structure as City Engineer disclaimer
- Personal accountability clause present
- Full legal/financial/judicial compliance

**Fields after disclaimer:**
- उपायुक्त यांचे अन्य अभिप्राय: `[free text]`
- उपायुक्त - नाव: `[select dropdown]`
- उपायुक्त स्वाक्षरी: `[DSC/PNG signature]`

---

### 4. Commissioner (आयुक्त)

**Marathi:**
> आयुक्त यांचे अभिप्राय - सदर प्रस्तावातून कोणत्याही कायदेशीर तरतुदी/वित्तीय
> तरतुदी/शासन निर्णय/शासन निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे
> उल्लंघन होत नाही याबाबत सदर करणाऱ्या सर्व अधिकाऱ्यांनी नमूद केले आहे. सदर
> करणाऱ्या सर्व अधिकाऱ्यांनी कोणतीही कायदेशीर अडचण दर्शविलेली नसल्याने
> त्यांच्या शिफारशीप्रमाणे प्रस्ताव मान्य करण्यात येत आहे.

**English Translation:**
> Commissioner's opinion — All presenting officers have confirmed that this
> proposal does not violate any legal provisions, financial provisions, government
> decisions, government directives, court orders, or court directives. Since all
> presenting officers have indicated no legal obstacles, the proposal is being
> approved as per their recommendations.

**Key characteristics:**
- Collective accountability — relies on prior officers' confirmations
- Approval "as per their recommendations"
- No personal liability clause — responsibility is on presenting officers
- This is the culminating approval text

**Fields after disclaimer:**
- आयुक्त यांचे अन्य अभिप्राय: `[free text]`
- आयुक्त - नाव: `[select dropdown]`  *(Note: no separate नाव select — Commissioner is known)*
- आयुक्त स्वाक्षरी: `[DSC/PNG signature]`

---

### 5. Standing Committee (स्थायी समिती) — TBD

Disclaimer text to be provided. Will follow a similar pattern. Expected to be a
committee resolution-style approval rather than individual officer disclaimer.

---

### 6. Collector (जिल्हाधिकारी) — TBD

Disclaimer text to be provided. Expected to be a formal government approval text.

---

### 7. Estimate Approval (AE/SE/City Engineer)

The estimate approval stage (Tab 3) also requires a disclaimer. Uses the
**Account Officer** style disclaimer adapted for engineering review:

**Marathi:**
> प्रस्तावित कामगिरी मी तपासली असून या कामगिरीसाठी प्रस्तावित लेखाशीर्ष
> योग्य आहे. या प्रस्तावातून कोणत्याही लेखाविषयक बाबींचे उल्लंघन होणार नाही.

*(Same as Account Officer — confirms the estimate and budget head are appropriate)*

---

## UI Implementation Pattern

Every disclaimer section in the UI follows this common structure:

```html
<div class="approval-disclaimer card p-4 mb-3">
  <!-- 1. Pre-defined Marathi text (read-only) -->
  <div class="disclaimer-text bg-light p-3 rounded border mb-3"
       lang="mr">
    <p class="mb-0">{{ disclaimerText }}</p>
  </div>

  <!-- 2. Mandatory checkbox -->
  <div class="form-check mb-3">
    <input type="checkbox" id="disclaimerCheck" class="form-check-input"
           required aria-required="true">
    <label for="disclaimerCheck" class="form-check-label fw-bold">
      मी सहमत आहे / I agree *
    </label>
  </div>

  <!-- 3. Other opinion -->
  <div class="mb-3">
    <label class="form-label">अन्य अभिप्राय / Other Opinion</label>
    <textarea class="form-control" rows="3"
              placeholder="Enter additional remarks..."></textarea>
  </div>

  <!-- 4. Name selection -->
  <div class="mb-3">
    <label class="form-label">नाव / Name *</label>
    <select class="form-select" required>
      <option value="">Select officer...</option>
      <!-- Populated from API: users with this role -->
    </select>
  </div>

  <!-- 5. Signature -->
  <div class="mb-3">
    <label class="form-label">स्वाक्षरी / Signature *</label>
    <button class="btn btn-outline-primary">✍️ Apply Signature</button>
    <div class="signature-preview mt-2">
      <!-- Shows uploaded PNG signature -->
    </div>
  </div>

  <!-- 6. Action buttons -->
  <div class="d-flex gap-2">
    <button class="btn btn-success" id="approveBtn" disabled>
      ✅ Approve / मंजूर
    </button>
    <button class="btn btn-warning" id="pushbackBtn">
      ↩️ Push Back / परत पाठवा
    </button>
  </div>
</div>
```

**JavaScript behaviour:**
- `approveBtn` is disabled until `disclaimerCheck` is checked AND name is
  selected AND signature is applied.
- `pushbackBtn` opens a modal for mandatory push-back note (no disclaimer needed).

---

## Backend Enforcement

```csharp
// In ApproveStageCommandHandler:
if (!command.DisclaimerAccepted)
    return Result.Failure("Disclaimer must be accepted before approval.");

if (string.IsNullOrWhiteSpace(command.SignaturePath))
    return Result.Failure("Signature is required for approval.");

// Store the exact disclaimer text that was shown at time of approval
approval.DisclaimerText = GetDisclaimerForRole(currentStage);
approval.DisclaimerAccepted = true;
```

The exact disclaimer text is **stored in the ProposalApprovals record** at the
time of approval, ensuring an immutable record of what the officer agreed to —
even if the disclaimer text is later updated in the system.

---

## Push-Back (No Disclaimer)

Push-back does **not** require a disclaimer checkbox. It requires:
- A mandatory note/reason explaining why the proposal is being pushed back
- The note is stored in `PushBackNote_En` / `PushBackNote_Mr`
- No signature required for push-back
- Push-back always returns to Creator JE regardless of stage
