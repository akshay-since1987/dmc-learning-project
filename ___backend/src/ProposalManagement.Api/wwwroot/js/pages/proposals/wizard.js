/**
 * Multi-step Proposal Wizard (4 steps)
 * Each step visible one at a time, Next/Back navigation, Save as Draft per step.
 * Calls POST /api/v1/proposals/step with { stepNumber, ...fields }
 * File uploads via POST /api/v1/proposals/{id}/documents
 */

import api from '../../api.js';
import { getCurrentUser } from '../../auth.js';
import { createDualLangInput } from '../../dual-lang-input.js';
import { showToast } from '../../toast.js';
import { renderSignatureOverlay } from '../../signature-overlay.js';
import { formatCurrency, $ } from '../../utils.js';
import { translateDOM } from '../../i18n.js';

const TOTAL_STEPS = 4;

// ── Master data cache ──────────────────────────────────────────────
let masters = {};

async function loadMasters() {
  const [depts, desigs, fundTypes, accountHeads, wards, procMethods, officers] = await Promise.all([
    api.get('/masters/departments?pageSize=200').catch(() => ({ items: [] })),
    api.get('/masters/designations?pageSize=200').catch(() => ({ items: [] })),
    api.get('/masters/fund-types?pageSize=200').catch(() => ({ items: [] })),
    api.get('/masters/account-heads?pageSize=200').catch(() => ({ items: [] })),
    api.get('/masters/wards?pageSize=200').catch(() => ({ items: [] })),
    api.get('/masters/procurement-methods?pageSize=200').catch(() => ({ items: [] })),
    api.get('/v1/proposals/accounting-officers').catch(() => ({ items: [] })),
  ]);
  masters = {
    departments: depts.items || [],
    designations: desigs.items || [],
    fundTypes: fundTypes.items || [],
    accountHeads: accountHeads.items || [],
    wards: wards.items || [],
    procurementMethods: procMethods.items || [],
    accountingOfficers: officers.items || [],
  };
}

// ── Amount in words ────────────────────────────────────────────────

const ONES = ['', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine',
  'Ten', 'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen'];
const TENS = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

function numberToWordsEn(num) {
  if (num === 0) return 'Zero';
  if (num < 0) return 'Minus ' + numberToWordsEn(-num);

  const intPart = Math.floor(num);
  const paise = Math.round((num - intPart) * 100);

  let words = convertIntPart(intPart) + ' Rupees';
  if (paise > 0) words += ' and ' + convertIntPart(paise) + ' Paise';
  words += ' Only';
  return words;
}

function convertIntPart(n) {
  if (n === 0) return '';
  if (n < 20) return ONES[n];
  if (n < 100) return TENS[Math.floor(n / 10)] + (n % 10 ? ' ' + ONES[n % 10] : '');
  if (n < 1000) return ONES[Math.floor(n / 100)] + ' Hundred' + (n % 100 ? ' ' + convertIntPart(n % 100) : '');
  if (n < 100000) return convertIntPart(Math.floor(n / 1000)) + ' Thousand' + (n % 1000 ? ' ' + convertIntPart(n % 1000) : '');
  if (n < 10000000) return convertIntPart(Math.floor(n / 100000)) + ' Lakh' + (n % 100000 ? ' ' + convertIntPart(n % 100000) : '');
  return convertIntPart(Math.floor(n / 10000000)) + ' Crore' + (n % 10000000 ? ' ' + convertIntPart(n % 10000000) : '');
}

// ── State ──────────────────────────────────────────────────────────

let proposalId = null;
let proposalData = null;
let currentStep = 1;
let dualInputs = {};
let generatedSubmitterPdfPath = null;
let pendingSubmitterSignature = null;

const SUBMITTER_TERMS_EN = 'Final declaration by submitter: I have verified the proposed work and, to the best of my knowledge, this proposal does not violate any legal provisions, financial provisions, Government Resolution, Government direction, court judgement, or court direction. If any such violation is later found, I shall be responsible.';
const SUBMITTER_TERMS_ALT = 'सादर करणारे अधिकारी यांचा अंतिम अभिप्राय - प्रस्तावित कामांबाबत मी तपासणी असून या कामागिरीद्वारे कोणत्याही कायदेधीर तरतुदी/वित्तीय तरतुदी/शासन निर्णय/शासन निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे उल्लंघन होत नाही. या कामागिरीद्वारे कोणत्याही कायदेधीर तरतुदी/वित्तीय तरतुदी/शासन निर्णय/शासन निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे उल्लंघन होत असल्याचे निदर्शनास आल्यास त्यास मी जबाबदार राहील.';

/**
 * @param {HTMLElement} container
 * @param {string|null} editId - existing proposal ID to edit
 */
export async function renderWizard(container, editId = null) {
  if (!container) {
    console.error('Wizard mount container not found');
    return;
  }

  proposalId = editId || null;
  proposalData = null;
  currentStep = 1;
  dualInputs = {};
  generatedSubmitterPdfPath = null;
  pendingSubmitterSignature = null;

  container.innerHTML = `<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div><p class="mt-2 text-muted">Loading wizard...</p></div>`;

  try {
    await loadMasters();
  } catch (err) {
    console.error('loadMasters failed:', err);
    container.innerHTML = `<div class="alert alert-danger m-4"><strong>loadMasters error:</strong><br><code>${escapeHtml(err?.message || String(err))}</code></div>`;
    return;
  }

  if (proposalId) {
    try {
      proposalData = await api.get(`/v1/proposals/${proposalId}`);
      currentStep = Math.min((proposalData.completedStep || 0) + 1, TOTAL_STEPS);
    } catch (err) {
      container.innerHTML = `<div class="alert alert-danger">Failed to load proposal: ${escapeHtml(err.message)}</div>`;
      return;
    }
  }

  try {
    renderWizardShell(container);
    showStep(currentStep);
  } catch (err) {
    console.error('Wizard render error:', err);
    container.innerHTML = `<div class="alert alert-danger m-4"><strong>Wizard render error:</strong><br><code>${escapeHtml(err?.message || String(err))}</code><pre class="mt-2 small">${escapeHtml(err?.stack || '')}</pre></div>`;
  }
}

