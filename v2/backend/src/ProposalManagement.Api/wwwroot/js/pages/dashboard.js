// Dashboard page
import { api } from '../api.js';
import { getUser } from '../auth.js';
import { showLoading, hideLoading, stageBadge, formatDate } from '../utils.js';

export async function renderDashboardPage() {
    const user = getUser();
    const content = document.getElementById('page-content');

    content.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
                <h4 class="mb-1">Dashboard</h4>
                <p class="text-muted mb-0" style="font-size:0.85rem;">Welcome, ${user.fullName} (${user.role})</p>
            </div>
            ${user.role === 'JE' || user.role === 'Lotus' ? '<a href="#/proposals/new" class="btn btn-primary"><i class="bi bi-plus-circle me-1"></i>New Proposal</a>' : ''}
        </div>

        <div id="stats-row" class="row g-3 mb-4">
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-secondary" id="stat-draft">—</div><div class="stat-label">Draft</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-primary" id="stat-progress">—</div><div class="stat-label">In Progress</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-warning" id="stat-pushed">—</div><div class="stat-label">Pushed Back</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-orange" id="stat-parked" style="color:#fd7e14;">—</div><div class="stat-label">Parked</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-success" id="stat-approved">—</div><div class="stat-label">Approved</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-dark" id="stat-total">—</div><div class="stat-label">Total</div></div></div>
        </div>

        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h6 class="mb-0" id="table-title">Recent Proposals</h6>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table data-table mb-0" aria-label="Recent proposals">
                        <thead>
                            <tr>
                                <th scope="col">#</th>
                                <th scope="col">Proposal No.</th>
                                <th scope="col">Work Title</th>
                                <th scope="col">Department</th>
                                <th scope="col">Stage</th>
                                <th scope="col">Date</th>
                                <th scope="col">Action</th>
                            </tr>
                        </thead>
                        <tbody id="proposals-tbody">
                            <tr><td colspan="7" class="text-center py-4 text-muted">Loading...</td></tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>`;

    // Load stats
    const statsRes = await api.get('/proposals/stats');
    if (statsRes.success && statsRes.data) {
        const s = statsRes.data;
        document.getElementById('stat-draft').textContent = s.draft;
        document.getElementById('stat-progress').textContent = s.inProgress;
        document.getElementById('stat-pushed').textContent = s.pushedBack;
        document.getElementById('stat-parked').textContent = s.parked;
        document.getElementById('stat-approved').textContent = s.approved;
        document.getElementById('stat-total').textContent = s.total;
    }

    // Load recent proposals
    let endpoint = '/proposals/my?page=1&pageSize=10';
    let title = 'My Recent Proposals';

    const role = user.role;
    if (['Commissioner', 'Auditor', 'Lotus'].includes(role)) {
        endpoint = '/proposals/all?page=1&pageSize=10';
        title = 'All Recent Proposals';
    } else if (['CityEngineer', 'AccountOfficer', 'DyCommissioner', 'StandingCommittee', 'Collector'].includes(role)) {
        endpoint = '/proposals/pending?page=1&pageSize=10';
        title = 'Pending My Approval';
    }

    document.getElementById('table-title').textContent = title;

    const res = await api.get(endpoint);
    const tbody = document.getElementById('proposals-tbody');

    if (res.success && res.data && res.data.items.length > 0) {
        tbody.innerHTML = res.data.items.map((p, i) => `
            <tr>
                <td>${i + 1}</td>
                <td><a href="#/proposals/${p.id}" class="text-decoration-none fw-medium">${p.proposalNumber}</a></td>
                <td>${p.workTitle_En || '—'}</td>
                <td>${p.departmentName || '—'}</td>
                <td>${stageBadge(p.currentStage)}</td>
                <td>${formatDate(p.proposalDate)}</td>
                <td><a href="#/proposals/${p.id}" class="btn btn-sm btn-outline-primary"><i class="bi bi-eye"></i></a></td>
            </tr>`).join('');
    } else {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center py-4 text-muted">No proposals found</td></tr>';
    }
}
