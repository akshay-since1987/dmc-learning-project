/** Proposal detail view — with workflow actions (approve/pushback) + timeline */
import api from '../../api.js';
import { showToast } from '../../toast.js';
import { showModal } from '../../modal.js';
import { navigate } from '../../router.js';
import { getUserRole, getCurrentUser } from '../../auth.js';

const STAGE_BADGES = {
  Draft: 'bg-secondary', Submitted: 'bg-info', AtCityEngineer: 'bg-primary',
  AtChiefAccountant: 'bg-warning text-dark', AtDeputyCommissioner: 'bg-info',
  AtCommissioner: 'bg-dark', PushedBack: 'bg-danger', Approved: 'bg-success', Cancelled: 'bg-secondary',
};

const STAGE_LABELS = {
  Draft: 'Draft', Submitted: 'Submitted', AtCityEngineer: 'City Engineer',
  AtChiefAccountant: 'Chief Accountant', AtDeputyCommissioner: 'Dy. Commissioner',
  AtCommissioner: 'Commissioner', PushedBack: 'Pushed Back', Approved: 'Approved', Cancelled: 'Cancelled',
};

/** Which role can act at which stage */
const STAGE_ROLE_MAP = {
  AtCityEngineer: 'CityEngineer',
  AtChiefAccountant: 'ChiefAccountant',
  AtDeputyCommissioner: 'DeputyCommissioner',
  AtCommissioner: 'Commissioner',
};

/** PushBack target options per stage */
const PUSHBACK_TARGETS = {
  AtCityEngineer: ['Draft'],
  AtChiefAccountant: ['Draft', 'AtCityEngineer'],
  AtDeputyCommissioner: ['Draft', 'AtCityEngineer', 'AtChiefAccountant'],
  AtCommissioner: ['Draft', 'AtCityEngineer', 'AtChiefAccountant', 'AtDeputyCommissioner'],
};

const ACTION_ICONS = { Submit: 'bi-send', Approve: 'bi-check-circle', PushBack: 'bi-arrow-return-left', Resubmit: 'bi-send', Cancel: 'bi-x-circle' };
const ACTION_COLORS = { Submit: 'info', Approve: 'success', PushBack: 'danger', Resubmit: 'info', Cancel: 'secondary' };

export async function renderProposalDetail(container, proposalId) {
  container.innerHTML = `<div class="text-center py-5"><div class="spinner-border text-primary"></div></div>`;

  try {
    const [p, history] = await Promise.all([
      api.get(`/proposals/${proposalId}`),
      api.get(`/workflow/${proposalId}/history`).catch(() => []),
    ]);
    buildDetail(container, p, history);
  } catch (err) {
    container.innerHTML = `<div class="alert alert-danger">${err.message || 'Failed to load proposal'}</div>`;
  }
}