function renderWizardShell(container) {
  const user = getCurrentUser();

  container.innerHTML = `
    <div class="wizard-container">
      <!-- Progress bar -->
      <div class="card border-0 shadow-sm mb-4">
        <div class="card-body py-3">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <h6 class="mb-0 fw-semibold">
              <i class="bi bi-file-earmark-plus me-2"></i>
              ${proposalId ? 'Edit Proposal' : 'New Proposal'} 
              ${proposalData?.proposalNumber ? `<span class="badge bg-primary ms-2">${escapeHtml(proposalData.proposalNumber)}</span>` : ''}
            </h6>
            <small class="text-muted" id="step-indicator">Step 1 of ${TOTAL_STEPS}</small>
          </div>
          <div class="wizard-progress">
            ${Array.from({ length: TOTAL_STEPS }, (_, i) => `
              <div class="wizard-step-dot ${i + 1 <= currentStep ? 'active' : ''}" data-step="${i + 1}" title="Step ${i + 1}: ${stepTitles[i]}">
                <div class="dot">${i + 1}</div>
                <small class="d-none d-lg-block">${stepTitles[i]}</small>
              </div>
            `).join('')}
          </div>
        </div>
      </div>

      <!-- Step content -->
      <div class="card border-0 shadow-sm mb-4">
        <div class="card-header bg-transparent border-bottom">
          <h6 class="mb-0 fw-semibold" id="step-title"></h6>
        </div>
        <div class="card-body" id="step-content" aria-live="polite"></div>
      </div>

      <!-- Navigation -->
      <div class="d-flex justify-content-between align-items-center">
        <button class="btn btn-outline-secondary" id="btn-prev" aria-label="Previous step">
          <i class="bi bi-arrow-left me-1"></i>Back
        </button>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-primary" id="btn-save-draft">
            <i class="bi bi-save me-1"></i>Save Draft
          </button>
          <button class="btn btn-primary" id="btn-next" aria-label="Next step">
            Next<i class="bi bi-arrow-right ms-1"></i>
          </button>
          <button class="btn btn-success d-none" id="btn-submit">
            <i class="bi bi-send me-1"></i>Submit for Approval
          </button>
        </div>
      </div>
    </div>
  `;

  // Event listeners
  $('btn-prev')?.addEventListener('click', () => goToStep(currentStep - 1));
  $('btn-next')?.addEventListener('click', () => saveAndNext());
  $('btn-save-draft')?.addEventListener('click', () => saveDraft());
  $('btn-submit')?.addEventListener('click', () => submitProposal());

  // Click on step dots to navigate
  container.querySelectorAll('.wizard-step-dot').forEach(dot => {
    dot.addEventListener('click', () => {
      const step = parseInt(dot.dataset.step, 10);
      if (step <= (proposalData?.completedStep || 0) + 1) goToStep(step);
    });
  });

  addWizardStyles();
}

const stepTitles = [
  'Basic Info', 'Technical Sanction', 'Accounting Info', 'Work Place & Compliance'
];

function showStep(step) {
  currentStep = step;
  const content = $('step-content');
  const title = $('step-title');
  const indicator = $('step-indicator');
  if (!content || !title || !indicator) return;

  dualInputs = {};
  title.textContent = `Step ${step}: ${stepTitles[step - 1]}`;
  indicator.textContent = `Step ${step} of ${TOTAL_STEPS}`;

  // Update progress dots
  document.querySelectorAll('.wizard-step-dot').forEach(dot => {
    const s = parseInt(dot.dataset.step, 10);
    dot.classList.toggle('active', s <= step);
    dot.classList.toggle('completed', s < step || s <= (proposalData?.completedStep || 0));
  });

  // Nav button visibility
  $('btn-prev')?.classList.toggle('invisible', step === 1);
  $('btn-next')?.classList.toggle('d-none', step === TOTAL_STEPS);
  $('btn-submit')?.classList.toggle('d-none', step !== TOTAL_STEPS);

  // Render step
  const renderers = [null, renderStep1, renderStep2, renderStep3, renderStep4];
  content.innerHTML = '';
  renderers[step](content);
  translateDOM(content);
}

function goToStep(step) {
  if (step < 1 || step > TOTAL_STEPS) return;

  // Step locking for shared steps (2 = Technical Sanction, 3 = Accounting)
  const LOCKABLE_STEPS = [2, 3];
  const leavingStep = currentStep;

  // Release lock on the step we're leaving (if lockable and we have a proposalId)
  if (LOCKABLE_STEPS.includes(leavingStep) && proposalId) {
    api.delete(`/v1/proposals/${proposalId}/steps/${leavingStep}/lock`).catch(() => {});
  }

  // Acquire lock on the step we're entering
  if (LOCKABLE_STEPS.includes(step) && proposalId) {
    api.post(`/v1/proposals/${proposalId}/steps/${step}/lock`).then(() => {
      showStep(step);
    }).catch(err => {
      if (err.status === 409) {
        showToast(`This step is currently being edited by another user. Please try later.`, 'warning');
      } else {
        showStep(step); // proceed anyway if lock service unavailable
      }
    });
    return;
  }

  showStep(step);
}

// ── Bilingual label helper ─────────────────────────────────────────
/**
 * Returns an HTML <label> with English text on top and Marathi below in smaller muted text.
 * @param {string} en - English label
 * @param {string} alt - Marathi / alt language label
 * @param {string|null} htmlFor - value for `for` attribute
 * @param {boolean} required - show red asterisk
 */
function biLabel(en, alt, htmlFor = null, required = false) {
  const forAttr = htmlFor ? ` for="${htmlFor}"` : '';
  const req = required ? ' <span class="text-danger" aria-hidden="true">*</span>' : '';
  const altPart = alt ? `<br><small class="text-muted" style="font-size:0.8em;font-weight:400">${escapeHtml(alt)}</small>` : '';
  return `<label${forAttr} class="form-label fw-medium mb-1">${escapeHtml(en)}${req}${altPart}</label>`;
}

/**
 * Yes/No/Select option list with alt labels appended.
 */
function ynOptions(trueSelected, falseSelected) {
  return `
    <option value="">-- Select / निवडा --</option>
    <option value="true" ${trueSelected ? 'selected' : ''}>Yes — होय</option>
    <option value="false" ${falseSelected ? 'selected' : ''}>No — नाही</option>`;
}

