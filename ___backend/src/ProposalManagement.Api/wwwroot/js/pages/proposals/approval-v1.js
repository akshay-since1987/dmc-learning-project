/**
 * Approval Console V1
 * Officer views read-only proposal, adds note/remark, generates PDF,
 * overlays signature, signs, then approves or pushes back.
 */

import api from '../../api.js';
import { getCurrentUser } from '../../auth.js';
import { showToast } from '../../toast.js';
import { renderSignatureOverlay } from '../../signature-overlay.js';
import { navigate } from '../../router.js';
import { translateDOM } from '../../i18n.js';

const STAGE_LABELS = {
  Draft: 'Draft',
  Submitted: 'Submitted',
  AtCityEngineer: 'At City Engineer',
  AtADO: 'At ADO',
  AtChiefAccountant: 'At Chief Accountant',
  AtDeputyCommissioner: 'At Dy. Commissioner',
  AtCommissioner: 'At Commissioner',
  PushedBack: 'Pushed Back',
  Approved: 'Approved',
  Cancelled: 'Cancelled'
};

const PUSHBACK_TARGETS = {
  AtCityEngineer: ['Draft'],
  AtADO: ['Draft'],
  AtChiefAccountant: ['Draft', 'AtCityEngineer'],
  AtDeputyCommissioner: ['Draft', 'AtCityEngineer', 'AtChiefAccountant'],
  AtCommissioner: ['Draft', 'AtCityEngineer', 'AtChiefAccountant', 'AtDeputyCommissioner']
};

const APPROVAL_TERMS_EN = [
  'I have reviewed the complete proposal details, attached records, and prior stage remarks.',
  'I confirm that this approval is being granted only after applying all applicable rules, circulars, and directives.',
  'I accept accountability for this approval decision as per my official role and delegation.'
];

const APPROVAL_TERMS_ALT = [
  'मी संपूर्ण प्रस्ताव तपशील, जोडलेली कागदपत्रे आणि मागील टप्प्यातील नोंदी तपासल्या आहेत.',
  'लागू असलेल्या नियम, शासन निर्देश आणि संबंधित अटींचे पालन करूनच ही मान्यता देत आहे.',
  'माझ्या अधिकृत भूमिकेनुसार या मान्यतेच्या निर्णयाची जबाबदारी मी स्वीकारतो/स्वीकारते.'
];