function buildDetail(container, p, history) {
  const role = getUserRole();
  const canEdit = (p.currentStage === 'Draft' || p.currentStage === 'PushedBack') && role !== 'Auditor';
  const isHandler = role === 'Lotus' || STAGE_ROLE_MAP[p.currentStage] === role;
  const canAct = isHandler && !['Draft', 'Approved', 'Cancelled', 'PushedBack'].includes(p.currentStage);

  container.innerHTML = `
    <div class="d-flex justify-content-between align-items-start mb-4 flex-wrap gap-2">
      <div>
        <h5 class="mb-1">${esc(p.proposalNumber)}</h5>
        <p class="text-muted mb-0">${esc(p.subject_En)}</p>
        ${p.subject_Alt ? `<p class="text-muted mb-0" lang="mr">${esc(p.subject_Alt)}</p>` : ''}
      </div>
      <div class="d-flex gap-2 align-items-center flex-wrap">
        <span class="badge ${STAGE_BADGES[p.currentStage] || 'bg-secondary'} fs-6">
          ${STAGE_LABELS[p.currentStage] || p.currentStage}
        </span>
        ${p.pushBackCount > 0 ? `<span class="badge bg-danger fs-6" title="Push-back count"><i class="bi bi-arrow-return-left me-1"></i>${p.pushBackCount}</span>` : ''}
        ${canEdit ? `<button class="btn btn-outline-primary btn-sm" id="btn-edit"><i class="bi bi-pencil me-1"></i>Edit</button>` : ''}
        <button class="btn btn-outline-secondary btn-sm" id="btn-back"><i class="bi bi-arrow-left me-1"></i>Back</button>
      </div>
    </div>

    ${canAct ? `
    <!-- Workflow Actions -->
    <div class="card mb-3 border-primary">
      <div class="card-header bg-primary bg-opacity-10">
        <h6 class="mb-0"><i class="bi bi-lightning me-2"></i>Workflow Actions</h6>
      </div>
      <div class="card-body d-flex gap-3 flex-wrap">
        <button class="btn btn-success d-flex align-items-center gap-2" id="btn-approve">
          <i class="bi bi-check-circle-fill"></i> Approve & Forward
        </button>
        <button class="btn btn-danger d-flex align-items-center gap-2" id="btn-pushback">
          <i class="bi bi-arrow-return-left"></i> Push Back
        </button>
      </div>
    </div>` : ''}

    <!-- Detail cards -->
    <div class="row g-3">
      ${detailCard('Basic Information', [
        row('Department', p.departmentName_En, p.departmentName_Alt),
        row('Submitted By', p.submittedByName_En, p.submittedByName_Alt),
        row('Designation', p.submitterDesignationName_En, p.submitterDesignationName_Alt),
        row('Fund Type', p.fundTypeName_En),
        row('Fund Year', p.fundYear),
        row('Reference No.', p.referenceNumber),
        row('Ward', p.wardName_En || '—'),
        row('Date', formatDate(p.date)),
      ])}

      ${detailCard('Brief Information', [
        row('English', p.briefInfo_En),
        row('मराठी', p.briefInfo_Alt, null, 'mr'),
      ])}

      ${detailCard('Financial Details', [
        row('Estimated Cost', currency(p.estimatedCost)),
        row('Account Head', p.accountHeadName_En),
        row('Approved Budget', currency(p.approvedBudget)),
        row('Previous Expenditure', currency(p.previousExpenditure)),
        row('Proposed Work Cost', currency(p.proposedWorkCost)),
        row('Remaining Balance', currency(p.remainingBalance), null, null, p.remainingBalance < 0 ? 'text-danger fw-bold' : 'text-success fw-bold'),
      ])}

      ${detailCard('Technical Details', [
        row('Site Inspection', bool(p.siteInspectionDone)),
        row('Tech. Approval Date', p.technicalApprovalDate ? formatDate(p.technicalApprovalDate) : '—'),
        row('Tech. Approval No.', p.technicalApprovalNumber || '—'),
        row('Tech. Approval Cost', p.technicalApprovalCost != null ? currency(p.technicalApprovalCost) : '—'),
        row('Competent Authority TA', bool(p.competentAuthorityTADone)),
        row('Procurement Method', p.procurementMethodName_En || '—'),
      ])}

      ${detailCard('Compliance & Legal', [
        row('Tender Period Verified', bool(p.tenderPeriodVerified)),
        row('Site Ownership Verified', bool(p.siteOwnershipVerified)),
        row('NOC Obtained', bool(p.nocObtained)),
        row('Legal Obstacle', bool(p.legalObstacleExists)),
        row('Court Case Pending', bool(p.courtCasePending)),
        p.courtCaseDetails_En ? row('Court Case Details', p.courtCaseDetails_En) : null,
        row('Audit Objection', bool(p.auditObjectionExists)),
        p.auditObjectionDetails_En ? row('Objection Details', p.auditObjectionDetails_En) : null,
        row('Duplicate Fund Check', bool(p.duplicateFundCheckDone)),
        row('Other Work In Progress', bool(p.otherWorkInProgress)),
        p.otherWorkDetails_En ? row('Other Work Details', p.otherWorkDetails_En) : null,
        row('DLP Check', bool(p.dlpCheckDone)),
        row('Overall Compliance', bool(p.overallComplianceConfirmed)),
      ].filter(Boolean))}

      ${detailCard('Metadata', [
        row('Push Back Count', String(p.pushBackCount)),
        row('Created', formatDateTime(p.createdAt)),
        row('Last Updated', formatDateTime(p.updatedAt)),
      ])}
    </div>

    <!-- Approval Timeline -->
    <div class="card mt-3">
      <div class="card-header">
        <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>Approval Timeline</h6>
      </div>
      <div class="card-body" id="timeline-container"></div>
    </div>
  `;

  // Bind buttons
  container.querySelector('#btn-back')?.addEventListener('click', () => navigate('/proposals'));
  container.querySelector('#btn-edit')?.addEventListener('click', () => navigate(`/proposals/${p.id}/edit`));
  container.querySelector('#btn-approve')?.addEventListener('click', () => showApproveModal(p));
  container.querySelector('#btn-pushback')?.addEventListener('click', () => showPushBackModal(p));

  // Render timeline
  renderTimeline(container.querySelector('#timeline-container'), history);
}

