/** Proposal create/edit form — all 44+ fields organized in sections */
import api from '../../api.js';
import { showToast } from '../../toast.js';
import { navigate } from '../../router.js';
import { createDualLangInput } from '../../dual-lang-input.js';

/** Masters data cache */
let masters = null;

async function loadMasters() {
  if (masters) return masters;
  const [depts, fundTypes, accountHeads, wards, procMethods, designations] = await Promise.all([
    api.get('/masters/departments?pageSize=200'),
    api.get('/masters/fund-types?pageSize=200'),
    api.get('/masters/account-heads?pageSize=200'),
    api.get('/masters/wards?pageSize=200'),
    api.get('/masters/procurement-methods?pageSize=200'),
    api.get('/masters/designations?pageSize=200'),
  ]);
  const extract = (res) => Array.isArray(res) ? res : (res?.items ?? []);
  masters = {
    depts: extract(depts),
    fundTypes: extract(fundTypes),
    accountHeads: extract(accountHeads),
    wards: extract(wards),
    procMethods: extract(procMethods),
    designations: extract(designations),
  };
  return masters;
}

function selectOptions(items, selectedId, valueKey = 'id', labelKey = 'name_En') {
  return items.map(i =>
    `<option value="${i[valueKey]}" ${i[valueKey] === selectedId ? 'selected' : ''}>${escapeHtml(i[labelKey])}</option>`
  ).join('');
}

/**
 * @param {HTMLElement} container
 * @param {string|null} proposalId - null for create, GUID for edit
 */
export async function renderProposalForm(container, proposalId = null) {
  const isEdit = !!proposalId;

  container.innerHTML = `<div class="text-center py-5"><div class="spinner-border text-primary"></div><p class="mt-2">Loading form...</p></div>`;

  let proposal = null;
  try {
    const m = await loadMasters();
    if (isEdit) {
      proposal = await api.get(`/proposals/${proposalId}`);
    }
    renderForm(container, proposal, m, isEdit);
  } catch (err) {
    container.innerHTML = `<div class="alert alert-danger">Failed to load: ${err.message || 'Unknown error'}</div>`;
  }
}