// ── Step 1: Basic Info ─────────────────────────────────────────────
function renderStep1(container) {
  const user = getCurrentUser();
  const d = proposalData || {};
  const todayStr = new Date().toISOString().split('T')[0];
  const dateVal = d.date || todayStr;
  const docs = (d.documents || []);
  const defaultDesig = d.submitterDesignationId || user?.designationId || '';
  const proposerName = [user?.fullName_En, user?.fullName_Alt].filter(Boolean).join(' — ');

  container.innerHTML = `
    <div class="row g-3">
      <!-- Subject + Date row -->
      <div class="col-md-8" id="subject-dual"></div>
      <div class="col-md-4">
        ${biLabel('Date', 'तारीख', 'w-date', true)}
        <input type="date" class="form-control" id="w-date" value="${dateVal}" required aria-label="Date">
      </div>

      <!-- Proposal Brief -->
      <div class="col-12" id="brief-dual"></div>

      <!-- Proposer Name (readonly) + Designation -->
      <div class="col-md-6">
        ${biLabel("Proposer's Name", 'प्रस्तावकाचे नाव', 'w-proposer')}
        <input type="text" class="form-control bg-light" id="w-proposer" value="${escapeAttr(proposerName)}" readonly aria-label="Proposer name">
      </div>
      <div class="col-md-6">
        ${biLabel('Designation', 'पदनाम', 'w-desig', true)}
        <select class="form-select" id="w-desig" required aria-label="Designation">
          <option value="">-- Select / निवडा --</option>
          ${masters.designations.map(x => `<option value="${x.id}" ${defaultDesig === x.id ? 'selected' : ''}>${escapeHtml(x.name_En)}${x.name_Alt ? ' — ' + escapeHtml(x.name_Alt) : ''}</option>`).join('')}
        </select>
      </div>

      <!-- Fund Type + Fund Owner (dependent) -->
      <div class="col-md-4">
        ${biLabel('Fund Type', 'निधी प्रकार', 'w-fund-type', true)}
        <select class="form-select" id="w-fund-type" required aria-label="Fund Type">
          <option value="">-- Select / निवडा --</option>
          ${masters.fundTypes.map(x => `<option value="${x.id}" ${d.fundTypeId === x.id ? 'selected' : ''}>${escapeHtml(x.name_En)}${x.name_Alt ? ' — ' + escapeHtml(x.name_Alt) : ''}</option>`).join('')}
        </select>
      </div>
      <div class="col-md-4">
        ${biLabel('Fund Owner', 'निधी मालक', 'w-fund-owner', true)}
        <select class="form-select" id="w-fund-owner" required aria-label="Fund Owner">
          <option value="">-- Select Fund Type first / आधी निधी प्रकार निवडा --</option>
        </select>
      </div>
      <div class="col-md-4">
        ${biLabel('Fund Approval Year', 'निधी मंजुरी वर्ष', 'w-fund-year', true)}
        <select class="form-select" id="w-fund-year" required aria-label="Fund Year">
          <option value="">-- Select Year / वर्ष निवडा --</option>
          ${generateFundYears().map(y => `<option value="${y}" ${d.fundYear === y ? 'selected' : ''}>${y}</option>`).join('')}
        </select>
      </div>

      <!-- Ward -->
      <div class="col-md-6">
        ${biLabel('Prabhag / Ward', 'प्रभाग', 'w-ward', true)}
        <select class="form-select" id="w-ward" required aria-label="Ward">
          <option value="">-- Select Ward / प्रभाग निवडा --</option>
          ${masters.wards.map(x => `<option value="${x.id}" ${d.wardId === x.id ? 'selected' : ''}>${escapeHtml(x.name_En)}${x.name_Alt ? ' — ' + escapeHtml(x.name_Alt) : ''}</option>`).join('')}
        </select>
      </div>

      <!-- Proposal Amount -->
      <div class="col-md-6">
        ${biLabel('Proposal Amount (₹)', 'प्रस्तावाची रक्कम (₹)', 'w-est-cost', true)}
        <input type="number" class="form-control" id="w-est-cost" value="${d.estimatedCost || ''}" min="1" step="0.01" required aria-label="Proposal Amount">
      </div>
      <div class="col-12">
        <div class="alert alert-light border py-2 px-3 small mb-0" id="amount-words" role="status">
          <strong>Amount in words / शब्दात रक्कम:</strong> <span id="amount-words-en">—</span>
        </div>
      </div>

      <!-- Upload Estimate PDF -->
      <div class="col-md-6">
        ${biLabel('Upload Estimate PDF', 'अंदाजपत्रक PDF अपलोड', 'upload-estimate', true)}
        <input type="file" class="form-control" id="upload-estimate" accept=".pdf" aria-label="Estimate PDF">
        ${renderExistingDocs(docs, 'EstimateCopy')}
      </div>

      <!-- Field Visit -->
      <div class="col-md-6">
        ${biLabel('Field Visit Done?', 'क्षेत्र भेट झाली का?', 'w-field-visit', true)}
        <select class="form-select" id="w-field-visit" aria-label="Field visit done">
          ${ynOptions(d.siteInspectionDone === true, d.siteInspectionDone === false && d.completedStep >= 1)}
        </select>
      </div>

      <!-- Conditional: Field visit uploads -->
      <div class="col-12" id="field-visit-uploads" style="display:${d.siteInspectionDone ? 'block' : 'none'}">
        <div class="row g-3">
          <div class="col-md-6">
            ${biLabel('Upload Field Visit Report (PDF)', 'क्षेत्र भेट अहवाल अपलोड (PDF)', 'upload-field-visit')}
            <input type="file" class="form-control" id="upload-field-visit" accept=".pdf" aria-label="Field visit report">
            ${renderExistingDocs(docs, 'FieldVisitReport')}
          </div>
          <div class="col-md-6">
            ${biLabel('Field Visit Geo-tagging / Site Photo', 'जिओ-टॅगिंग / साइट फोटो', 'upload-geotagged')}
            <input type="file" class="form-control" id="upload-geotagged" accept=".jpg,.jpeg,.png" aria-label="Site photo">
            ${renderExistingDocs(docs, 'GeoTaggedPhoto')}
          </div>
        </div>
      </div>
    </div>
  `;

  // Create dual-lang inputs
  dualInputs.subject = createDualLangInput({
    name: 'subject', label: 'Subject / Title', type: 'text', required: true,
    valueEn: d.subject_En || '', valueAlt: d.subject_Alt || '',
    placeholderEn: 'Proposal subject in English', placeholderAlt: 'विषय मराठीत',
    maxLength: 500
  });
  container.querySelector('#subject-dual')?.appendChild(dualInputs.subject);

  dualInputs.brief = createDualLangInput({
    name: 'brief', label: 'Proposal Brief', type: 'textarea', required: true,
    valueEn: d.briefInfo_En || '', valueAlt: d.briefInfo_Alt || '',
    placeholderEn: 'Brief description of the proposal', placeholderAlt: 'प्रस्तावाचे थोडक्यात वर्णन',
    maxLength: 2000
  });
  container.querySelector('#brief-dual')?.appendChild(dualInputs.brief);

  // Amount in words update
  const costInput = $('w-est-cost');
  const wordsEl = $('amount-words-en');
  const updateWords = () => {
    const val = parseFloat(costInput?.value);
    if (wordsEl) {
      wordsEl.textContent = (val > 0) ? numberToWordsEn(val) : '—';
    }
  };
  costInput?.addEventListener('input', updateWords);
  updateWords();

  // Fund Owner dependent dropdown
  const fundTypeSelect = $('w-fund-type');
  const fundOwnerSelect = $('w-fund-owner');
  function updateFundOwnerOptions() {
    const selectedId = fundTypeSelect?.value;
    const ft = masters.fundTypes.find(x => x.id === selectedId);
    if (!fundOwnerSelect) return;
    fundOwnerSelect.innerHTML = '';
    if (!ft) {
      fundOwnerSelect.innerHTML = '<option value="">Select Fund Type first</option>';
      return;
    }
    const owners = [];
    if (ft.isMnp) owners.push({ value: 'MNP', label: 'MNP (Municipal)' });
    if (ft.isState) owners.push({ value: 'State', label: 'State' });
    if (ft.isCentral) owners.push({ value: 'Central', label: 'Central' });
    if (ft.isDpdc) owners.push({ value: 'DPDC', label: 'DPDC' });
    if (owners.length === 0) {
      fundOwnerSelect.innerHTML = '<option value="">No owners configured</option>';
      return;
    }
    fundOwnerSelect.innerHTML = '<option value="">Select Owner</option>' +
      owners.map(o => `<option value="${o.value}" ${d.fundOwner === o.value ? 'selected' : ''}>${o.label}</option>`).join('');
    // Auto-select if only one owner
    if (owners.length === 1) fundOwnerSelect.value = owners[0].value;
  }
  fundTypeSelect?.addEventListener('change', updateFundOwnerOptions);
  updateFundOwnerOptions();

  // Field visit toggle
  $('w-field-visit')?.addEventListener('change', (e) => {
    const fieldVisitUploads = $('field-visit-uploads');
    if (fieldVisitUploads) {
      fieldVisitUploads.style.display = e.target.value === 'true' ? 'block' : 'none';
    }
  });
}