// ── Timeline rendering ─────────────────────────────────────────────

function renderTimeline(container, history) {
  if (!history || history.length === 0) {
    container.innerHTML = `<div class="text-center py-3 text-muted"><i class="bi bi-clock" style="font-size:1.5rem;opacity:0.4"></i><p class="mt-2">No workflow history yet</p></div>`;
    return;
  }

  const timeline = document.createElement('div');
  timeline.className = 'timeline';

  for (const h of history) {
    const action = h.action || 'Submit';
    const color = ACTION_COLORS[action] || 'primary';
    const icon = ACTION_ICONS[action] || 'bi-circle';

    const item = document.createElement('div');
    item.className = 'timeline-item';

    item.innerHTML = `
      <div class="timeline-dot ${action.toLowerCase()}"></div>
      <div class="timeline-content">
        <div class="timeline-header">
          <div>
            <span class="badge bg-${color} me-2">${esc(action)}</span>
            <strong>${esc(STAGE_LABELS[h.fromStage] || h.fromStage)}</strong>
            <i class="bi bi-arrow-right mx-1"></i>
            <strong>${esc(STAGE_LABELS[h.toStage] || h.toStage)}</strong>
          </div>
          <span class="timeline-time">${formatDateTime(h.createdAt)}</span>
        </div>
        <div class="timeline-body">
          <div class="mb-1">
            <i class="bi bi-person me-1"></i>
            <strong>${esc(h.actionByName_En)}</strong>
            ${h.actionByDesignation_En ? `<span class="text-muted ms-1">— ${esc(h.actionByDesignation_En)}</span>` : ''}
          </div>
          ${h.opinion_En ? `<div class="mt-2"><small class="text-muted">Opinion:</small><br>${esc(h.opinion_En)}</div>` : ''}
          ${h.remarks_En ? `<div class="mt-1"><small class="text-muted">Remarks:</small><br>${esc(h.remarks_En)}</div>` : ''}
          ${h.reason_En ? `<div class="mt-2 p-2 bg-danger bg-opacity-10 rounded"><small class="text-danger fw-bold">Reason for Push Back:</small><br>${esc(h.reason_En)}</div>` : ''}
          ${h.pushedBackToStage ? `<div class="mt-1"><small class="text-muted">Pushed back to:</small> <span class="badge bg-warning text-dark">${esc(STAGE_LABELS[h.pushedBackToStage] || h.pushedBackToStage)}</span></div>` : ''}
        </div>
      </div>
    `;
    timeline.appendChild(item);
  }

  container.appendChild(timeline);
}

// ── Approve Modal ──────────────────────────────────────────────────

function showApproveModal(proposal) {
  const bodyHtml = `
    <div class="mb-3">
      <div class="alert alert-info py-2 mb-3">
        <i class="bi bi-info-circle me-1"></i>
        Approving <strong>${esc(proposal.proposalNumber)}</strong> will move it from
        <strong>${esc(STAGE_LABELS[proposal.currentStage])}</strong> to the next stage.
      </div>
      <label for="approve-opinion" class="form-label">Opinion (optional)</label>
      <textarea class="form-control" id="approve-opinion" rows="3" maxlength="4000"
        placeholder="Enter your professional opinion..."></textarea>
    </div>
    <div class="mb-3">
      <label for="approve-remarks" class="form-label">Remarks (optional)</label>
      <textarea class="form-control" id="approve-remarks" rows="2" maxlength="4000"
        placeholder="Any additional remarks..."></textarea>
    </div>
  `;

  showModal({
    title: 'Approve Proposal',
    body: bodyHtml,
    size: 'lg',
    buttons: [
      { label: 'Cancel', className: 'btn btn-secondary', onClick: (close) => close() },
      {
        label: 'Approve', className: 'btn btn-success',
        onClick: async (close) => {
          const opinion = document.querySelector('#approve-opinion')?.value?.trim() || null;
          const remarks = document.querySelector('#approve-remarks')?.value?.trim() || null;
          try {
            await api.post(`/workflow/${proposal.id}/approve`, {
              opinion_En: opinion,
              remarks_En: remarks,
            });
            close();
            showToast('Proposal approved successfully', 'success');
            renderProposalDetail(document.getElementById('main-content'), proposal.id);
          } catch (err) {
            showToast(err.message || 'Approval failed', 'danger');
          }
        }
      }
    ]
  });
}

