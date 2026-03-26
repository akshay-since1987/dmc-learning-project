/** Audit Trail viewer — used by Lotus, Commissioner, and Auditor */
import api from '../../api.js';
import { getUserRole } from '../../auth.js';
import { debounce } from '../../utils.js';

const MODULE_OPTIONS = ['', 'Auth', 'Proposal', 'Workflow', 'Lotus', 'Master', 'Document', 'System'];
const ACTION_OPTIONS = ['', 'Create', 'Update', 'Delete', 'Login', 'Logout', 'Approve', 'PushBack', 'Submit', 'Upload', 'Download', 'Generate', 'FailedAuth'];

const MODULE_BADGES = {
  Auth: 'bg-info',
  Proposal: 'bg-primary',
  Workflow: 'bg-warning text-dark',
  Lotus: 'bg-dark',
  Master: 'bg-secondary',
  Document: 'bg-success',
  System: 'bg-danger',
};

const SEVERITY_BADGES = {
  Info: 'bg-info',
  Warning: 'bg-warning text-dark',
  Critical: 'bg-danger',
};

export function renderAuditTrail(container) {
  const role = getUserRole();
  const isAuditor = role === 'Auditor';

  // Auditor can only see proposal-related modules
  const modules = isAuditor
    ? ['', 'Proposal', 'Workflow', 'Document']
    : MODULE_OPTIONS;

  const state = { search: '', module: '', action: '', fromDate: '', toDate: '', pageIndex: 1, pageSize: 20, totalCount: 0, items: [] };

  container.innerHTML = `
    <div class="card mb-3">
      <div class="card-body">
        <div class="row g-2 align-items-end">
          <div class="col-md-3">
            <label class="form-label form-label-sm mb-1">Search</label>
            <div class="input-group input-group-sm">
              <span class="input-group-text"><i class="bi bi-search"></i></span>
              <input type="search" class="form-control" id="audit-search" placeholder="User, entity, description...">
            </div>
          </div>
          <div class="col-md-2">
            <label class="form-label form-label-sm mb-1">Module</label>
            <select class="form-select form-select-sm" id="audit-module">
              ${modules.map(m => `<option value="${m}">${m || 'All Modules'}</option>`).join('')}
            </select>
          </div>
          <div class="col-md-2">
            <label class="form-label form-label-sm mb-1">Action</label>
            <select class="form-select form-select-sm" id="audit-action">
              ${ACTION_OPTIONS.map(a => `<option value="${a}">${a || 'All Actions'}</option>`).join('')}
            </select>
          </div>
          <div class="col-md-2">
            <label class="form-label form-label-sm mb-1">From</label>
            <input type="date" class="form-control form-control-sm" id="audit-from">
          </div>
          <div class="col-md-2">
            <label class="form-label form-label-sm mb-1">To</label>
            <input type="date" class="form-control form-control-sm" id="audit-to">
          </div>
          <div class="col-md-1">
            <button class="btn btn-outline-secondary btn-sm w-100" id="audit-reset" title="Reset filters">
              <i class="bi bi-x-lg"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-hover table-sm align-middle mb-0">
            <thead class="table-light">
              <tr>
                <th scope="col" style="width:160px">Timestamp</th>
                <th scope="col">User</th>
                <th scope="col">Action</th>
                <th scope="col">Module</th>
                <th scope="col">Entity</th>
                <th scope="col">Description</th>
                <th scope="col" style="width:80px">Severity</th>
                <th scope="col" style="width:50px"></th>
              </tr>
            </thead>
            <tbody id="audit-tbody"></tbody>
          </table>
        </div>
      </div>
      <div class="card-footer d-flex align-items-center justify-content-between flex-wrap gap-2">
        <small class="text-muted" id="audit-info"></small>
        <nav aria-label="Audit pagination">
          <ul class="pagination pagination-sm mb-0" id="audit-pagination"></ul>
        </nav>
      </div>
    </div>

    <!-- Detail offcanvas -->
    <div class="offcanvas offcanvas-end" tabindex="-1" id="audit-detail-offcanvas" aria-labelledby="audit-detail-title" style="width:500px">
      <div class="offcanvas-header">
        <h6 class="offcanvas-title" id="audit-detail-title">Audit Detail</h6>
        <button type="button" class="btn-close" data-bs-dismiss="offcanvas" aria-label="Close"></button>
      </div>
      <div class="offcanvas-body" id="audit-detail-body"></div>
    </div>
  `;

  // Bind filters
  const searchInput = container.querySelector('#audit-search');
  const moduleSelect = container.querySelector('#audit-module');
  const actionSelect = container.querySelector('#audit-action');
  const fromInput = container.querySelector('#audit-from');
  const toInput = container.querySelector('#audit-to');
  const resetBtn = container.querySelector('#audit-reset');

  const reload = () => { state.pageIndex = 1; loadData(); };

  searchInput.addEventListener('input', debounce(() => { state.search = searchInput.value; reload(); }, 300));
  moduleSelect.addEventListener('change', () => { state.module = moduleSelect.value; reload(); });
  actionSelect.addEventListener('change', () => { state.action = actionSelect.value; reload(); });
  fromInput.addEventListener('change', () => { state.fromDate = fromInput.value; reload(); });
  toInput.addEventListener('change', () => { state.toDate = toInput.value; reload(); });
  resetBtn.addEventListener('click', () => {
    searchInput.value = ''; moduleSelect.value = ''; actionSelect.value = '';
    fromInput.value = ''; toInput.value = '';
    state.search = ''; state.module = ''; state.action = '';
    state.fromDate = ''; state.toDate = '';
    reload();
  });

  async function loadData() {
    const tbody = container.querySelector('#audit-tbody');
    tbody.innerHTML = `<tr><td colspan="8" class="text-center py-4">
      <div class="spinner-border spinner-border-sm text-primary me-2"></div>Loading...
    </td></tr>`;

    try {
      const params = new URLSearchParams({ pageIndex: state.pageIndex, pageSize: state.pageSize });
      if (state.search) params.set('search', state.search);
      if (state.module) params.set('module', state.module);
      if (state.action) params.set('action', state.action);
      if (state.fromDate) params.set('fromDate', state.fromDate);
      if (state.toDate) params.set('toDate', state.toDate);

      const result = await api.get(`/audit?${params}`);
      state.items = result.items || [];
      state.totalCount = result.totalCount || 0;
      renderRows();
      renderPagination();
      renderInfo();
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="8" class="text-center text-danger py-4">
        Failed to load audit data: ${escapeHtml(err.message || 'Unknown error')}
      </td></tr>`;
    }
  }

  function renderRows() {
    const tbody = container.querySelector('#audit-tbody');
    tbody.innerHTML = '';

    if (state.items.length === 0) {
      tbody.innerHTML = `<tr><td colspan="8" class="text-center py-4 text-muted">
        <i class="bi bi-journal-x d-block mb-2" style="font-size:2rem;opacity:0.3"></i>
        No audit records found
      </td></tr>`;
      return;
    }

    for (const row of state.items) {
      const tr = document.createElement('tr');
      tr.style.cursor = 'pointer';
      tr.addEventListener('click', () => showDetail(row));

      const ts = new Date(row.timestamp);
      const timeStr = ts.toLocaleString('en-IN', {
        day: '2-digit', month: 'short', year: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit'
      });

      tr.innerHTML = `
        <td><small>${timeStr}</small></td>
        <td>
          <div class="fw-medium">${escapeHtml(row.userName || '—')}</div>
          <small class="text-muted">${escapeHtml(row.userRole || '')}</small>
        </td>
        <td><span class="badge bg-outline-secondary border">${escapeHtml(row.action)}</span></td>
        <td><span class="badge ${MODULE_BADGES[row.module] || 'bg-secondary'}">${escapeHtml(row.module)}</span></td>
        <td>
          <div>${escapeHtml(row.entityType)}</div>
          <small class="text-muted font-monospace">${escapeHtml((row.entityId || '').substring(0, 8))}...</small>
        </td>
        <td><small>${escapeHtml(truncate(row.description, 60))}</small></td>
        <td><span class="badge ${SEVERITY_BADGES[row.severity] || 'bg-secondary'}">${escapeHtml(row.severity)}</span></td>
        <td><i class="bi bi-chevron-right text-muted"></i></td>
      `;
      tbody.appendChild(tr);
    }
  }

  function renderInfo() {
    const info = container.querySelector('#audit-info');
    const start = (state.pageIndex - 1) * state.pageSize + 1;
    const end = Math.min(state.pageIndex * state.pageSize, state.totalCount);
    info.textContent = state.totalCount > 0
      ? `Showing ${start}–${end} of ${state.totalCount} records`
      : 'No records';
  }

  function renderPagination() {
    const pag = container.querySelector('#audit-pagination');
    pag.innerHTML = '';
    const totalPages = Math.ceil(state.totalCount / state.pageSize);
    if (totalPages <= 1) return;

    const addPage = (label, page, disabled = false, active = false) => {
      const li = document.createElement('li');
      li.className = `page-item${disabled ? ' disabled' : ''}${active ? ' active' : ''}`;
      const a = document.createElement('a');
      a.className = 'page-link';
      a.href = '#';
      a.textContent = label;
      if (!disabled && !active) {
        a.addEventListener('click', (e) => { e.preventDefault(); state.pageIndex = page; loadData(); });
      }
      li.appendChild(a);
      pag.appendChild(li);
    };

    addPage('«', state.pageIndex - 1, state.pageIndex === 1);
    const startPage = Math.max(1, state.pageIndex - 2);
    const endPage = Math.min(totalPages, startPage + 4);
    for (let i = startPage; i <= endPage; i++) {
      addPage(String(i), i, false, i === state.pageIndex);
    }
    addPage('»', state.pageIndex + 1, state.pageIndex === totalPages);
  }

  function showDetail(row) {
    const body = container.querySelector('#audit-detail-body');
    const ts = new Date(row.timestamp);

    body.innerHTML = `
      <div class="mb-3">
        <span class="badge ${MODULE_BADGES[row.module] || 'bg-secondary'} me-1">${escapeHtml(row.module)}</span>
        <span class="badge bg-outline-secondary border me-1">${escapeHtml(row.action)}</span>
        <span class="badge ${SEVERITY_BADGES[row.severity] || 'bg-secondary'}">${escapeHtml(row.severity)}</span>
      </div>

      <dl class="row mb-0">
        <dt class="col-4">Timestamp</dt>
        <dd class="col-8">${ts.toLocaleString('en-IN')}</dd>
        <dt class="col-4">User</dt>
        <dd class="col-8">${escapeHtml(row.userName || '—')} <small class="text-muted">(${escapeHtml(row.userRole)})</small></dd>
        <dt class="col-4">IP Address</dt>
        <dd class="col-8"><code>${escapeHtml(row.ipAddress)}</code></dd>
        <dt class="col-4">Entity Type</dt>
        <dd class="col-8">${escapeHtml(row.entityType)}</dd>
        <dt class="col-4">Entity ID</dt>
        <dd class="col-8"><code class="small">${escapeHtml(row.entityId)}</code></dd>
        <dt class="col-4">Description</dt>
        <dd class="col-8">${escapeHtml(row.description)}</dd>
      </dl>

      ${row.oldValues ? `
        <div class="mt-3">
          <h6 class="text-danger"><i class="bi bi-dash-circle me-1"></i>Old Values</h6>
          <pre class="bg-light p-2 rounded small" style="max-height:200px;overflow:auto">${escapeHtml(formatJson(row.oldValues))}</pre>
        </div>` : ''}

      ${row.newValues ? `
        <div class="mt-3">
          <h6 class="text-success"><i class="bi bi-plus-circle me-1"></i>New Values</h6>
          <pre class="bg-light p-2 rounded small" style="max-height:200px;overflow:auto">${escapeHtml(formatJson(row.newValues))}</pre>
        </div>` : ''}
    `;

    // Open the offcanvas
    const offcanvasEl = container.querySelector('#audit-detail-offcanvas');
    const bsOffcanvas = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
    bsOffcanvas.show();
  }

  // Initial load
  loadData();
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str || '';
  return div.innerHTML;
}

function truncate(str, len) {
  if (!str) return '';
  return str.length > len ? str.substring(0, len) + '...' : str;
}

function formatJson(jsonStr) {
  try {
    return JSON.stringify(JSON.parse(jsonStr), null, 2);
  } catch {
    return jsonStr || '';
  }
}