// ── Step 2: Technical Sanction + Publishing ────────────────────────
function renderStep2(container) {
  const d = proposalData || {};
  const docs = (d.documents || []);

  container.innerHTML = `
    <h6 class="fw-semibold text-primary mb-3"><i class="bi bi-tools me-2"></i>Technical Sanction <small class="fw-normal text-muted">— तांत्रिक मान्यता</small></h6>
    <div class="row g-3">
      <div class="col-md-4">
        ${biLabel('TS Date', 'तां.मा. तारीख', 'w-ts-date')}
        <input type="date" class="form-control" id="w-ts-date" value="${d.technicalApprovalDate || ''}" aria-label="TS Date">
      </div>
      <div class="col-md-4">
        ${biLabel('TS Number', 'तां.मा. क्रमांक', 'w-ts-number')}
        <input type="text" class="form-control" id="w-ts-number" value="${escapeAttr(d.technicalApprovalNumber || '')}" maxlength="100" aria-label="TS Number">
      </div>
      <div class="col-md-4">
        ${biLabel('TS Amount (₹)', 'तां.मा. रक्कम (₹)', 'w-ts-amount')}
        <input type="number" class="form-control" id="w-ts-amount" value="${d.technicalApprovalCost || ''}" step="0.01" min="0" aria-label="TS Amount">
      </div>
      <div class="col-md-6">
        ${biLabel('Technical Sanction done by Govt/Central?', 'शासन/केंद्रामार्फत तांत्रिक मान्यता?', 'w-ta-done')}
        <select class="form-select" id="w-ta-done" aria-label="Technical sanction by govt">
          ${ynOptions(d.competentAuthorityTADone === true, !d.competentAuthorityTADone)}
        </select>
      </div>
      <div class="col-md-6">
        ${biLabel('TS PDF Upload', 'तां.मा. PDF अपलोड', 'upload-ts-doc')}
        <input type="file" class="form-control" id="upload-ts-doc" accept=".pdf" aria-label="TS Document">
        ${renderExistingDocs(docs, 'TechnicalSanctionDoc')}
      </div>
    </div>

    <hr class="my-4">
    <h6 class="fw-semibold text-primary mb-3"><i class="bi bi-megaphone me-2"></i>Publishing Info <small class="fw-normal text-muted">— प्रकाशन माहिती</small></h6>
    <div class="row g-3">
      <div class="col-md-6">
        ${biLabel('Publication Method (Procurement Method)', 'प्रकाशन पद्धत (खरेदी पद्धत)', 'w-proc-method')}
        <select class="form-select" id="w-proc-method" aria-label="Procurement Method">
          <option value="">-- Select Method / पद्धत निवडा --</option>
          ${masters.procurementMethods.map(x => `<option value="${x.id}" ${d.procurementMethodId === x.id ? 'selected' : ''}>${escapeHtml(x.name_En)}${x.name_Alt ? ' — ' + escapeHtml(x.name_Alt) : ''}</option>`).join('')}
        </select>
      </div>
      <div class="col-md-6">
        ${biLabel('For how many days published?', 'किती दिवस प्रसिद्ध केले?', 'w-pub-days')}
        <input type="number" class="form-control" id="w-pub-days" value="${d.publicationDays ?? ''}" min="0" aria-label="Publication days">
      </div>
    </div>
    <div class="alert alert-info mt-3 small">
      <i class="bi bi-info-circle me-1"></i>
      This step can also be filled by the Technical Department.
      <br><small class="text-muted">हे पाऊल तांत्रिक विभागाद्वारे देखील भरता येते.</small>
    </div>
  `;
}

// ── Step 3: Accounting Info ────────────────────────────────────────
function renderStep3(container) {
  const d = proposalData || {};
  const docs = (d.documents || []);
  const officers = masters.accountingOfficers || [];
  // Pre-compute existing officer's mobile for display
  const existingOfficer = d.accountingOfficerId
    ? officers.find(o => o.id === d.accountingOfficerId)
    : null;
  const existingOfficerId = existingOfficer
    ? (existingOfficer.mobileNumber ? `****${existingOfficer.mobileNumber.slice(-4)}` : '')
    : (d.homeId || '');

  container.innerHTML = `
    <h6 class="fw-semibold text-primary mb-3"><i class="bi bi-calculator me-2"></i>Accounting Info <small class="fw-normal text-muted">— लेखा माहिती</small></h6>
    <div class="row g-3">
      <div class="col-md-7">
        ${biLabel('Accounting Officer', 'लेखा अधिकारी', 'w-acct-officer')}
        <select class="form-select" id="w-acct-officer" aria-label="Accounting Officer">
          <option value="">-- Select Officer / अधिकारी निवडा --</option>
          ${officers.map(x => `<option value="${x.id}" data-mobile="${escapeAttr(x.mobileNumber || '')}" ${d.accountingOfficerId === x.id ? 'selected' : ''}>${escapeHtml(x.fullName_En)}${x.fullName_Alt ? ' — ' + escapeHtml(x.fullName_Alt) : ''}</option>`).join('')}
        </select>
      </div>
      <div class="col-md-5">
        ${biLabel("Officer's ID", 'अधिकारी ओळख क्र.', 'w-officer-id')}
        <input type="text" class="form-control bg-light" id="w-officer-id"
          value="${escapeAttr(existingOfficerId)}"
          readonly aria-label="Officer ID" placeholder="Auto-populated / आपोआप भरले जाईल">
      </div>
      <div class="col-md-6">
        ${biLabel('Accounting Note (Upload PDF)', 'लेखा नोट (PDF अपलोड)', 'upload-acct-doc')}
        <input type="file" class="form-control" id="upload-acct-doc" accept=".pdf" aria-label="Accounting document">
        ${renderExistingDocs(docs, 'AccountingDoc')}
      </div>

      <!-- Previous Expenditure -->
      <div class="col-12">
        ${biLabel('Pre expenditure of the selected fund for this work?', 'या कामासाठी निवडलेल्या निधीतून पूर्व खर्च?')}
        <div class="d-flex gap-3 mt-1">
          <div class="form-check">
            <input class="form-check-input" type="radio" name="hasPrevExp" id="prev-exp-yes" value="true" ${d.hasPreviousExpenditure ? 'checked' : ''}>
            <label class="form-check-label" for="prev-exp-yes">Yes — होय</label>
          </div>
          <div class="form-check">
            <input class="form-check-input" type="radio" name="hasPrevExp" id="prev-exp-no" value="false" ${!d.hasPreviousExpenditure ? 'checked' : ''}>
            <label class="form-check-label" for="prev-exp-no">No — नाही</label>
          </div>
        </div>
      </div>
      <div class="col-md-6" id="prev-exp-amt-wrapper" style="display:${d.hasPreviousExpenditure ? 'block' : 'none'}">
        ${biLabel('Amount (₹)', 'रक्कम (₹)', 'w-prev-exp-amt')}
        <input type="number" class="form-control" id="w-prev-exp-amt" value="${d.previousExpenditureAmount || ''}" step="0.01" min="0" aria-label="Previous expenditure amount">
      </div>

      <!-- Estimated Amount (read-only from Step 1) -->
      <div class="col-md-6">
        ${biLabel('Estimated Amount (₹)', 'अंदाजित रक्कम (₹)', 'w-estimated-display')}
        <input type="text" class="form-control bg-light" id="w-estimated-display" value="${formatCurrency(d.estimatedCost || 0)}" readonly aria-label="Estimated amount">
      </div>

      <!-- Balance Amount -->
      <div class="col-12">
        <div class="alert alert-light border py-2 px-3 small" id="balance-display" role="status">
          <strong>Balance Amount / शिल्लक रक्कम (Estimate − Previous Expenditure / अंदाज − पूर्व खर्च):</strong> <span id="balance-value">—</span>
        </div>
      </div>

      <div class="col-12">
        ${biLabel('Is Accountant willing to process the payment withdrawal for this task?', 'लेखापाल या कामासाठी देयक प्रक्रिया करण्यास तयार आहेत का?')}
        <div class="d-flex gap-3 mt-1">
          <div class="form-check">
            <input class="form-check-input" type="radio" name="acctWilling" id="acct-willing-yes" value="true" ${d.accountantWillingToProcess ? 'checked' : ''}>
            <label class="form-check-label" for="acct-willing-yes">Yes — होय</label>
          </div>
          <div class="form-check">
            <input class="form-check-input" type="radio" name="acctWilling" id="acct-willing-no" value="false" ${!d.accountantWillingToProcess ? 'checked' : ''}>
            <label class="form-check-label" for="acct-willing-no">No — नाही</label>
          </div>
        </div>
      </div>
    </div>
    <div class="alert alert-info mt-3 small">
      <i class="bi bi-info-circle me-1"></i>
      This step can also be filled by the Account Department.
      <br><small class="text-muted">हे पाऊल लेखा विभागाद्वारे देखील भरता येते.</small>
    </div>
  `;

  // Auto-populate officer ID when officer selection changes
  document.getElementById('w-acct-officer')?.addEventListener('change', (e) => {
    const sel = e.target.options[e.target.selectedIndex];
    const mobile = sel?.dataset.mobile || '';
    document.getElementById('w-officer-id').value = mobile ? `****${mobile.slice(-4)}` : '';
  });

  // Toggle previous expenditure amount
  document.querySelectorAll('input[name="hasPrevExp"]').forEach(r => {
    r.addEventListener('change', () => {
      const prevExpWrapper = $('prev-exp-amt-wrapper');
      if (prevExpWrapper) {
        prevExpWrapper.style.display = r.value === 'true' && r.checked ? 'block' : 'none';
      }
      calcBalance();
    });
  });

  // Auto-calculate balance
  const calcBalance = () => {
    const estimate = d.estimatedCost || 0;
    const prevExp = parseFloat($('w-prev-exp-amt')?.value) || 0;
    const balance = estimate - prevExp;
    const balanceValue = $('balance-value');
    if (balanceValue) {
      balanceValue.textContent = formatCurrency(balance);
    }
  };
  $('w-prev-exp-amt')?.addEventListener('input', calcBalance);
  calcBalance();
}