export async function renderApprovalConsole(container, proposalId) {
  container.innerHTML = `<div class="loading-container"><div class="spinner" role="status"><span class="visually-hidden">Loading...</span></div></div>`;

  const user = getCurrentUser();
  let proposal, history;

  try {
    [proposal, history] = await Promise.all([
      api.get(`/v1/proposals/${proposalId}`),
      api.get(`/workflow/${proposalId}/history`)
    ]);
  } catch (err) {
    container.innerHTML = `<div class="alert alert-danger">${err.message || 'Failed to load proposal'}</div>`;
    return;
  }

  const stage = proposal.currentStage;
  const canApprove = isStageHandler(stage, user.role);
  const isProposer = proposal.submittedById === user.id;
  const targets = PUSHBACK_TARGETS[stage] || [];
  let currentPdfPath = proposal.latestSignedPdfPath || proposal.submitterSignedPdfPath || null;
  let pendingSignature = null;

  container.innerHTML = `
    <div class="mb-3">
      <button class="btn btn-sm btn-outline-secondary" id="btn-back"><i class="bi bi-arrow-left me-1"></i>Back</button>
    </div>

    <div class="card mb-3">
      <div class="card-header d-flex justify-content-between align-items-center">
        <div>
          <h5 class="mb-0">${proposal.proposalNumber} — ${proposal.subject_En || ''}</h5>
          <small class="text-muted">${proposal.subject_Alt || ''}</small>
        </div>
        <span class="badge bg-${stageBadgeColor(stage)}">${STAGE_LABELS[stage] || stage}</span>
      </div>
      <div class="card-body" id="proposal-fields-area"></div>
    </div>

    <!-- Timeline -->
    <div class="card mb-3">
      <div class="card-header"><h6 class="mb-0"><i class="bi bi-clock-history me-1"></i>Approval History</h6></div>
      <div class="card-body" id="timeline-area"></div>
    </div>

    ${canApprove || isProposer ? `
    <!-- Action Panel -->
    <div class="card mb-3" id="action-panel">
      <div class="card-header bg-primary text-white">
        <h6 class="mb-0"><i class="bi bi-pen me-1"></i>Note & Sign</h6>
      </div>
      <div class="card-body">
        <div class="row g-3 mb-3">
          <div class="col-md-6">
            <label class="form-label" for="opinion-en">Opinion (English)</label>
            <textarea class="form-control" id="opinion-en" rows="3" placeholder="Enter your opinion..."></textarea>
          </div>
          <div class="col-md-6">
            <label class="form-label" for="opinion-alt">Opinion (मराठी)</label>
            <textarea class="form-control" id="opinion-alt" rows="3" placeholder="तुमचे मत लिहा..."></textarea>
          </div>
        </div>
        <div class="row g-3 mb-3">
          <div class="col-md-6">
            <label class="form-label" for="remarks-en">Remarks (English)</label>
            <textarea class="form-control" id="remarks-en" rows="2"></textarea>
          </div>
          <div class="col-md-6">
            <label class="form-label" for="remarks-alt">Remarks (मराठी)</label>
            <textarea class="form-control" id="remarks-alt" rows="2"></textarea>
          </div>
        </div>

        ${canApprove ? `
        <div class="card border-info mb-3" id="approval-terms-card">
          <div class="card-header bg-info bg-opacity-10">
            <h6 class="mb-0"><i class="bi bi-shield-check me-1"></i>Approval Terms / मान्यता अटी</h6>
          </div>
          <div class="card-body">
            <div class="row g-3">
              <div class="col-md-6">
                <h6 class="small text-uppercase text-muted">English</h6>
                <ol class="mb-0 small">
                  ${APPROVAL_TERMS_EN.map(t => `<li>${escapeHtml(t)}</li>`).join('')}
                </ol>
              </div>
              <div class="col-md-6" lang="mr">
                <h6 class="small text-uppercase text-muted">मराठी</h6>
                <ol class="mb-0 small">
                  ${APPROVAL_TERMS_ALT.map(t => `<li>${escapeHtml(t)}</li>`).join('')}
                </ol>
              </div>
            </div>
            <hr>
            <div class="form-check">
              <input class="form-check-input" type="checkbox" id="agree-terms">
              <label class="form-check-label" for="agree-terms">
                I agree to the terms / मी वरील अटी मान्य करतो/करते
              </label>
            </div>
          </div>
        </div>` : ''}

        <div class="d-flex gap-2 flex-wrap">
          <button class="btn btn-outline-primary" id="btn-open-pdf"><i class="bi bi-vector-pen me-1"></i>Open PDF & Sign</button>
          ${canApprove ? `<button class="btn btn-success" id="btn-approve" disabled><i class="bi bi-check-lg me-1"></i>Approve & Forward</button>` : ''}
          ${targets.length > 0 ? `
          <div class="d-flex align-items-center gap-2">
            <select class="form-select form-select-sm" id="pushback-target" style="width:auto">
              <option value="">Push back to...</option>
              ${targets.map(t => `<option value="${t}">${STAGE_LABELS[t] || t}</option>`).join('')}
            </select>
            <button class="btn btn-warning btn-sm" id="btn-pushback"><i class="bi bi-arrow-return-left me-1"></i>Push Back</button>
          </div>` : ''}
        </div>
        <div id="approval-readiness" class="small text-muted mt-2"></div>
      </div>
    </div>

    <!-- Pushback reason (shown when pushback selected) -->
    <div class="card mb-3 d-none" id="pushback-reason-card">
      <div class="card-header bg-warning"><h6 class="mb-0">Push-back Reason (Mandatory)</h6></div>
      <div class="card-body">
        <div class="row g-3">
          <div class="col-md-6">
            <label class="form-label" for="reason-en">Reason (English)</label>
            <textarea class="form-control" id="reason-en" rows="3" required></textarea>
          </div>
          <div class="col-md-6">
            <label class="form-label" for="reason-alt">Reason (मराठी)</label>
            <textarea class="form-control" id="reason-alt" rows="3"></textarea>
          </div>
        </div>
        <button class="btn btn-warning mt-3" id="btn-confirm-pushback"><i class="bi bi-arrow-return-left me-1"></i>Confirm Push Back</button>
      </div>
    </div>
    ` : ''}

    <div class="card mb-3" id="existing-pdf-card">
      <div class="card-header"><h6 class="mb-0"><i class="bi bi-file-earmark-pdf me-1"></i>Current Proposal PDF</h6></div>
      <div class="card-body" id="existing-pdf-body"></div>
    </div>

    <!-- PDF Viewer & Signature overlay area -->
    <div class="card mb-3 d-none" id="pdf-viewer-card">
      <div class="card-header"><h6 class="mb-0"><i class="bi bi-file-earmark-pdf me-1"></i>Stage Document — Sign Here</h6></div>
      <div class="card-body p-0" id="pdf-viewer-area"></div>
    </div>
  `;

  // ── Render read-only fields ──
  renderProposalFields(container.querySelector('#proposal-fields-area'), proposal);
  renderTimeline(container.querySelector('#timeline-area'), history);
  translateDOM(container);

  const makePdfDownloadUrl = (path) => `/api/v1/proposals/${proposalId}/pdf/download?path=${encodeURIComponent(path)}`;

  const approveBtn = container.querySelector('#btn-approve');
  const termsCheckbox = container.querySelector('#agree-terms');
  const readiness = container.querySelector('#approval-readiness');
  const openPdfBtn = container.querySelector('#btn-open-pdf');
  const existingPdfBody = container.querySelector('#existing-pdf-body');

  const renderExistingPdf = () => {
    if (!existingPdfBody) return;
    if (!currentPdfPath) {
      existingPdfBody.innerHTML = '<div class="alert alert-warning mb-0">Submitter signed PDF is not available yet. Approver cannot sign or approve until proposer submits with signature.</div>';
      if (openPdfBtn) openPdfBtn.disabled = true;
      return;
    }

    if (openPdfBtn) openPdfBtn.disabled = false;
    const pdfUrl = makePdfDownloadUrl(currentPdfPath);
    existingPdfBody.innerHTML = `
      <div class="d-flex justify-content-between align-items-center mb-2 flex-wrap gap-2">
        <small class="text-muted">Path: ${escapeHtml(currentPdfPath)}</small>
        <a class="btn btn-sm btn-outline-secondary" href="${pdfUrl}" target="_blank" rel="noopener">
          <i class="bi bi-box-arrow-up-right me-1"></i>Open in new tab
        </a>
      </div>
      <iframe title="Current proposal PDF" src="${pdfUrl}" style="width:100%;height:520px;border:1px solid #dee2e6;border-radius:8px"></iframe>
    `;
  };

  const updateReadiness = () => {
    if (!approveBtn || !readiness) return;
    if (!canApprove) {
      approveBtn.disabled = true;
      readiness.textContent = 'You are not allowed to approve at this stage.';
      readiness.className = 'small text-muted mt-2';
      return;
    }

    const termsAccepted = !!termsCheckbox?.checked;
    const signed = !!pendingSignature;
    approveBtn.disabled = !(termsAccepted && signed);

    if (!currentPdfPath) {
      readiness.textContent = 'Submitter signed PDF is required before approver can sign and approve.';
      readiness.className = 'small text-warning mt-2';
      return;
    }

    if (!signed) {
      readiness.textContent = 'Please place your signature on the PDF.';
      readiness.className = 'small text-warning mt-2';
      return;
    }

    if (!termsAccepted) {
      readiness.textContent = 'Please agree to the terms to enable approval.';
      readiness.className = 'small text-warning mt-2';
      return;
    }

    readiness.textContent = 'Ready for approval.';
    readiness.className = 'small text-success mt-2';
  };

  renderExistingPdf();
  updateReadiness();

  // ── Back ──
  container.querySelector('#btn-back')?.addEventListener('click', () => navigate('/approvals'));

  termsCheckbox?.addEventListener('change', updateReadiness);

  // ── Open PDF signer ──
  container.querySelector('#btn-open-pdf')?.addEventListener('click', async () => {
    if (!currentPdfPath) {
      showToast('Generate PDF before signing', 'warning');
      return;
    }

    const sigUrl = user.signaturePath ? user.signaturePath : null;
    if (!sigUrl) {
      showToast('No signature uploaded. Please upload signature in Profile first.', 'warning');
      return;
    }

    const pdfCard = container.querySelector('#pdf-viewer-card');
    const pdfArea = container.querySelector('#pdf-viewer-area');
    if (!pdfCard || !pdfArea) return;

    pdfCard.classList.remove('d-none');
    try {
      await renderSignatureOverlay(
        pdfArea,
        makePdfDownloadUrl(currentPdfPath),
        sigUrl,
        (meta) => {
          pendingSignature = { ...meta, generatedPdfPath: currentPdfPath };
          pdfCard.classList.add('d-none');
          showToast('Signature placement captured', 'success');
          updateReadiness();
        },
        () => {
          pdfCard.classList.add('d-none');
        }
      );
    } catch (err) {
      pdfCard.classList.add('d-none');
      showToast(err.message || 'Failed to open PDF signer', 'danger');
    }
  });

  // ── Approve ──
  container.querySelector('#btn-approve')?.addEventListener('click', async () => {
    const btn = container.querySelector('#btn-approve');
    const termsAccepted = !!termsCheckbox?.checked;
    if (!pendingSignature) {
      showToast('Signature on PDF is mandatory before approval', 'warning');
      return;
    }
    if (!termsAccepted) {
      showToast('Please agree to terms before approval', 'warning');
      return;
    }

    btn.disabled = true;
    try {
      const body = {
        termsAccepted,
        opinion_En: container.querySelector('#opinion-en')?.value?.trim() || null,
        opinion_Alt: container.querySelector('#opinion-alt')?.value?.trim() || null,
        remarks_En: container.querySelector('#remarks-en')?.value?.trim() || null,
        remarks_Alt: container.querySelector('#remarks-alt')?.value?.trim() || null,
      };
      const approveResult = await api.post(`/workflow/${proposalId}/approve`, body);

      // Save signature with the history entry from the approve result
      if (pendingSignature && approveResult?.historyId) {
        await api.post(`/v1/proposals/${proposalId}/pdf/sign`, {
          stageHistoryId: approveResult.historyId,
          ...pendingSignature
        });
      }
      pendingSignature = null;

      showToast('Proposal approved and forwarded!', 'success');
      navigate('/approvals');
    } catch (err) {
      showToast(err.message || 'Approval failed', 'danger');
      btn.disabled = false;
      updateReadiness();
    }
  });

  // ── Push Back ──
  container.querySelector('#pushback-target')?.addEventListener('change', (e) => {
    const card = container.querySelector('#pushback-reason-card');
    if (e.target.value) {
      card.classList.remove('d-none');
    } else {
      card.classList.add('d-none');
    }
  });

  container.querySelector('#btn-pushback')?.addEventListener('click', () => {
    const target = container.querySelector('#pushback-target')?.value;
    if (!target) { showToast('Select a stage to push back to', 'warning'); return; }
    container.querySelector('#pushback-reason-card').classList.remove('d-none');
  });

  container.querySelector('#btn-confirm-pushback')?.addEventListener('click', async () => {
    const target = container.querySelector('#pushback-target')?.value;
    const reasonEn = container.querySelector('#reason-en')?.value?.trim();
    if (!target) { showToast('Select push-back target', 'warning'); return; }
    if (!reasonEn) { showToast('Push-back reason is mandatory', 'warning'); return; }

    const btn = container.querySelector('#btn-confirm-pushback');
    btn.disabled = true;
    try {
      const body = {
        targetStage: target,
        reason_En: reasonEn,
        reason_Alt: container.querySelector('#reason-alt')?.value?.trim() || null,
        opinion_En: container.querySelector('#opinion-en')?.value?.trim() || null,
        opinion_Alt: container.querySelector('#opinion-alt')?.value?.trim() || null,
        remarks_En: container.querySelector('#remarks-en')?.value?.trim() || null,
        remarks_Alt: container.querySelector('#remarks-alt')?.value?.trim() || null,
      };
      await api.post(`/workflow/${proposalId}/pushback`, body);
      showToast('Proposal pushed back successfully', 'success');
      navigate('/approvals');
    } catch (err) {
      showToast(err.message || 'Push-back failed', 'danger');
      btn.disabled = false;
    }
  });
}

