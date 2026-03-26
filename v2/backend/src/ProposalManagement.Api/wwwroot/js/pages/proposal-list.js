// Proposal List page — My Proposals / All Proposals / Pending
import { api } from '../api.js';
import { getUser } from '../auth.js';
import { stageBadge, formatDate, debounce, escapeHtml } from '../utils.js';

export async function renderProposalListPage(params = {}) {
    const user = getUser();
    const mode = params.mode || 'my'; // my | all | pending
    const content = document.getElementById('page-content');

    const titles = { my: 'My Proposals', all: 'All Proposals', pending: 'Pending Approvals' };
    const title = titles[mode] || 'Proposals';
    const canCreate = (user.role === 'JE' || user.role === 'Lotus') && mode === 'my';

    content.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h4 class="mb-0">${title}</h4>
            ${canCreate ? '<a href="#/proposals/new" class="btn btn-primary btn-sm"><i class="bi bi-plus-circle me-1"></i>New Proposal</a>' : ''}
        </div>

        <div class="card mb-3">
            <div class="card-body py-2">
                <div class="row g-2 align-items-end">
                    <div class="col-md-4">
                        <label for="search-input" class="form-label visually-hidden">Search</label>
                        <input type="text" class="form-control form-control-sm" id="search-input"
                            placeholder="Search by title or number...">
                    </div>
                    <div class="col-md-3">
                        <label for="stage-filter" class="form-label visually-hidden">Stage</label>
                        <select class="form-select form-select-sm" id="stage-filter" aria-label="Filter by stage">
                            <option value="">All Stages</option>
                            <option value="Draft">Draft</option>
                            <option value="AtCityEngineer">At City Engineer</option>
                            <option value="AtAccountOfficer">At Account Officer</option>
                            <option value="AtDyCommissioner">At Dy Commissioner</option>
                            <option value="AtCommissioner">At Commissioner</option>
                            <option value="AtStandingCommittee">At Standing Committee</option>
                            <option value="AtCollector">At Collector</option>
                            <option value="PushedBack">Pushed Back</option>
                            <option value="Parked">Parked</option>
                            <option value="Approved">Approved</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <button class="btn btn-sm btn-outline-secondary w-100" id="btn-reset-filters">
                            <i class="bi bi-arrow-counterclockwise me-1"></i>Reset
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <div class="card">
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table data-table mb-0" aria-label="${title}">
                        <thead>
                            <tr>
                                <th scope="col">#</th>
                                <th scope="col">Proposal No.</th>
                                <th scope="col">Work Title</th>
                                <th scope="col">Department</th>
                                <th scope="col">Category</th>
                                <th scope="col">Stage</th>
                                <th scope="col">Priority</th>
                                <th scope="col">Created</th>
                                <th scope="col">Action</th>
                            </tr>
                        </thead>
                        <tbody id="list-tbody">
                            <tr><td colspan="9" class="text-center py-4 text-muted">Loading...</td></tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="card-footer d-flex justify-content-between align-items-center">
                <small class="text-muted" id="page-info">—</small>
                <nav aria-label="Pagination">
                    <ul class="pagination pagination-sm mb-0" id="pagination"></ul>
                </nav>
            </div>
        </div>`;

    let currentPage = 1;
    const pageSize = 20;

    async function loadData() {
        const search = document.getElementById('search-input').value.trim();
        const stage = document.getElementById('stage-filter').value;

        const endpoints = {
            my: '/proposals/my',
            all: '/proposals/all',
            pending: '/proposals/pending'
        };

        let url = `${endpoints[mode]}?page=${currentPage}&pageSize=${pageSize}`;
        if (search) url += `&search=${encodeURIComponent(search)}`;
        if (stage) url += `&stage=${encodeURIComponent(stage)}`;

        const res = await api.get(url);
        const tbody = document.getElementById('list-tbody');

        if (res.success && res.data && res.data.items.length > 0) {
            const start = (currentPage - 1) * pageSize;
            tbody.innerHTML = res.data.items.map((p, i) => `
                <tr>
                    <td>${start + i + 1}</td>
                    <td><a href="#/proposals/${p.id}" class="text-decoration-none fw-medium">${escapeHtml(p.proposalNumber)}</a></td>
                    <td>${escapeHtml(p.workTitle_En)}</td>
                    <td>${escapeHtml(p.departmentName || '—')}</td>
                    <td>${escapeHtml(p.workCategoryName || '—')}</td>
                    <td>${stageBadge(p.currentStage)}</td>
                    <td><span class="badge bg-${p.priority === 'High' ? 'danger' : p.priority === 'Low' ? 'secondary' : 'warning text-dark'}">${p.priority}</span></td>
                    <td>${formatDate(p.createdAt)}</td>
                    <td><a href="#/proposals/${p.id}" class="btn btn-sm btn-outline-primary" aria-label="View proposal"><i class="bi bi-eye"></i></a></td>
                </tr>`).join('');

            // Pagination info
            const total = res.data.totalCount;
            const totalPages = Math.ceil(total / pageSize);
            document.getElementById('page-info').textContent = `Showing ${start + 1}–${Math.min(start + pageSize, total)} of ${total}`;

            renderPagination(totalPages);
        } else {
            tbody.innerHTML = '<tr><td colspan="9" class="text-center py-4 text-muted">No proposals found</td></tr>';
            document.getElementById('page-info').textContent = '0 results';
            document.getElementById('pagination').innerHTML = '';
        }
    }

    function renderPagination(totalPages) {
        const pagination = document.getElementById('pagination');
        if (totalPages <= 1) { pagination.innerHTML = ''; return; }

        let html = `<li class="page-item ${currentPage <= 1 ? 'disabled' : ''}">
            <button class="page-link" data-page="${currentPage - 1}" aria-label="Previous">&laquo;</button>
        </li>`;

        for (let i = 1; i <= Math.min(totalPages, 7); i++) {
            html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <button class="page-link" data-page="${i}">${i}</button>
            </li>`;
        }

        html += `<li class="page-item ${currentPage >= totalPages ? 'disabled' : ''}">
            <button class="page-link" data-page="${currentPage + 1}" aria-label="Next">&raquo;</button>
        </li>`;

        pagination.innerHTML = html;
        pagination.querySelectorAll('.page-link').forEach(btn => {
            btn.addEventListener('click', () => {
                const p = parseInt(btn.dataset.page);
                if (p >= 1 && p <= totalPages) {
                    currentPage = p;
                    loadData();
                }
            });
        });
    }

    // Event listeners
    document.getElementById('search-input').addEventListener('input', debounce(() => {
        currentPage = 1;
        loadData();
    }, 400));

    document.getElementById('stage-filter').addEventListener('change', () => {
        currentPage = 1;
        loadData();
    });

    document.getElementById('btn-reset-filters').addEventListener('click', () => {
        document.getElementById('search-input').value = '';
        document.getElementById('stage-filter').value = '';
        currentPage = 1;
        loadData();
    });

    loadData();
}