// ── Step 4: Work Place + Compliance + Authority ────────────────────
function renderStep4(container) {
  const d = proposalData || {};
  const docs = (d.documents || []);

  container.innerHTML = `
    <!-- Section A: Work Place Ownership -->
    <h6 class="fw-semibold text-primary mb-3"><i class="bi bi-building me-2"></i>Work Place Details <small class="fw-normal text-muted">— कार्यस्थळ तपशील</small></h6>
    <div class="row g-3">
      <div class="col-md-6">
        ${biLabel('Is work place owned by Palika?', 'कामाची जागा पालिकेच्या मालकीची आहे का?', 'w-palika')}
        <select class="form-select" id="w-palika" aria-label="Work place owned by palika">
          ${ynOptions(d.workPlaceWithinPalika === true, d.workPlaceWithinPalika === false && d.completedStep >= 4)}
        </select>
      </div>

      <!-- Ownership doc (shown when Yes) -->
      <div class="col-md-6" id="ownership-doc-wrapper" style="display:${d.workPlaceWithinPalika ? 'block' : 'none'}">
        ${biLabel('Work Place Ownership Document (PDF)', 'जागा मालकी दस्तऐवज (PDF)', 'upload-ownership')}
        <input type="file" class="form-control" id="upload-ownership" accept=".pdf" aria-label="Ownership document">
        ${renderExistingDocs(docs, 'OwnershipDoc')}
      </div>

      <!-- NOC (shown when No) -->
      <div class="col-md-6" id="noc-wrapper" style="display:${d.workPlaceWithinPalika === false && d.completedStep >= 4 ? 'block' : 'none'}">
        ${biLabel('NOC Upload', 'NOC अपलोड', 'upload-noc')}
        <input type="file" class="form-control" id="upload-noc" accept=".pdf" aria-label="NOC document">
        ${renderExistingDocs(docs, 'NocDocument')}
      </div>
      <div class="col-md-6" id="have-noc-wrapper" style="display:${d.workPlaceWithinPalika === false && d.completedStep >= 4 ? 'block' : 'none'}">
        ${biLabel('Do you have NOC?', 'तुमच्याकडे NOC आहे का?', 'w-noc')}
        <select class="form-select" id="w-noc" aria-label="Have NOC">
          ${ynOptions(d.nocObtained === true, !d.nocObtained && d.completedStep >= 4)}
        </select>
      </div>
    </div>

    <hr class="my-4">

    <!-- Section B: Legalities -->
    <h6 class="fw-semibold text-primary mb-3"><i class="bi bi-shield-check me-2"></i>Legalities <small class="fw-normal text-muted">— कायदेशीर बाबी</small></h6>
    <div class="row g-3">
      <div class="col-md-6">
        ${biLabel('Is survey done by Head about all legalities?', 'सर्व कायद्याबाबत प्रमुखाने सर्वेक्षण केले का?', 'w-legal-survey')}
        <select class="form-select" id="w-legal-survey" aria-label="Legal survey done">
          ${ynOptions(d.legalSurveyDone === true, !d.legalSurveyDone && d.completedStep >= 4)}
        </select>
      </div>
      <div class="col-md-6">
        ${biLabel('Is any court matter pending about the work area?', 'कामाच्या जागेबद्दल न्यायालयीन प्रकरण प्रलंबित आहे का?', 'w-court-case')}
        <select class="form-select" id="w-court-case" aria-label="Court case pending">
          ${ynOptions(d.courtCasePending === true, !d.courtCasePending && d.completedStep >= 4)}
        </select>
      </div>
      <div class="col-12" id="court-details-wrapper" style="display:${d.courtCasePending ? 'block' : 'none'}">
        <div class="row g-3">
          <div class="col-md-6">
            ${biLabel('Upload Court Document (PDF)', 'न्यायालयीन दस्तऐवज अपलोड (PDF)', 'upload-court-doc')}
            <input type="file" class="form-control" id="upload-court-doc" accept=".pdf" aria-label="Court document">
            ${renderExistingDocs(docs, 'CourtDoc')}
          </div>
          <div class="col-md-6" id="court-dual"></div>
        </div>
      </div>
    </div>

    <hr class="my-4">

    <!-- Section C: Duplication & Compliance -->
    <h6 class="fw-semibold text-primary mb-3"><i class="bi bi-clipboard-check me-2"></i>Duplication & Compliance <small class="fw-normal text-muted">— डुप्लिकेशन आणि अनुपालन</small></h6>
    <div class="row g-3">
      <div class="col-md-6">
        ${biLabel('Is same work carried out from another fund head?', 'दुसऱ्या निधी शीर्षकातून समान काम केले जाते का?', 'w-dup-fund')}
        <select class="form-select" id="w-dup-fund" aria-label="Same work from another fund">
          ${ynOptions(d.duplicateFundCheckDone === true, !d.duplicateFundCheckDone && d.completedStep >= 4)}
        </select>
      </div>
      <div class="col-md-6" id="dup-fund-details-wrapper" style="display:${d.duplicateFundCheckDone ? 'block' : 'none'}">
        ${biLabel('Upload Details (PDF)', 'तपशील अपलोड (PDF)', 'upload-dup-fund-doc')}
        <input type="file" class="form-control" id="upload-dup-fund-doc" accept=".pdf" aria-label="Duplicate fund document">
        ${renderExistingDocs(docs, 'DuplicateFundDoc')}
      </div>

      <div class="col-md-6">
        ${biLabel('Is same work already proposed in any other fund?', 'इतर कोणत्याही निधीत समान काम आधीच प्रस्तावित आहे का?', 'w-same-fund')}
        <select class="form-select" id="w-same-fund" aria-label="Same work in other fund">
          ${ynOptions(d.sameWorkProposedInOtherFund === true, !d.sameWorkProposedInOtherFund && d.completedStep >= 4)}
        </select>
      </div>
      <div class="col-md-6" id="same-fund-details-wrapper" style="display:${d.sameWorkProposedInOtherFund ? 'block' : 'none'}">
        ${biLabel('Upload Details (PDF)', 'तपशील अपलोड (PDF)', 'upload-same-fund-doc')}
        <input type="file" class="form-control" id="upload-same-fund-doc" accept=".pdf" aria-label="Same fund document">
        ${renderExistingDocs(docs, 'OtherFundDoc')}
      </div>

      <div class="col-md-6">
        ${biLabel('Is the work/tender completed with past vendor?', 'मागील ठेकेदाराशी काम/निविदा पूर्ण झाली का?', 'w-vendor-tenure')}
        <select class="form-select" id="w-vendor-tenure" aria-label="Vendor tenure completed">
          ${ynOptions(d.vendorTenureCompleted === true, !d.vendorTenureCompleted && d.completedStep >= 4)}
        </select>
      </div>
      <div class="col-md-6">
        ${biLabel('DLP / Verification', 'DLP / पडताळणी', 'w-dlp')}
        <select class="form-select" id="w-dlp" aria-label="DLP check done">
          ${ynOptions(d.dlpCheckDone === true, !d.dlpCheckDone && d.completedStep >= 4)}
        </select>
      </div>
    </div>

    <hr class="my-4">

    <!-- Section D: Approval Authority -->
    <h6 class="fw-semibold text-primary mb-3"><i class="bi bi-person-badge me-2"></i>Approval Authority <small class="fw-normal text-muted">— मंजुरी प्राधिकरण</small></h6>
    <div class="row g-3">
      <div class="col-md-6">
        ${biLabel('Who will approve this work?', 'हे काम कोण मंजूर करेल?', 'w-first-approver', true)}
        <select class="form-select" id="w-first-approver" required aria-label="First approver role">
          <option value="">-- Select / निवडा --</option>
          <option value="CityEngineer" ${d.firstApproverRole === 'CityEngineer' ? 'selected' : ''}>City Engineer — नगर अभियंता</option>
          <option value="ADO" ${d.firstApproverRole === 'ADO' ? 'selected' : ''}>Additional Development Officer (ADO) — अतिरिक्त विकास अधिकारी</option>
        </select>
      </div>
    </div>
    <hr class="my-4">

    <h6 class="fw-semibold text-primary mb-3"><i class="bi bi-shield-check me-2"></i>Submitter Declaration & Remarks <small class="fw-normal text-muted">— सादरकर्त्याची घोषणा व टिप्पणी</small></h6>
    <div class="card border-warning mb-3">
      <div class="card-body">
        <p class="small mb-2"><strong>Declaration (English)</strong><br>${escapeHtml(SUBMITTER_TERMS_EN)}</p>
        <p class="small mb-3" lang="mr"><strong>घोषणा (मराठी)</strong><br>${escapeHtml(SUBMITTER_TERMS_ALT)}</p>
        <div class="form-check">
          <input class="form-check-input" type="checkbox" id="w-submitter-terms" ${d.submitterDeclarationAccepted ? 'checked' : ''}>
          <label class="form-check-label" for="w-submitter-terms">
            I confirm and accept the above declaration. / मी वरील घोषणा मान्य करतो.
          </label>
        </div>
      </div>
    </div>

    <div class="row g-3">
      <div class="col-12" id="submitter-remarks-dual"></div>
    </div>

    <div class="card mt-4" id="submitter-finalization-card">
      <div class="card-header bg-light">
        <h6 class="mb-0"><i class="bi bi-file-earmark-pdf me-2"></i>Final Preview & Signature <small class="text-muted">— अंतिम पूर्वावलोकन आणि स्वाक्षरी</small></h6>
      </div>
      <div class="card-body">
        <div class="d-flex flex-wrap gap-2 align-items-center">
          <button type="button" class="btn btn-outline-primary" id="btn-generate-submitter-pdf">
            <i class="bi bi-file-earmark-pdf me-1"></i>Generate Full PDF
          </button>
          <button type="button" class="btn btn-outline-success" id="btn-place-submitter-sign" disabled>
            <i class="bi bi-pen me-1"></i>Place Signature
          </button>
          <span class="small text-muted" id="submitter-sign-status">Generate PDF and place signature before requesting approval.</span>
        </div>
      </div>
      <div class="card-body p-0 d-none" id="submitter-sign-overlay"></div>
    </div>

    <div class="alert alert-success mt-4">
      <h6 class="alert-heading"><i class="bi bi-check-circle me-2"></i>Ready to Submit — सादर करण्यास तयार</h6>
      <p class="mb-0 small">After accepting declaration, adding remarks, and placing your signature on the generated PDF, click <strong>Submit for Approval</strong> to send the proposal to the first approver (City Engineer/ADO as selected).<br><small class="text-muted">घोषणा मान्य करून, टिप्पणी भरून आणि तयार PDF वर स्वाक्षरी ठेवल्यानंतर <strong>मंजुरीसाठी सादर करा</strong> वर क्लिक करा.</small></p>
    </div>
  `;

  // Work place toggle: Yes→show ownership doc, No→show NOC fields
  $('w-palika')?.addEventListener('change', (e) => {
    const isYes = e.target.value === 'true';
    const isNo = e.target.value === 'false';
    const ownershipDoc = $('ownership-doc-wrapper');
    const nocWrapper = $('noc-wrapper');
    const haveNocWrapper = $('have-noc-wrapper');
    if (ownershipDoc) ownershipDoc.style.display = isYes ? 'block' : 'none';
    if (nocWrapper) nocWrapper.style.display = isNo ? 'block' : 'none';
    if (haveNocWrapper) haveNocWrapper.style.display = isNo ? 'block' : 'none';
  });

  // Court case toggle
  $('w-court-case')?.addEventListener('change', (e) => {
    const courtDetails = $('court-details-wrapper');
    if (courtDetails) {
      courtDetails.style.display = e.target.value === 'true' ? 'block' : 'none';
    }
  });

  // Duplicate fund toggle
  $('w-dup-fund')?.addEventListener('change', (e) => {
    const dupFundDetails = $('dup-fund-details-wrapper');
    if (dupFundDetails) {
      dupFundDetails.style.display = e.target.value === 'true' ? 'block' : 'none';
    }
  });

  // Same fund toggle
  $('w-same-fund')?.addEventListener('change', (e) => {
    const sameFundDetails = $('same-fund-details-wrapper');
    if (sameFundDetails) {
      sameFundDetails.style.display = e.target.value === 'true' ? 'block' : 'none';
    }
  });

  // Court case details dual-lang
  dualInputs.courtDetails = createDualLangInput({
    name: 'courtDetails', label: 'Work Area Description', type: 'textarea', required: false,
    valueEn: d.courtCaseDetails_En || '', valueAlt: d.courtCaseDetails_Alt || '',
    maxLength: 2000
  });
  container.querySelector('#court-dual')?.appendChild(dualInputs.courtDetails);

  dualInputs.submitterRemarks = createDualLangInput({
    name: 'submitterRemarks', label: 'Submitter Remarks', type: 'textarea', required: false,
    valueEn: d.submitterRemarks_En || '', valueAlt: d.submitterRemarks_Alt || '',
    placeholderEn: 'Add final remarks for the proposal (optional)',
    placeholderAlt: 'प्रस्तावासाठी अंतिम टिप्पणी लिहा (ऐच्छिक)',
    maxLength: 2000
  });
  container.querySelector('#submitter-remarks-dual')?.appendChild(dualInputs.submitterRemarks);

  const user = getCurrentUser();
  const termsCheckbox = $('w-submitter-terms');
  const generatePdfBtn = $('btn-generate-submitter-pdf');
  const placeSignBtn = $('btn-place-submitter-sign');
  const signStatus = $('submitter-sign-status');
  const overlayHost = $('submitter-sign-overlay');

  const openSignatureOverlay = async () => {
    if (!proposalId || !generatedSubmitterPdfPath) {
      showToast('Generate PDF before placing signature', 'warning');
      return;
    }

    if (!user?.signaturePath) {
      if (signStatus) {
        signStatus.textContent = 'No signature found in profile. Upload signature first, then place it on PDF.';
        signStatus.className = 'small text-danger';
      }
      showToast('No signature found in profile. Please upload signature in profile first.', 'warning');
      return;
    }

    if (!overlayHost) return;

    const pdfUrl = `/api/v1/proposals/${proposalId}/pdf/download?path=${encodeURIComponent(generatedSubmitterPdfPath)}`;
    const signatureImageUrl = user.signaturePath;
    overlayHost.classList.remove('d-none');

    try {
      await renderSignatureOverlay(
        overlayHost,
        pdfUrl,
        signatureImageUrl,
        (meta) => {
          pendingSubmitterSignature = { ...meta, generatedPdfPath: generatedSubmitterPdfPath };
          overlayHost.classList.add('d-none');
          showToast('Signature placement captured', 'success');
          updateSubmitReadiness();
        },
        () => {
          overlayHost.classList.add('d-none');
        }
      );
    } catch (err) {
      overlayHost.classList.add('d-none');
      showToast(err.message || 'Failed to open signature overlay', 'danger');
    }
  };

  const updateSubmitReadiness = () => {
    const canSubmit = Boolean(termsCheckbox?.checked && generatedSubmitterPdfPath && pendingSubmitterSignature);
    const submitBtn = $('btn-submit');
    if (submitBtn) submitBtn.disabled = !canSubmit;
    if (signStatus) {
      if (pendingSubmitterSignature) {
        signStatus.textContent = 'Signature placed. You can request approval now.';
        signStatus.className = 'small text-success';
      } else if (generatedSubmitterPdfPath) {
        signStatus.textContent = 'PDF generated. Place signature to continue.';
        signStatus.className = 'small text-warning';
      } else {
        signStatus.textContent = 'Generate PDF and place signature before requesting approval.';
        signStatus.className = 'small text-muted';
      }
    }
  };

  termsCheckbox?.addEventListener('change', updateSubmitReadiness);

  generatePdfBtn?.addEventListener('click', async () => {
    if (!proposalId) {
      showToast('Please save this step first to generate PDF', 'warning');
      return;
    }

    const stepSaved = await saveStep(4);
    if (!stepSaved) return;

    const btn = generatePdfBtn;
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Generating...';

    try {
      const result = await api.post(`/v1/proposals/${proposalId}/pdf/generate`);
      generatedSubmitterPdfPath = result?.filePath || null;
      pendingSubmitterSignature = null;
      if (placeSignBtn) placeSignBtn.disabled = !generatedSubmitterPdfPath;
      showToast('PDF generated successfully', 'success');
      updateSubmitReadiness();
      if (generatedSubmitterPdfPath) {
        await openSignatureOverlay();
      }
    } catch (err) {
      showToast(err.message || 'Failed to generate PDF', 'danger');
    } finally {
      btn.disabled = false;
      btn.innerHTML = '<i class="bi bi-file-earmark-pdf me-1"></i>Generate Full PDF';
    }
  });

  placeSignBtn?.addEventListener('click', async () => {
    await openSignatureOverlay();
  });

  updateSubmitReadiness();
}

