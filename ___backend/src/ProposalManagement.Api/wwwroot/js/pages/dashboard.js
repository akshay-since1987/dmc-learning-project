/** Dashboard page — role-specific overview with stats */

import api from '../api.js';
import { getCurrentUser, hasRole } from '../auth.js';
import { getInitials } from '../utils.js';
import { translateDOM } from '../i18n.js';

export function renderDashboard(container) {
  const user = getCurrentUser();
  const role = user?.role || '';

  container.innerHTML = `
    <!-- Welcome banner -->
    <div class="card border-0 bg-primary bg-gradient text-white mb-4 shadow-sm">
      <div class="card-body py-4 px-4">
        <div class="d-flex align-items-center gap-3">
          <div class="user-avatar" style="width:56px;height:56px;font-size:1.25rem;background:rgba(255,255,255,0.2);border:2px solid rgba(255,255,255,0.4)">
            ${getInitials(user?.fullName_En)}
          </div>
          <div>
            <h4 class="mb-1 fw-semibold">Welcome, ${user?.fullName_En || 'User'}</h4>
            <div class="d-flex align-items-center gap-2 opacity-75">
              <span class="badge bg-white bg-opacity-25 text-white">${role}</span>
              <span>${user?.mobileNumber || ''}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Stats row -->
    <div class="row g-3 mb-4" id="stats-row">
      <div class="col-sm-6 col-xl-3">
        <div class="card stat-card h-100 border-0 shadow-sm">
          <div class="stat-icon bg-primary bg-opacity-10 text-primary">
            <i class="bi bi-file-earmark-text"></i>
          </div>
          <div>
            <div class="stat-value" id="stat-total">
              <div class="spinner-border spinner-border-sm text-muted" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
            <div class="stat-label" data-i18n="dashboard.totalProposals">Total Proposals</div>
          </div>
        </div>
      </div>
      <div class="col-sm-6 col-xl-3">
        <div class="card stat-card h-100 border-0 shadow-sm">
          <div class="stat-icon bg-warning bg-opacity-10 text-warning">
            <i class="bi bi-hourglass-split"></i>
          </div>
          <div>
            <div class="stat-value" id="stat-pending">
              <div class="spinner-border spinner-border-sm text-muted" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
            <div class="stat-label" data-i18n="dashboard.pendingApprovals">Pending Approvals</div>
          </div>
        </div>
      </div>
      <div class="col-sm-6 col-xl-3">
        <div class="card stat-card h-100 border-0 shadow-sm">
          <div class="stat-icon bg-success bg-opacity-10 text-success">
            <i class="bi bi-check-circle"></i>
          </div>
          <div>
            <div class="stat-value" id="stat-approved">
              <div class="spinner-border spinner-border-sm text-muted" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
            <div class="stat-label" data-i18n="dashboard.approved">Approved</div>
          </div>
        </div>
      </div>
      <div class="col-sm-6 col-xl-3">
        <div class="card stat-card h-100 border-0 shadow-sm">
          <div class="stat-icon bg-danger bg-opacity-10 text-danger">
            <i class="bi bi-arrow-return-left"></i>
          </div>
          <div>
            <div class="stat-value" id="stat-pushback">
              <div class="spinner-border spinner-border-sm text-muted" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
            <div class="stat-label" data-i18n="dashboard.pushedBack">Pushed Back</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick actions -->
    <div class="card border-0 shadow-sm">
      <div class="card-header bg-transparent border-bottom">
        <h6 class="mb-0 fw-semibold"><i class="bi bi-lightning-charge me-2 text-warning"></i>Quick Actions</h6>
      </div>
      <div class="card-body">
        <div class="d-flex flex-wrap gap-2" id="quick-actions"></div>
      </div>
    </div>
  `;

  // Render quick actions based on role
  const actionsEl = container.querySelector('#quick-actions');
  const actions = getQuickActions(role);
  for (const action of actions) {
    const btn = document.createElement('a');
    btn.href = action.href;
    btn.className = `btn btn-outline-${action.color} d-inline-flex align-items-center gap-2`;
    btn.innerHTML = `<i class="bi ${action.icon}"></i><span data-i18n="${action.labelKey}">${action.label}</span>`;
    actionsEl.appendChild(btn);
  }

  translateDOM(container);

  // Fetch stats from API
  loadStats();
}

async function loadStats() {
  try {
    const stats = await api.get('/proposals/stats');
    animateStat('stat-total', stats.total);
    animateStat('stat-pending', stats.pending);
    animateStat('stat-approved', stats.approved);
    animateStat('stat-pushback', stats.pushedBack);
  } catch {
    document.getElementById('stat-total')?.replaceChildren(document.createTextNode('0'));
    document.getElementById('stat-pending')?.replaceChildren(document.createTextNode('0'));
    document.getElementById('stat-approved')?.replaceChildren(document.createTextNode('0'));
    document.getElementById('stat-pushback')?.replaceChildren(document.createTextNode('0'));
  }
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
    el.textContent = Math.round(progress * target);
    if (progress < 1) requestAnimationFrame(step);
  }
  requestAnimationFrame(step);
}

function getQuickActions(role) {
  const actions = [];

  if (role === 'Submitter' || role === 'Lotus') {
    actions.push({
      href: '#/proposals/new',
      icon: 'bi-plus-circle',
      color: 'primary',
      labelKey: 'nav.newProposal',
      label: 'New Proposal'
    });
  }

  if (['Submitter', 'CityEngineer', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner'].includes(role)) {
    actions.push({
      href: '#/proposals',
      icon: 'bi-list-ul',
      color: 'secondary',
      labelKey: 'nav.myProposals',
      label: 'My Proposals'
    });
  }

  if (['CityEngineer', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner'].includes(role)) {
    actions.push({
      href: '#/approvals',
      icon: 'bi-check2-square',
      color: 'success',
      labelKey: 'nav.pendingApprovals',
      label: 'Pending Approvals'
    });
  }

  if (['Commissioner', 'Auditor', 'Lotus'].includes(role)) {
    actions.push({
      href: '#/proposals/all',
      icon: 'bi-collection',
      color: 'info',
      labelKey: 'nav.allProposals',
      label: 'All Proposals'
    });
  }

  if (role === 'Lotus') {
    actions.push({
      href: '#/lotus/users',
      icon: 'bi-people',
      color: 'dark',
      labelKey: 'nav.users',
      label: 'Manage Users'
    });
    actions.push({
      href: '#/lotus/settings',
      icon: 'bi-sliders',
      color: 'secondary',
      labelKey: 'nav.settings',
      label: 'Settings'
    });
  }

  return actions;
}