// ── Helpers ──

function isStageHandler(stage, role) {
  const map = {
    AtCityEngineer: 'CityEngineer',
    AtADO: 'ADO',
    AtChiefAccountant: 'ChiefAccountant',
    AtDeputyCommissioner: 'DeputyCommissioner',
    AtCommissioner: 'Commissioner',
  };
  return map[stage] === role || role === 'Lotus';
}

function stageBadgeColor(stage) {
  switch (stage) {
    case 'Draft': return 'secondary';
    case 'Submitted': case 'AtCityEngineer': case 'AtADO': return 'info';
    case 'AtChiefAccountant': case 'AtDeputyCommissioner': return 'primary';
    case 'AtCommissioner': return 'warning';
    case 'Approved': return 'success';
    case 'PushedBack': return 'danger';
    case 'Cancelled': return 'dark';
    default: return 'secondary';
  }
}

function renderProposalFields(area, p) {
  const field = (label, val) => `
    <div class="col-md-6">
      <div class="mb-2">
        <small class="text-muted">${label}</small>
        <div class="fw-medium">${val || '—'}</div>
      </div>
    </div>`;

  const money = (v) => v != null ? `₹ ${Number(v).toLocaleString('en-IN', { minimumFractionDigits: 2 })}` : '—';
  const yesNo = (v) => v ? '<span class="text-success">Yes</span>' : '<span class="text-danger">No</span>';

  area.innerHTML = `
    <h6 class="text-primary mb-2">Basic Information</h6>
    <div class="row g-2">
      ${field('Department', p.departmentName_En)}
      ${field('Proposer', p.submittedByName_En)}
      ${field('Proposer (Alt)', p.submittedByName_Alt || '—')}
      ${field('Designation', p.submitterDesignationName_En || '—')}
      ${field('Subject (EN)', p.subject_En)}
      ${field('Subject (मराठी)', p.subject_Alt)}
      ${field('Brief Info (EN)', p.briefInfo_En || '—')}
      ${field('Brief Info (Alt)', p.briefInfo_Alt || '—')}
      ${field('Reference', p.referenceNumber)}
      ${field('Fund Type', p.fundTypeName_En)}
      ${field('Fund Year', p.fundYear)}
      ${field('Account Head', p.accountHeadName_En)}
      ${field('Ward', p.wardName_En)}
      ${field('Estimated Cost', money(p.estimatedCost))}
      ${field('Date', p.date)}
    </div>
    <hr>
    <h6 class="text-primary mb-2">Technical & Publishing</h6>
    <div class="row g-2">
      ${field('Site Inspection', yesNo(p.siteInspectionDone))}
      ${field('TS Date', p.technicalApprovalDate || '—')}
      ${field('TS Number', p.technicalApprovalNumber || '—')}
      ${field('TS Cost', money(p.technicalApprovalCost))}
      ${field('Competent Authority TA Done', yesNo(p.competentAuthorityTADone))}
      ${field('Procurement Method', p.procurementMethodName_En)}
      ${field('Publication Days', p.publicationDays ?? '—')}
      ${field('Tender Period Verified', yesNo(p.tenderPeriodVerified))}
      ${field('Site Ownership Verified', yesNo(p.siteOwnershipVerified))}
    </div>
    <hr>
    <h6 class="text-primary mb-2">Accounting & Compliance</h6>
    <div class="row g-2">
      ${field('Previous Expenditure', money(p.previousExpenditure))}
      ${field('Approved Budget', money(p.approvedBudget))}
      ${field('Proposed Work Cost', money(p.proposedWorkCost))}
      ${field('Remaining Balance', money(p.remainingBalance))}
      ${field('NOC Obtained', yesNo(p.nocObtained))}
      ${field('Legal Obstacle Exists', yesNo(p.legalObstacleExists))}
      ${field('Court Case Pending', yesNo(p.courtCasePending))}
      ${field('Court Case Details (EN)', p.courtCaseDetails_En || '—')}
      ${field('Court Case Details (Alt)', p.courtCaseDetails_Alt || '—')}
      ${field('Audit Objection Exists', yesNo(p.auditObjectionExists))}
      ${field('Audit Objection Details (EN)', p.auditObjectionDetails_En || '—')}
      ${field('Audit Objection Details (Alt)', p.auditObjectionDetails_Alt || '—')}
      ${field('Duplicate Fund Check', yesNo(p.duplicateFundCheckDone))}
      ${field('Other Work In Progress', yesNo(p.otherWorkInProgress))}
      ${field('Other Work Details (EN)', p.otherWorkDetails_En || '—')}
      ${field('Other Work Details (Alt)', p.otherWorkDetails_Alt || '—')}
      ${field('DLP Check Done', yesNo(p.dlpCheckDone))}
      ${field('Overall Compliance', yesNo(p.overallComplianceConfirmed))}
    </div>
  `;
}