// ── Data collection per step ───────────────────────────────────────

function collectStepData(step) {
  const data = { stepNumber: step, proposalId };

  switch (step) {
    case 1: {
      const user = getCurrentUser();
      const subj = dualInputs.subject?.getValues() || {};
      const brief = dualInputs.brief?.getValues() || {};
      Object.assign(data, {
        date: $('w-date')?.value || null,
        departmentId: user?.departmentId || null,
        submitterDesignationId: $('w-desig')?.value || null,
        subject_En: subj.en, subject_Alt: subj.alt,
        briefInfo_En: brief.en, briefInfo_Alt: brief.alt,
        fundTypeId: $('w-fund-type')?.value || null,
        fundOwner: $('w-fund-owner')?.value || null,
        fundYear: $('w-fund-year')?.value || null,
        wardId: $('w-ward')?.value || null,
        estimatedCost: parseFloat($('w-est-cost')?.value) || null,
        siteInspectionDone: $('w-field-visit')?.value === 'true',
      });
      break;
    }
    case 2:
      // Technical Sanction + Publishing
      data.technicalApprovalDate = $('w-ts-date')?.value || null;
      data.technicalApprovalNumber = $('w-ts-number')?.value || null;
      data.technicalApprovalCost = parseFloat($('w-ts-amount')?.value) || null;
      data.competentAuthorityTADone = $('w-ta-done')?.value === 'true';
      data.procurementMethodId = $('w-proc-method')?.value || null;
      data.publicationDays = parseInt($('w-pub-days')?.value, 10) || null;
      break;
    case 3:
      // Accounting Info
      data.accountingOfficerId = document.getElementById('w-acct-officer')?.value || null;
      data.homeId = document.getElementById('w-officer-id')?.value || null;
      data.hasPreviousExpenditure = document.querySelector('#prev-exp-yes')?.checked || false;
      data.previousExpenditureAmount = parseFloat($('w-prev-exp-amt')?.value) || null;
      data.accountantWillingToProcess = document.querySelector('#acct-willing-yes')?.checked || false;
      break;
    case 4: {
      // Work Place + Compliance + Authority
      const court = dualInputs.courtDetails?.getValues() || {};
      const submitterRemarks = dualInputs.submitterRemarks?.getValues() || {};
      data.workPlaceWithinPalika = $('w-palika')?.value === 'true';
      data.nocObtained = $('w-noc')?.value === 'true';
      data.legalSurveyDone = $('w-legal-survey')?.value === 'true';
      data.courtCasePending = $('w-court-case')?.value === 'true';
      data.courtCaseDetails_En = court.en || null;
      data.courtCaseDetails_Alt = court.alt || null;
      data.duplicateFundCheckDone = $('w-dup-fund')?.value === 'true';
      data.sameWorkProposedInOtherFund = $('w-same-fund')?.value === 'true';
      data.vendorTenureCompleted = $('w-vendor-tenure')?.value === 'true';
      data.dlpCheckDone = $('w-dlp')?.value === 'true';
      data.firstApproverRole = $('w-first-approver')?.value || null;
      data.submitterDeclarationAccepted = $('w-submitter-terms')?.checked || false;
      data.submitterRemarks_En = submitterRemarks.en || null;
      data.submitterRemarks_Alt = submitterRemarks.alt || null;
      break;
    }
  }

  return data;
}

