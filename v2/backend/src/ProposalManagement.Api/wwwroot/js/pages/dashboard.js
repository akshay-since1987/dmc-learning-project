// Dashboard page
import { api } from '../api.js';
import { getUser } from '../auth.js';
import { showLoading, hideLoading, stageBadge, formatDate } from '../utils.js';
import { t, tBilingual, translatePage, onLangChange } from '../i18n.js';

export async function renderDashboardPage() {
    const user = getUser();
    const content = document.getElementById('page-content');

    content.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
                <h4 class="mb-1" data-i18n="dashboard.title">${t('dashboard.title')}</h4>
                <p class="text-muted mb-0" style="font-size:0.85rem;"><span data-i18n="dashboard.welcome">${t('dashboard.welcome')}</span>, ${user.fullName} (${user.role})</p>
            </div>
            ${user.role === 'JE' || user.role === 'Lotus' ? `<a href="#/proposals/new" class="btn btn-primary"><i class="bi bi-plus-circle me-1"></i><span data-i18n="dashboard.newProposal">${t('dashboard.newProposal')}</span></a>` : ''}
        </div>

        <div id="stats-row" class="row g-3 mb-4">
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-secondary" id="stat-draft">—</div><div class="stat-label" data-i18n="dashboard.draft">${t('dashboard.draft')}</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-primary" id="stat-progress">—</div><div class="stat-label" data-i18n="dashboard.inProgress">${t('dashboard.inProgress')}</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-warning" id="stat-pushed">—</div><div class="stat-label" data-i18n="dashboard.pushedBack">${t('dashboard.pushedBack')}</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-orange" id="stat-parked" style="color:#fd7e14;">—</div><div class="stat-label" data-i18n="dashboard.parked">${t('dashboard.parked')}</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-success" id="stat-approved">—</div><div class="stat-label" data-i18n="dashboard.approved">${t('dashboard.approved')}</div></div></div>
            <div class="col-6 col-md-4 col-lg-2"><div class="card stat-card p-3 text-center"><div class="stat-value text-dark" id="stat-total">—</div><div class="stat-label" data-i18n="dashboard.total">${t('dashboard.total')}</div></div></div>
        </div>

        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h6 class="mb-0" id="table-title" data-i18n="dashboard.recent">${t('dashboard.recent')}</h6>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table data-table mb-0" aria-label="Recent proposals">
                        <thead>
                            <tr>
                                <th scope="col">#</th>
                                <th scope="col" data-i18n="dashboard.proposalNo">${t('dashboard.proposalNo')}</th>
                                <th scope="col" data-i18n="dashboard.workTitle">${t('dashboard.workTitle')}</th>
                                <th scope="col" data-i18n="dashboard.department">${t('dashboard.department')}</th>
                                <th scope="col" data-i18n="dashboard.stage">${t('dashboard.stage')}</th>
                                <th scope="col" data-i18n="dashboard.date">${t('dashboard.date')}</th>
                                <th scope="col" data-i18n="dashboard.action">${t('dashboard.action')}</th>
                            </tr>
                        </thead>
                        <tbody id="proposals-tbody">
                            <tr><td colspan="7" class="text-center py-4 text-muted" data-i18n="common.loading">${t('common.loading')}</td></tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>`;

    translatePage(content);
    onLangChange(() => translatePage(content));

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
    let titleKey = 'dashboard.myRecent';

    const role = user.role;
    if (['Commissioner', 'Auditor', 'Lotus'].includes(role)) {
        endpoint = '/proposals/all?page=1&pageSize=10';
        titleKey = 'dashboard.allRecent';
    } else if (['CityEngineer', 'AccountOfficer', 'DyCommissioner', 'StandingCommittee', 'Collector'].includes(role)) {
        endpoint = '/proposals/pending?page=1&pageSize=10';
        titleKey = 'dashboard.pendingApproval';
    }

    document.getElementById('table-title').textContent = t(titleKey);

    const res = await api.get(endpoint);
    const tbody = document.getElementById('proposals-tbody');

    if (res.success && res.data && res.data.items.length > 0) {
        tbody.innerHTML = res.data.items.map((p, i) => `
            <tr>
                <td>${i + 1}</td>
                <td><a href="#/proposals/${p.id}" class="text-decoration-none fw-medium">${p.proposalNumber}</a></td>
                <td>${p.workTitle_En || '—'}</td>
                <td>${p.departmentName || '—'}${p.departmentName_Mr ? ` <span class="text-muted small" lang="mr">(${p.departmentName_Mr})</span>` : ''}</td>
                <td>${stageBadge(p.currentStage)}</td>
                <td>${formatDate(p.proposalDate)}</td>
                <td><a href="#/proposals/${p.id}" class="btn btn-sm btn-outline-primary"><i class="bi bi-eye"></i></a></td>
            </tr>`).join('');
    } else {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">${t('dashboard.noProposals')}</td></tr>`;
    }
}
