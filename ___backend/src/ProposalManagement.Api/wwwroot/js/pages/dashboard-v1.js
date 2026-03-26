/** Dashboard v1 — role-aware dashboard with stats, recent proposals, stage breakdown */

import api from '../../api.js';
import { getCurrentUser } from '../../auth.js';
import { getInitials, formatDate, formatCurrency } from '../../utils.js';
import { translateDOM, t } from '../../i18n.js';

/**
 * @param {HTMLElement} container
 */
export async function renderDashboardV1(container) {
  const user = getCurrentUser();
  const role = user?.role || '';

  container.innerHTML = buildSkeleton(user, role);
  translateDOM(container);
  loadDashboardData(container, role);
}

function buildSkeleton(user, role) {
  const initials = getInitials(user?.fullName_En);
  return `
    <a href="#main-content" class="skip-link visually-hidden-focusable">Skip to content</a>

    <!-- Welcome Banner -->
    <div class="card border-0 bg-gradient text-white mb-4 shadow-sm" style="background:linear-gradient(135deg,var(--bs-primary),#1a3a5c)">
      <div class="card-body py-4 px-4">
        <div class="d-flex align-items-center gap-3 flex-wrap">
          <div class="user-avatar" style="width:56px;height:56px;font-size:1.25rem;background:rgba(255,255,255,0.2);border:2px solid rgba(255,255,255,0.4)">
            ${initials}
          </div>
          <div class="flex-grow-1">
            <h4 class="mb-1 fw-semibold" data-i18n="dashboard.welcome">Welcome, ${escapeHtml(user?.fullName_En || 'User')}</h4>
            <div class="d-flex align-items-center gap-2 opacity-75 flex-wrap">
              <span class="badge bg-white bg-opacity-25 text-white">${escapeHtml(role)}</span>
              <span class="d-none d-sm-inline">${escapeHtml(user?.mobileNumber || '')}</span>
            </div>
          </div>
          <div class="d-none d-md-block text-end">
            <div class="fs-5 fw-semibold" id="dash-date">${new Date().toLocaleDateString('en-IN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="row g-3 mb-4" id="stats-row">
      ${statCard('stat-total', 'bi-file-earmark-text', 'primary', 'dashboard.totalProposals', 'Total Proposals')}
      ${statCard('stat-draft', 'bi-pencil-square', 'secondary', 'dashboard.drafts', 'Drafts')}
      ${statCard('stat-pending', 'bi-hourglass-split', 'warning', 'dashboard.pendingApprovals', 'In Progress')}
      ${statCard('stat-approved', 'bi-check-circle', 'success', 'dashboard.approved', 'Approved')}
      ${statCard('stat-pushback', 'bi-arrow-return-left', 'danger', 'dashboard.pushedBack', 'Pushed Back')}
      ${statCard('stat-cancelled', 'bi-x-circle', 'dark', 'dashboard.cancelled', 'Cancelled')}
    </div>

    <!-- Two-column layout: Stage breakdown + Recent proposals -->
    <div class="row g-4">
      <!-- Stage Breakdown -->
      <div class="col-lg-5">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-header bg-transparent border-bottom d-flex align-items-center gap-2">
            <i class="bi bi-pie-chart text-primary"></i>
            <h6 class="mb-0 fw-semibold" data-i18n="dashboard.stageBreakdown">Stage Breakdown</h6>
          </div>
          <div class="card-body" id="stage-breakdown">
            <div class="text-center py-4"><div class="spinner-border spinner-border-sm text-muted"></div></div>
          </div>
        </div>
      </div>

      <!-- Recent Proposals -->
      <div class="col-lg-7">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-header bg-transparent border-bottom d-flex align-items-center justify-content-between">
            <div class="d-flex align-items-center gap-2">
              <i class="bi bi-clock-history text-primary"></i>
              <h6 class="mb-0 fw-semibold" data-i18n="dashboard.recentProposals">Recent Proposals</h6>
            </div>
            <a href="#/proposals" class="btn btn-sm btn-outline-primary" data-i18n="dashboard.viewAll">View All</a>
          </div>
          <div class="card-body p-0" id="recent-proposals">
            <div class="text-center py-4"><div class="spinner-border spinner-border-sm text-muted"></div></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="card border-0 shadow-sm mt-4">
      <div class="card-header bg-transparent border-bottom">
        <div class="d-flex align-items-center gap-2">
          <i class="bi bi-lightning-charge text-warning"></i>
          <h6 class="mb-0 fw-semibold" data-i18n="dashboard.quickActions">Quick Actions</h6>
        </div>
      </div>
      <div class="card-body">
        <div class="d-flex flex-wrap gap-2" id="quick-actions">
          ${buildQuickActions(role)}
        </div>
      </div>
    </div>
  `;
}