// ── Push Back Modal ────────────────────────────────────────────────

function showPushBackModal(proposal) {
  const targets = PUSHBACK_TARGETS[proposal.currentStage] || [];
  const targetOptions = targets.map(t =>
    `<option value="${t}">${esc(STAGE_LABELS[t] || t)}</option>`
  ).join('');

  const bodyHtml = `
    <div class="mb-3">
      <div class="alert alert-warning py-2 mb-3">
        <i class="bi bi-exclamation-triangle me-1"></i>
        Pushing back <strong>${esc(proposal.proposalNumber)}</strong> from
        <strong>${esc(STAGE_LABELS[proposal.currentStage])}</strong>.
      </div>
      <label for="pushback-target" class="form-label">Push Back To <span class="text-danger">*</span></label>
      <select class="form-select" id="pushback-target" required>
        <option value="">Select target stage...</option>
        ${targetOptions}
      </select>
    </div>
    <div class="mb-3">
      <label for="pushback-reason" class="form-label">Reason <span class="text-danger">*</span></label>
      <textarea class="form-control" id="pushback-reason" rows="3" maxlength="4000" required
        placeholder="Mandatory: explain why the proposal is being pushed back..."></textarea>
      <div class="form-text">A reason is required for all push-back actions.</div>
    </div>
    <div class="mb-3">
      <label for="pushback-remarks" class="form-label">Remarks (optional)</label>
      <textarea class="form-control" id="pushback-remarks" rows="2" maxlength="4000"
        placeholder="Additional remarks..."></textarea>
    </div>
  `;

  showModal({
    title: 'Push Back Proposal',
    body: bodyHtml,
    size: 'lg',
    buttons: [
      { label: 'Cancel', className: 'btn btn-secondary', onClick: (close) => close() },
      {
        label: 'Push Back', className: 'btn btn-danger',
        onClick: async (close) => {
          const target = document.querySelector('#pushback-target')?.value;
          const reason = document.querySelector('#pushback-reason')?.value?.trim();
          const remarks = document.querySelector('#pushback-remarks')?.value?.trim() || null;

          if (!target) { showToast('Select a target stage', 'warning'); return; }
          if (!reason) { showToast('Reason is required', 'warning'); return; }

          try {
            await api.post(`/workflow/${proposal.id}/pushback`, {
              targetStage: target,
              reason_En: reason,
              remarks_En: remarks,
            });
            close();
            showToast('Proposal pushed back', 'success');
            renderProposalDetail(document.getElementById('main-content'), proposal.id);
          } catch (err) {
            showToast(err.message || 'Push back failed', 'danger');
          }
        }
      }
    ]
  });
}

// ── Helpers ────────────────────────────────────────────────────────

function detailCard(title, rows) {
  return `
    <div class="col-md-6">
      <div class="card h-100">
        <div class="card-header"><h6 class="mb-0">${esc(title)}</h6></div>
        <div class="card-body p-0">
          <table class="table table-sm mb-0">
            <tbody>
              ${rows.map(r => r).join('')}
            </tbody>
          </table>
        </div>
      </div>
    </div>`;
}

function row(label, value, altValue = null, lang = null, extraClass = '') {
  const langAttr = lang ? ` lang="${lang}"` : '';
  const cls = extraClass ? ` class="${extraClass}"` : '';
  let html = `<tr><th class="text-muted fw-normal" style="width:40%">${esc(label)}</th><td${langAttr}${cls}>${esc(value || '')}`;
  if (altValue) html += `<br><small class="text-muted" lang="mr">${esc(altValue)}</small>`;
  html += `</td></tr>`;
  return html;
}

function bool(val) { return val ? '✅ Yes' : '❌ No'; }
function currency(val) { return `₹ ${Number(val || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}`; }
function formatDate(d) { return d ? new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }) : ''; }
function formatDateTime(d) { return d ? new Date(d).toLocaleString('en-IN') : ''; }
function esc(str) {
  const div = document.createElement('div');
  div.textContent = str || '';
  return div.innerHTML;
}
