/** Lotus Dashboard — overview of system entities */
import api from '../../api.js';

export function renderLotusDashboard(container) {
  container.innerHTML = `
    <div class="row g-4" id="lotus-stats">
      <div class="col-12">
        <div class="d-flex align-items-center gap-2 mb-3">
          <div class="spinner-border spinner-border-sm text-primary"></div>
          <span>Loading statistics...</span>
        </div>
      </div>
    </div>
  `;

  loadStats(container);
}

async function loadStats(container) {
  const statsEl = container.querySelector('#lotus-stats');

  // Load counts from each entity
  const entities = [
    { api: '/lotus/users?pageSize=1', label: 'Users', icon: 'bi-people-fill', color: 'primary', href: '#/lotus/users' },
    { api: '/lotus/departments?pageSize=1', label: 'Departments', icon: 'bi-building', color: 'success', href: '#/lotus/departments' },
    { api: '/lotus/designations?pageSize=1', label: 'Designations', icon: 'bi-award-fill', color: 'info', href: '#/lotus/designations' },
    { api: '/lotus/fund-types?pageSize=1', label: 'Fund Types', icon: 'bi-wallet2', color: 'warning', href: '#/lotus/fund-types' },
    { api: '/lotus/account-heads?pageSize=1', label: 'Account Heads', icon: 'bi-calculator-fill', color: 'danger', href: '#/lotus/account-heads' },
    { api: '/lotus/wards?pageSize=1', label: 'Wards', icon: 'bi-geo-alt-fill', color: 'secondary', href: '#/lotus/wards' },
  ];

  try {
    const results = await Promise.allSettled(entities.map(e => api.get(e.api)));

    let html = '';
    results.forEach((result, i) => {
      const entity = entities[i];
      const count = result.status === 'fulfilled' ? (result.value.totalCount ?? 0) : '–';
      html += `
        <div class="col-sm-6 col-lg-4 col-xl-3">
          <a href="${entity.href}" class="text-decoration-none">
            <div class="card h-100 border-0 shadow-sm lotus-stat-card" role="article" aria-label="${entity.label}: ${count}">
              <div class="card-body d-flex align-items-center gap-3">
                <div class="rounded-3 d-flex align-items-center justify-content-center bg-${entity.color} bg-opacity-10"
                     style="width:52px;height:52px;flex-shrink:0">
                  <i class="bi ${entity.icon} text-${entity.color}" style="font-size:1.5rem"></i>
                </div>
                <div>
                  <div class="text-muted small">${entity.label}</div>
                  <div class="fw-bold fs-4 text-${entity.color}">${count}</div>
                </div>
              </div>
            </div>
          </a>
        </div>`;
    });

    html += `
      <div class="col-12 mt-2">
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <h5 class="card-title mb-3"><i class="bi bi-lightning-charge me-2"></i>Quick Actions</h5>
            <div class="d-flex flex-wrap gap-2">
              <a href="#/lotus/users" class="btn btn-outline-primary btn-sm"><i class="bi bi-person-plus me-1"></i>Add User</a>
              <a href="#/lotus/departments" class="btn btn-outline-success btn-sm"><i class="bi bi-plus-circle me-1"></i>Add Department</a>
              <a href="#/lotus/wards" class="btn btn-outline-secondary btn-sm"><i class="bi bi-plus-circle me-1"></i>Add Ward</a>
              <a href="#/lotus/audit" class="btn btn-outline-dark btn-sm"><i class="bi bi-journal-text me-1"></i>Audit Log</a>
              <a href="#/lotus/settings" class="btn btn-outline-info btn-sm"><i class="bi bi-sliders me-1"></i>Settings</a>
            </div>
          </div>
        </div>
      </div>`;

    statsEl.innerHTML = html;
  } catch (err) {
    statsEl.innerHTML = `<div class="col-12"><div class="alert alert-danger">Failed to load statistics: ${err.message || 'Unknown error'}</div></div>`;
  }
}