function renderForm(container, p, m, isEdit) {
  container.innerHTML = '';

  const form = document.createElement('form');
  form.noValidate = true;
  form.className = 'proposal-form';

  // Header
  const header = document.createElement('div');
  header.className = 'd-flex justify-content-between align-items-center mb-4';
  header.innerHTML = `
    <div>
      <h5 class="mb-1">${isEdit ? `Edit Proposal — ${escapeHtml(p?.proposalNumber || '')}` : 'New Proposal'}</h5>
      <small class="text-muted">${isEdit ? 'Update proposal details' : 'Create a new proposal draft'}</small>
    </div>
    <div class="d-flex gap-2">
      <button type="button" class="btn btn-outline-secondary" id="btn-cancel"><i class="bi bi-arrow-left me-1"></i>Back</button>
      <button type="submit" class="btn btn-primary" id="btn-save"><i class="bi bi-save me-1"></i>${isEdit ? 'Save' : 'Create Draft'}</button>
      ${isEdit && (p?.currentStage === 'Draft' || p?.currentStage === 'PushedBack') ? `<button type="button" class="btn btn-success" id="btn-submit"><i class="bi bi-send me-1"></i>Submit for Approval</button>` : ''}
    </div>`;
  form.appendChild(header);

  // Section 1: Basic Info
  form.appendChild(sectionCard('Basic Information', () => {
    const div = document.createElement('div');
    div.className = 'row g-3';

    // Subject (dual lang)
    const subjectDual = createDualLangInput({
      name: 'subject', label: 'Subject', type: 'text', required: true, maxLength: 500,
      valueEn: p?.subject_En || '', valueAlt: p?.subject_Alt || '',
      placeholderEn: 'Proposal subject in English', placeholderAlt: 'प्रस्तावाचा विषय मराठीत'
    });
    div.appendChild(wrapCol(subjectDual, 12));

    // Department
    div.appendChild(wrapCol(selectField('departmentId', 'Department', m.depts, p?.departmentId, true), 4));

    // Designation
    div.appendChild(wrapCol(selectField('designationId', 'Designation', m.designations, p?.submitterDesignationId, true, 'id', 'name_En'), 4));

    // Fund Type
    div.appendChild(wrapCol(selectField('fundTypeId', 'Fund Type', m.fundTypes, p?.fundTypeId, true), 4));

    // Fund Year
    div.appendChild(wrapCol(textField('fundYear', 'Fund Year', p?.fundYear || getCurrentFundYear(), true, 'e.g. 2025-26'), 4));

    // Reference Number
    div.appendChild(wrapCol(textField('referenceNumber', 'Reference Number', p?.referenceNumber || '', true), 4));

    // Ward
    div.appendChild(wrapCol(selectField('wardId', 'Ward', m.wards, p?.wardId, false, 'id', 'name_En'), 4));

    return div;
  }));

  // Section 2: Brief Info (dual lang)
  form.appendChild(sectionCard('Brief Information', () => {
    const div = document.createElement('div');
    const briefDual = createDualLangInput({
      name: 'briefInfo', label: 'Brief Information', type: 'textarea', required: true,
      valueEn: p?.briefInfo_En || '', valueAlt: p?.briefInfo_Alt || '',
      placeholderEn: 'Brief description in English', placeholderAlt: 'संक्षिप्त माहिती मराठीत'
    });
    div.appendChild(briefDual);
    return div;
  }));

  // Section 3: Financial Details
  form.appendChild(sectionCard('Financial Details', () => {
    const div = document.createElement('div');
    div.className = 'row g-3';

    div.appendChild(wrapCol(numberField('estimatedCost', 'Estimated Cost (₹)', p?.estimatedCost, true), 3));
    div.appendChild(wrapCol(selectField('accountHeadId', 'Account Head', m.accountHeads, p?.accountHeadId, true), 3));
    div.appendChild(wrapCol(numberField('approvedBudget', 'Approved Budget (₹)', p?.approvedBudget, true), 3));
    div.appendChild(wrapCol(numberField('previousExpenditure', 'Previous Expenditure (₹)', p?.previousExpenditure, true), 3));
    div.appendChild(wrapCol(numberField('proposedWorkCost', 'Proposed Work Cost (₹)', p?.proposedWorkCost, true), 3));

    // Remaining balance (computed, read-only)
    const rbDiv = document.createElement('div');
    rbDiv.className = 'mb-3';
    rbDiv.innerHTML = `
      <label class="form-label">Remaining Balance (₹)</label>
      <input type="text" class="form-control bg-light" id="field-remainingBalance" readonly
        value="${computeBalance(p?.approvedBudget, p?.previousExpenditure, p?.proposedWorkCost)}">
      <small class="text-muted">Auto-calculated: Budget - Expenditure - Work Cost</small>`;
    div.appendChild(wrapCol(rbDiv, 3));

    return div;
  }));

  // Section 4: Technical Details
  form.appendChild(sectionCard('Technical Details', () => {
    const div = document.createElement('div');
    div.className = 'row g-3';

    div.appendChild(wrapCol(checkField('siteInspectionDone', 'Site Inspection Done', p?.siteInspectionDone), 4));
    div.appendChild(wrapCol(dateField('technicalApprovalDate', 'Technical Approval Date', p?.technicalApprovalDate), 4));
    div.appendChild(wrapCol(textField('technicalApprovalNumber', 'Technical Approval No.', p?.technicalApprovalNumber || ''), 4));
    div.appendChild(wrapCol(numberField('technicalApprovalCost', 'Tech. Approval Cost (₹)', p?.technicalApprovalCost), 4));
    div.appendChild(wrapCol(checkField('competentAuthorityTADone', 'Competent Authority TA Done', p?.competentAuthorityTADone), 4));
    div.appendChild(wrapCol(selectField('procurementMethodId', 'Procurement Method', m.procMethods, p?.procurementMethodId, false), 4));

    return div;
  }));

  // Section 5: Compliance Checks
  form.appendChild(sectionCard('Compliance & Legal Checks', () => {
    const div = document.createElement('div');
    div.className = 'row g-3';

    div.appendChild(wrapCol(checkField('tenderPeriodVerified', 'Tender Period Verified', p?.tenderPeriodVerified), 4));
    div.appendChild(wrapCol(checkField('siteOwnershipVerified', 'Site Ownership Verified', p?.siteOwnershipVerified), 4));
    div.appendChild(wrapCol(checkField('nocObtained', 'NOC Obtained', p?.nocObtained), 4));
    div.appendChild(wrapCol(checkField('legalObstacleExists', 'Legal Obstacle Exists', p?.legalObstacleExists), 4));
    div.appendChild(wrapCol(checkField('courtCasePending', 'Court Case Pending', p?.courtCasePending), 4));

    // Court case details (dual lang)
    const courtDual = createDualLangInput({
      name: 'courtCaseDetails', label: 'Court Case Details', type: 'textarea', required: false,
      valueEn: p?.courtCaseDetails_En || '', valueAlt: p?.courtCaseDetails_Alt || '',
    });
    div.appendChild(wrapCol(courtDual, 12));

    div.appendChild(wrapCol(checkField('auditObjectionExists', 'Audit Objection Exists', p?.auditObjectionExists), 4));

    const auditDual = createDualLangInput({
      name: 'auditObjectionDetails', label: 'Audit Objection Details', type: 'textarea', required: false,
      valueEn: p?.auditObjectionDetails_En || '', valueAlt: p?.auditObjectionDetails_Alt || '',
    });
    div.appendChild(wrapCol(auditDual, 12));

    div.appendChild(wrapCol(checkField('duplicateFundCheckDone', 'Duplicate Fund Check Done', p?.duplicateFundCheckDone), 4));
    div.appendChild(wrapCol(checkField('otherWorkInProgress', 'Other Work In Progress', p?.otherWorkInProgress), 4));

    const otherDual = createDualLangInput({
      name: 'otherWorkDetails', label: 'Other Work Details', type: 'textarea', required: false,
      valueEn: p?.otherWorkDetails_En || '', valueAlt: p?.otherWorkDetails_Alt || '',
    });
    div.appendChild(wrapCol(otherDual, 12));

    div.appendChild(wrapCol(checkField('dlpCheckDone', 'DLP Check Done', p?.dlpCheckDone), 4));
    div.appendChild(wrapCol(checkField('overallComplianceConfirmed', 'Overall Compliance Confirmed', p?.overallComplianceConfirmed), 4));

    return div;
  }));

  container.appendChild(form);

  // Auto-calculate remaining balance
  ['approvedBudget', 'previousExpenditure', 'proposedWorkCost'].forEach(id => {
    const el = form.querySelector(`#field-${id}`);
    if (el) el.addEventListener('input', () => {
      const rb = computeBalance(
        form.querySelector('#field-approvedBudget')?.value,
        form.querySelector('#field-previousExpenditure')?.value,
        form.querySelector('#field-proposedWorkCost')?.value
      );
      const rbField = form.querySelector('#field-remainingBalance');
      if (rbField) rbField.value = rb;
    });
  });

  // Cancel button
  form.querySelector('#btn-cancel')?.addEventListener('click', () => navigate('/proposals'));

  // Submit for approval
  form.querySelector('#btn-submit')?.addEventListener('click', async () => {
    if (!isEdit) return;
    try {
      await api.post(`/proposals/${p.id}/submit`);
      showToast('Proposal submitted for approval', 'success');
      navigate('/proposals');
    } catch (err) {
      showToast(err.message || 'Submit failed', 'danger');
    }
  });

  // Save form
  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!form.checkValidity()) {
      form.classList.add('was-validated');
      return;
    }

    const payload = gatherPayload(form);
    const saveBtn = form.querySelector('#btn-save');
    saveBtn.disabled = true;

    try {
      if (isEdit) {
        payload.id = p.id;
        await api.put(`/proposals/${p.id}`, payload);
        showToast('Proposal saved', 'success');
      } else {
        const result = await api.post('/proposals', payload);
        showToast('Proposal draft created', 'success');
        navigate(`/proposals/${result.id}/edit`);
      }
    } catch (err) {
      showToast(err.message || 'Save failed', 'danger');
    } finally {
      saveBtn.disabled = false;
    }
  });
}