function statCard(id, icon, color, labelKey, labelDefault) {
  return `
    <div class="col-6 col-md-4 col-xl-2">
      <div class="card stat-card h-100 border-0 shadow-sm">
        <div class="stat-icon bg-${color} bg-opacity-10 text-${color}">
          <i class="bi ${icon}"></i>
        </div>
        <div>
          <div class="stat-value" id="${id}">
            <div class="spinner-border spinner-border-sm text-muted" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
          </div>
          <div class="stat-label" data-i18n="${labelKey}">${labelDefault}</div>
        </div>
      </div>
    </div>`;
}

async function loadDashboardData(container, role) {
  try {
    const stats = await api.get('/v1/dashboard/stats');

    animateStat('stat-total', stats.total);
    animateStat('stat-draft', stats.draft);
    animateStat('stat-pending', stats.pending);
    animateStat('stat-approved', stats.approved);
    animateStat('stat-pushback', stats.pushedBack);
    animateStat('stat-cancelled', stats.cancelled);

    renderStageBreakdown(container.querySelector('#stage-breakdown'), stats.byStage, stats.total);
    renderRecentProposals(container.querySelector('#recent-proposals'), stats.recentProposals);
  } catch (err) {
    console.error('Dashboard data load failed:', err);
    // Set zeroes if API fails
    ['stat-total', 'stat-draft', 'stat-pending', 'stat-approved', 'stat-pushback', 'stat-cancelled']
      .forEach(id => { const el = document.getElementById(id); if (el) el.textContent = '0'; });
  }
}

function renderStageBreakdown(container, byStage, total) {
  if (!container || !byStage?.length) {
    if (container) container.innerHTML = '<p class="text-muted text-center py-3">No proposals yet</p>';
    return;
  }

  const stageColors = {
    Draft: 'secondary',
    Submitted: 'info',
    AtCityEngineer: 'primary',
    AtADO: 'primary',
    AtChiefAccountant: 'warning',
    AtDeputyCommissioner: 'info',
    AtCommissioner: 'dark',
    PushedBack: 'danger',
    Approved: 'success',
    Cancelled: 'secondary'
  };

  const stageLabels = {
    Draft: 'Draft',
    Submitted: 'Submitted',
    AtCityEngineer: 'City Engineer',
    AtADO: 'ADO',
    AtChiefAccountant: 'Chief Accountant',
    AtDeputyCommissioner: 'Dy. Commissioner',
    AtCommissioner: 'Commissioner',
    PushedBack: 'Pushed Back',
    Approved: 'Approved',
    Cancelled: 'Cancelled'
  };

  let html = '<div class="d-flex flex-column gap-3">';
  for (const item of byStage) {
    const pct = total > 0 ? Math.round((item.count / total) * 100) : 0;
    const color = stageColors[item.stage] || 'primary';
    const label = stageLabels[item.stage] || item.stage;
    html += `
      <div>
        <div class="d-flex justify-content-between align-items-center mb-1">
          <span class="fw-medium small">${escapeHtml(label)}</span>
          <span class="badge bg-${color} bg-opacity-10 text-${color}">${item.count}</span>
        </div>
        <div class="progress" style="height:8px" role="progressbar" aria-valuenow="${pct}" aria-valuemin="0" aria-valuemax="100" aria-label="${label}: ${pct}%">
          <div class="progress-bar bg-${color}" style="width:${pct}%;transition:width 0.6s ease"></div>
        </div>
      </div>`;
  }
  html += '</div>';
  container.innerHTML = html;
}