// ── Save & Navigation ──────────────────────────────────────────────

async function saveStep(step) {
  const data = collectStepData(step);

  try {
    setBusy(true);
    const result = await api.post('/v1/proposals/step', data);
    if (!proposalId && result?.id) {
      proposalId = result.id;
    }
    // Upload any files for this step
    await uploadStepFiles(step);
    // Refresh proposal data
    if (proposalId) {
      proposalData = await api.get(`/v1/proposals/${proposalId}`);
    }
    return true;
  } catch (err) {
    showToast(err.message || 'Failed to save step', 'danger');
    return false;
  } finally {
    setBusy(false);
  }
}

async function uploadStepFiles(step) {
  if (!proposalId) return;

  const uploads = [];
  switch (step) {
    case 1:
      uploads.push({ inputId: 'upload-estimate', docType: 'EstimateCopy' });
      uploads.push({ inputId: 'upload-field-visit', docType: 'FieldVisitReport' });
      uploads.push({ inputId: 'upload-geotagged', docType: 'GeoTaggedPhoto' });
      break;
    case 2:
      uploads.push({ inputId: 'upload-ts-doc', docType: 'TechnicalSanctionDoc' });
      break;
    case 3:
      uploads.push({ inputId: 'upload-acct-doc', docType: 'AccountingDoc' });
      break;
    case 4:
      uploads.push({ inputId: 'upload-ownership', docType: 'OwnershipDoc' });
      uploads.push({ inputId: 'upload-noc', docType: 'NocDocument' });
      uploads.push({ inputId: 'upload-court-doc', docType: 'CourtDoc' });
      uploads.push({ inputId: 'upload-dup-fund-doc', docType: 'DuplicateFundDoc' });
      uploads.push({ inputId: 'upload-same-fund-doc', docType: 'OtherFundDoc' });
      break;
  }

  for (const { inputId, docType } of uploads) {
    const input = document.getElementById(inputId);
    if (input?.files?.length > 0) {
      const formData = new FormData();
      formData.append('file', input.files[0]);
      formData.append('documentType', docType);
      try {
        await api.upload(`/v1/proposals/${proposalId}/documents`, formData);
      } catch (err) {
        showToast(`File upload failed: ${err.message}`, 'warning');
      }
    }
  }
}