function gatherPayload(form) {
  const val = (id) => form.querySelector(`#field-${id}`)?.value?.trim() || null;
  const num = (id) => { const v = form.querySelector(`#field-${id}`)?.value; return v ? Number(v) : 0; };
  const checked = (id) => form.querySelector(`#field-${id}`)?.checked || false;
  const dual = (name) => ({
    en: form.querySelector(`#dual-${name}-en`)?.value?.trim() || '',
    alt: form.querySelector(`#dual-${name}-alt`)?.value?.trim() || ''
  });

  const subject = dual('subject');
  const brief = dual('briefInfo');
  const court = dual('courtCaseDetails');
  const auditObj = dual('auditObjectionDetails');
  const otherWork = dual('otherWorkDetails');

  return {
    subject_En: subject.en, subject_Alt: subject.alt,
    departmentId: val('departmentId'),
    submitterDesignationId: val('designationId'),
    fundTypeId: val('fundTypeId'),
    fundYear: val('fundYear'),
    referenceNumber: val('referenceNumber'),
    wardId: val('wardId') || null,
    briefInfo_En: brief.en, briefInfo_Alt: brief.alt,
    estimatedCost: num('estimatedCost'),
    accountHeadId: val('accountHeadId'),
    approvedBudget: num('approvedBudget'),
    previousExpenditure: num('previousExpenditure'),
    proposedWorkCost: num('proposedWorkCost'),
    siteInspectionDone: checked('siteInspectionDone'),
    technicalApprovalDate: val('technicalApprovalDate') || null,
    technicalApprovalNumber: val('technicalApprovalNumber') || null,
    technicalApprovalCost: num('technicalApprovalCost') || null,
    competentAuthorityTADone: checked('competentAuthorityTADone'),
    procurementMethodId: val('procurementMethodId') || null,
    tenderPublicationPeriodId: null,
    tenderPeriodVerified: checked('tenderPeriodVerified'),
    siteOwnershipVerified: checked('siteOwnershipVerified'),
    nocObtained: checked('nocObtained'),
    legalObstacleExists: checked('legalObstacleExists'),
    courtCasePending: checked('courtCasePending'),
    courtCaseDetails_En: court.en || null, courtCaseDetails_Alt: court.alt || null,
    auditObjectionExists: checked('auditObjectionExists'),
    auditObjectionDetails_En: auditObj.en || null, auditObjectionDetails_Alt: auditObj.alt || null,
    duplicateFundCheckDone: checked('duplicateFundCheckDone'),
    otherWorkInProgress: checked('otherWorkInProgress'),
    otherWorkDetails_En: otherWork.en || null, otherWorkDetails_Alt: otherWork.alt || null,
    dlpCheckDone: checked('dlpCheckDone'),
    overallComplianceConfirmed: checked('overallComplianceConfirmed'),
    competentAuthorityId: null,
  };
}