function renderRecentProposals(container, proposals) {
  if (!container) return;
  if (!proposals?.length) {
    container.innerHTML = '<p class="text-muted text-center py-4">No proposals yet</p>';
    return;
  }

  const stageClasses = {
    Draft: 'bg-secondary',
    Submitted: 'bg-info',
    AtCityEngineer: 'bg-primary',
    AtADO: 'bg-primary',
    AtChiefAccountant: 'bg-warning text-dark',
    AtDeputyCommissioner: 'bg-info',
    AtCommissioner: 'bg-dark',
    PushedBack: 'bg-danger',
    Approved: 'bg-success',
    Cancelled: 'bg-secondary'
  };

  let html = '<div class="list-group list-group-flush">';
  for (const p of proposals) {
    const badgeClass = stageClasses[p.currentStage] || 'bg-secondary';
    html += `
      <a href="#/proposals/${p.id}" class="list-group-item list-group-item-action border-0 px-4 py-3">
        <div class="d-flex justify-content-between align-items-start">
          <div class="flex-grow-1 me-3">
            <div class="d-flex align-items-center gap-2 mb-1">
              <span class="fw-semibold text-primary small">${escapeHtml(p.proposalNumber)}</span>
              <span class="badge ${badgeClass} rounded-pill" style="font-size:0.7rem">${formatStage(p.currentStage)}</span>
            </div>
            <div class="text-truncate" style="max-width:400px">${escapeHtml(p.subject_En)}</div>
            <small class="text-muted">${escapeHtml(p.submittedByName_En)} &middot; ${formatDate(p.createdAt)}</small>
          </div>
          <div class="text-end">
            <span class="fw-semibold">${formatCurrency(p.estimatedCost)}</span>
          </div>
        </div>
      </a>`;
  }
  html += '</div>';
  container.innerHTML = html;
}

function buildQuickActions(role) {
  const actions = [];

  if (['Submitter', 'Lotus'].includes(role)) {
    actions.push({ href: '#/proposals/new/wizard', icon: 'bi-plus-circle', color: 'primary', label: 'New Proposal' });
  }
  if (['Submitter', 'CityEngineer', 'ADO', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner', 'Lotus'].includes(role)) {
    actions.push({ href: '#/proposals', icon: 'bi-list-ul', color: 'secondary', label: 'My Proposals' });
  }
  if (['CityEngineer', 'ADO', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner'].includes(role)) {
    actions.push({ href: '#/approvals', icon: 'bi-check2-square', color: 'success', label: 'Pending Approvals' });
  }
  if (['Commissioner', 'Auditor', 'Lotus'].includes(role)) {
    actions.push({ href: '#/proposals/all', icon: 'bi-collection', color: 'info', label: 'All Proposals' });
  }
  if (role === 'Lotus') {
    actions.push({ href: '#/lotus/users', icon: 'bi-people', color: 'warning', label: 'Manage Users' });
  }

  return actions.map(a =>
    `<a href="${a.href}" class="btn btn-outline-${a.color} d-inline-flex align-items-center gap-2">
      <i class="bi ${a.icon}"></i><span>${escapeHtml(a.label)}</span>
    </a>`
  ).join('');
}

function formatStage(stage) {
  const map = {
    Draft: 'Draft', Submitted: 'Submitted', AtCityEngineer: 'City Engineer', AtADO: 'ADO',
    AtChiefAccountant: 'Chief Accountant', AtDeputyCommissioner: 'Dy. Commissioner',
    AtCommissioner: 'Commissioner', PushedBack: 'Pushed Back', Approved: 'Approved', Cancelled: 'Cancelled'
  };
  return map[stage] || stage;
}

function animateStat(id, target) {
  const el = document.getElementById(id);
  if (!el) return;
  el.textContent = '0';
  if (target === 0) return;
  const duration = 600;
  const start = performance.now();
  function step(now) {
    const progress = Math.min((now - start) / duration, 1);
    el.textContent = Math.round(progress * target).toLocaleString();
    if (progress < 1) requestAnimationFrame(step);
  }
  requestAnimationFrame(step);
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str || '';
  return div.innerHTML;
}