function renderTimeline(area, history) {
  if (!history || history.length === 0) {
    area.innerHTML = '<p class="text-muted mb-0">No history yet.</p>';
    return;
  }
  const entries = Array.isArray(history) ? history : (history.items || history.data || []);
  area.innerHTML = `
    <div class="timeline-list">
      ${entries.map(h => {
        const isPushback = h.action === 'PushBack';
        const colorClass = isPushback ? 'border-danger' : (h.action === 'Approve' ? 'border-success' : 'border-info');
        return `
          <div class="card mb-2 border-start border-3 ${colorClass}">
            <div class="card-body py-2 px-3">
              <div class="d-flex justify-content-between">
                <div>
                  <strong>${h.action}</strong>
                  <span class="text-muted"> — ${h.actionByName_En || ''} (${h.actionByDesignation_En || ''})</span>
                </div>
                <small class="text-muted">${new Date(h.createdAt).toLocaleString()}</small>
              </div>
              <div class="small text-muted">${h.fromStage || ''} → ${h.toStage || ''}</div>
              ${h.opinion_En ? `<div class="mt-1 small"><strong>Opinion:</strong> ${escapeHtml(h.opinion_En)}</div>` : ''}
              ${h.remarks_En ? `<div class="small"><strong>Remarks:</strong> ${escapeHtml(h.remarks_En)}</div>` : ''}
              ${isPushback && h.reason_En ? `<div class="small text-danger"><strong>Reason:</strong> ${escapeHtml(h.reason_En)}</div>` : ''}
              ${h.dscSignedAt ? `<div class="small text-success"><i class="bi bi-patch-check"></i> Signed ${new Date(h.dscSignedAt).toLocaleString()}</div>` : ''}
            </div>
          </div>`;
      }).join('')}
    </div>`;
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str;
  return div.innerHTML;
}