async function saveAndNext() {
  const saved = await saveStep(currentStep);
  if (saved && currentStep < TOTAL_STEPS) {
    goToStep(currentStep + 1);
    showToast('Step saved', 'success');
  }
}

async function saveDraft() {
  const saved = await saveStep(currentStep);
  if (saved) showToast('Draft saved successfully', 'success');
}

async function submitProposal() {
  // Save step 4 first
  const saved = await saveStep(4);
  if (!saved) return;

  if (!proposalId) {
    showToast('Please complete all steps first', 'warning');
    return;
  }

  const acceptedTerms = $('w-submitter-terms')?.checked;
  if (!acceptedTerms) {
    showToast('Please accept the declaration before requesting approval', 'warning');
    return;
  }

  if (!generatedSubmitterPdfPath || !pendingSubmitterSignature) {
    showToast('Please generate full PDF and place your signature before requesting approval', 'warning');
    return;
  }

  try {
    setBusy(true);
    await api.post(`/v1/proposals/${proposalId}/submit`, {
      signaturePageNumber: pendingSubmitterSignature.pageNumber,
      signaturePositionX: pendingSubmitterSignature.x,
      signaturePositionY: pendingSubmitterSignature.y,
      signatureWidth: pendingSubmitterSignature.width,
      signatureHeight: pendingSubmitterSignature.height,
      signatureRotation: pendingSubmitterSignature.rotation,
      generatedPdfPath: pendingSubmitterSignature.generatedPdfPath || generatedSubmitterPdfPath,
    });
    showToast('Proposal submitted for approval!', 'success');
    generatedSubmitterPdfPath = null;
    pendingSubmitterSignature = null;
    window.location.hash = '#/proposals';
  } catch (err) {
    showToast(err.message || 'Submit failed', 'danger');
  } finally {
    setBusy(false);
  }
}

// ── Helpers ────────────────────────────────────────────────────────

function setBusy(busy) {
  const btns = ['btn-prev', 'btn-next', 'btn-save-draft', 'btn-submit'];
  btns.forEach(id => {
    const el = $(id);
    if (!el) return;
    if (id === 'btn-submit' && !busy) {
      el.disabled = !isSubmitReady();
      return;
    }
    el.disabled = busy;
  });
  const content = $('step-content');
  if (content) content.setAttribute('aria-busy', String(busy));
}

function isSubmitReady() {
  return Boolean($('w-submitter-terms')?.checked && generatedSubmitterPdfPath && pendingSubmitterSignature);
}

function renderExistingDocs(docs, type) {
  const filtered = (docs || []).filter(d => d.documentType === type);
  if (!filtered.length) return '';
  return `<div class="mt-1">${filtered.map(d =>
    `<small class="d-block text-success"><i class="bi bi-check-circle me-1"></i>${escapeHtml(d.fileName)}</small>`
  ).join('')}</div>`;
}

function generateFundYears() {
  const y = new Date().getFullYear();
  const years = [];
  for (let i = -1; i <= 2; i++) {
    years.push(`${y + i}-${(y + i + 1).toString().slice(2)}`);
  }
  return years;
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str || '';
  return div.innerHTML;
}

function escapeAttr(str) {
  return String(str || '').replace(/[&"'<>]/g, c => ({
    '&': '&amp;', '"': '&quot;', "'": '&#39;', '<': '&lt;', '>': '&gt;'
  })[c]);
}

// ── Wizard-specific styles ─────────────────────────────────────────
function addWizardStyles() {
  if (document.getElementById('wizard-styles')) return;
  const style = document.createElement('style');
  style.id = 'wizard-styles';
  style.textContent = `
    .wizard-container { max-width: 900px; margin: 0 auto; }

    .wizard-progress {
      display: flex; justify-content: space-between; align-items: flex-start;
      gap: 4px; position: relative;
    }
    .wizard-progress::before {
      content: ''; position: absolute; top: 16px; left: 24px; right: 24px;
      height: 3px; background: var(--bs-gray-300); z-index: 0;
    }
    .wizard-step-dot {
      display: flex; flex-direction: column; align-items: center; gap: 4px;
      cursor: pointer; z-index: 1; min-width: 40px; flex: 1;
    }
    .wizard-step-dot .dot {
      width: 32px; height: 32px; border-radius: 50%;
      background: var(--bs-gray-300); color: var(--bs-gray-600);
      display: flex; align-items: center; justify-content: center;
      font-size: 0.8rem; font-weight: 600; transition: all 0.3s ease;
      border: 2px solid transparent;
    }
    .wizard-step-dot.active .dot {
      background: var(--bs-primary); color: white;
      border-color: var(--bs-primary);
    }
    .wizard-step-dot.completed .dot {
      background: var(--bs-success); color: white;
      border-color: var(--bs-success);
    }
    .wizard-step-dot small {
      font-size: 0.65rem; text-align: center; color: var(--bs-gray-600);
      max-width: 80px; word-wrap: break-word;
    }
    .wizard-step-dot.active small { color: var(--bs-primary); font-weight: 600; }
  `;
  document.head.appendChild(style);
}