// ── Helper renderers ──────────

function sectionCard(title, contentFn) {
  const card = document.createElement('div');
  card.className = 'card mb-3';
  const cardHeader = document.createElement('div');
  cardHeader.className = 'card-header';
  cardHeader.innerHTML = `<h6 class="mb-0">${escapeHtml(title)}</h6>`;
  const cardBody = document.createElement('div');
  cardBody.className = 'card-body';
  cardBody.appendChild(contentFn());
  card.appendChild(cardHeader);
  card.appendChild(cardBody);
  return card;
}

function wrapCol(el, size = 6) {
  const col = document.createElement('div');
  col.className = `col-md-${size}`;
  col.appendChild(el);
  return col;
}

function textField(id, label, value = '', required = false, placeholder = '') {
  const div = document.createElement('div');
  div.className = 'mb-3';
  div.innerHTML = `
    <label for="field-${id}" class="form-label">${escapeHtml(label)} ${required ? '<span class="text-danger">*</span>' : ''}</label>
    <input type="text" class="form-control" id="field-${id}" value="${escapeAttr(value)}"
      placeholder="${escapeAttr(placeholder)}" ${required ? 'required' : ''}>
    <div class="invalid-feedback">Required.</div>`;
  return div;
}

function numberField(id, label, value = null, required = false) {
  const div = document.createElement('div');
  div.className = 'mb-3';
  div.innerHTML = `
    <label for="field-${id}" class="form-label">${escapeHtml(label)} ${required ? '<span class="text-danger">*</span>' : ''}</label>
    <input type="number" class="form-control" id="field-${id}" step="0.01"
      value="${value != null ? value : ''}" ${required ? 'required' : ''}>
    <div class="invalid-feedback">Required.</div>`;
  return div;
}

function dateField(id, label, value = null) {
  const div = document.createElement('div');
  div.className = 'mb-3';
  const dateVal = value ? new Date(value).toISOString().split('T')[0] : '';
  div.innerHTML = `
    <label for="field-${id}" class="form-label">${escapeHtml(label)}</label>
    <input type="date" class="form-control" id="field-${id}" value="${dateVal}">`;
  return div;
}

function checkField(id, label, checked = false) {
  const div = document.createElement('div');
  div.className = 'mb-3 form-check form-switch';
  div.innerHTML = `
    <input class="form-check-input" type="checkbox" role="switch" id="field-${id}" ${checked ? 'checked' : ''}>
    <label class="form-check-label" for="field-${id}">${escapeHtml(label)}</label>`;
  return div;
}

function selectField(id, label, items, selectedId, required = false, valueKey = 'id', labelKey = 'name_En') {
  const div = document.createElement('div');
  div.className = 'mb-3';
  const opts = items.map(i =>
    `<option value="${i[valueKey]}" ${String(i[valueKey]) === String(selectedId) ? 'selected' : ''}>${escapeHtml(i[labelKey])}</option>`
  ).join('');
  div.innerHTML = `
    <label for="field-${id}" class="form-label">${escapeHtml(label)} ${required ? '<span class="text-danger">*</span>' : ''}</label>
    <select class="form-select" id="field-${id}" ${required ? 'required' : ''}>
      <option value="">Select...</option>
      ${opts}
    </select>
    <div class="invalid-feedback">Required.</div>`;
  return div;
}

function computeBalance(budget, expenditure, workCost) {
  const b = Number(budget) || 0;
  const e = Number(expenditure) || 0;
  const w = Number(workCost) || 0;
  return (b - e - w).toFixed(2);
}

function getCurrentFundYear() {
  const now = new Date();
  const y = now.getMonth() >= 3 ? now.getFullYear() : now.getFullYear() - 1;
  return `${y}-${String(y + 1).slice(2)}`;
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str || '';
  return div.innerHTML;
}

function escapeAttr(str) {
  return (str || '').replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}
