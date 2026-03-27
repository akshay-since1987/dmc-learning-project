// Audit Trail page — for Lotus, Commissioner, Auditor
import { api } from '../api.js';
import { getUser, hasRole } from '../auth.js';
import { toast } from '../toast.js';
import { escapeHtml, formatDate } from '../utils.js';
import { t, tBilingual, translatePage, onLangChange } from '../i18n.js';

let currentPage = 1;
const pageSize = 25;

export async function renderAuditTrailPage() {
    const content = document.getElementById('page-content');

    content.innerHTML = `
        <h4 class="mb-3"><i class="bi bi-journal-text me-2"></i><span data-i18n="audit.title">${t('audit.title')}</span></h4>
        <div class="card mb-3">
            <div class="card-body">
                <form id="audit-filters" class="row g-2 align-items-end">
                    <div class="col-md-2">
                        <label for="afModule" class="form-label small" data-i18n="audit.module">${t('audit.module')}</label>
                        <select class="form-select form-select-sm" id="afModule">
                            <option value="" data-i18n="audit.allModules">${t('audit.allModules')}</option>
                            <option value="Auth">Auth</option>
                            <option value="Proposal">Proposal</option>
                            <option value="Workflow">Workflow</option>
                            <option value="Document">Document</option>
                            <option value="Master">Master</option>
                            <option value="Lotus">Lotus</option>
                            <option value="System">System</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <label for="afAction" class="form-label small" data-i18n="audit.action">${t('audit.action')}</label>
                        <select class="form-select form-select-sm" id="afAction">
                            <option value="" data-i18n="audit.allActions">${t('audit.allActions')}</option>
                            <option value="Create">Create</option>
                            <option value="Update">Update</option>
                            <option value="Delete">Delete</option>
                            <option value="Login">Login</option>
                            <option value="Approve">Approve</option>
                            <option value="PushBack">PushBack</option>
                            <option value="Submit">Submit</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <label for="afFrom" class="form-label small" data-i18n="audit.from">${t('audit.from')}</label>
                        <input type="date" class="form-control form-control-sm" id="afFrom">
                    </div>
                    <div class="col-md-2">
                        <label for="afTo" class="form-label small" data-i18n="audit.to">${t('audit.to')}</label>
                        <input type="date" class="form-control form-control-sm" id="afTo">
                    </div>
                    <div class="col-md-2">
                        <label for="afSearch" class="form-label small" data-i18n="audit.search">${t('audit.search')}</label>
                        <input type="text" class="form-control form-control-sm" id="afSearch" data-i18n-placeholder="audit.search" placeholder="${t('audit.search')}">
                    </div>
                    <div class="col-md-2 d-flex gap-1">
                        <button type="submit" class="btn btn-primary btn-sm flex-grow-1"><i class="bi bi-search me-1"></i><span data-i18n="audit.filter">${t('audit.filter')}</span></button>
                        <button type="button" class="btn btn-outline-secondary btn-sm" id="btn-reset-filters" data-i18n="audit.reset">${t('audit.reset')}</button>
                    </div>
                </form>
            </div>
        </div>
        <div class="card">
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover table-sm mb-0">
                        <thead class="table-light">
                            <tr>
                                <th style="width:140px;" data-i18n="audit.timestamp">${t('audit.timestamp')}</th>
                                <th data-i18n="audit.user">${t('audit.user')}</th>
                                <th data-i18n="audit.action">${t('audit.action')}</th>
                                <th data-i18n="audit.module">${t('audit.module')}</th>
                                <th data-i18n="audit.entity">${t('audit.entity')}</th>
                                <th data-i18n="audit.description">${t('audit.description')}</th>
                                <th data-i18n="audit.details">${t('audit.details')}</th>
                            </tr>
                        </thead>
                        <tbody id="audit-tbody">
                            <tr><td colspan="7" class="text-center py-4"><div class="spinner-border spinner-border-sm text-primary"></div></td></tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="card-footer d-flex justify-content-between align-items-center">
                <small class="text-muted" id="audit-info" data-i18n="common.loading">${t('common.loading')}</small>
                <div class="btn-group btn-group-sm">
                    <button class="btn btn-outline-secondary" id="btn-prev" disabled><i class="bi bi-chevron-left"></i></button>
                    <button class="btn btn-outline-secondary" id="btn-next" disabled><i class="bi bi-chevron-right"></i></button>
                </div>
            </div>
        </div>

        <!-- Detail Modal -->
        <div class="modal fade" id="auditDetailModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header"><h5 class="modal-title" data-i18n="audit.detailTitle">${t('audit.detailTitle')}</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
                    <div class="modal-body" id="audit-detail-body"></div>
                </div>
            </div>
        </div>`;

    translatePage(content);
    onLangChange(() => translatePage(content));

    document.getElementById('audit-filters').addEventListener('submit', e => { e.preventDefault(); currentPage = 1; loadAudit(); });
    document.getElementById('btn-reset-filters').addEventListener('click', () => {
        document.getElementById('audit-filters').reset();
        currentPage = 1;
        loadAudit();
    });
    document.getElementById('btn-prev').addEventListener('click', () => { if (currentPage > 1) { currentPage--; loadAudit(); } });
    document.getElementById('btn-next').addEventListener('click', () => { currentPage++; loadAudit(); });

    await loadAudit();
}

async function loadAudit() {
    const params = new URLSearchParams();
    params.set('page', currentPage);
    params.set('pageSize', pageSize);
    const module = document.getElementById('afModule').value;
    const action = document.getElementById('afAction').value;
    const from = document.getElementById('afFrom').value;
    const to = document.getElementById('afTo').value;
    const search = document.getElementById('afSearch').value.trim();
    if (module) params.set('module', module);
    if (action) params.set('action', action);
    if (from) params.set('from', from);
    if (to) params.set('to', to);
    if (search) params.set('search', search);

    const res = await api.get(`/audit?${params.toString()}`);
    const tbody = document.getElementById('audit-tbody');
    const info = document.getElementById('audit-info');

    if (!res.success) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger">${escapeHtml(res.error || 'Failed')}</td></tr>`;
        info.textContent = 'Error loading';
        return;
    }

    const data = res.data;
    const items = data.items || data;
    const total = data.totalCount || items.length;

    if (items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-3">No audit records found</td></tr>';
        info.textContent = '0 records';
        document.getElementById('btn-prev').disabled = true;
        document.getElementById('btn-next').disabled = true;
        return;
    }

    const actionBg = a => {
        if (a === 'Create') return 'success';
        if (a === 'Update') return 'primary';
        if (a === 'Delete') return 'danger';
        if (a === 'Approve') return 'success';
        if (a === 'PushBack') return 'warning text-dark';
        if (a === 'Login' || a === 'Logout') return 'info';
        return 'secondary';
    };

    tbody.innerHTML = items.map(a => `
        <tr>
            <td><small>${formatDate(a.timestamp || a.createdAt)}</small></td>
            <td><small>${escapeHtml(a.userName || '—')}</small></td>
            <td><span class="badge bg-${actionBg(a.action)}">${a.action}</span></td>
            <td><small>${escapeHtml(a.module || '—')}</small></td>
            <td><small>${escapeHtml(a.entityType || '—')}${a.entityId ? ` #${escapeHtml(a.entityId.substring(0, 8))}` : ''}</small></td>
            <td><small>${escapeHtml((a.description || '').substring(0, 80))}${a.description && a.description.length > 80 ? '...' : ''}</small></td>
            <td>${a.oldValues || a.newValues ? `<button class="btn btn-outline-secondary btn-sm py-0 px-1 btn-audit-detail" data-old="${escapeHtml(a.oldValues || '')}" data-new="${escapeHtml(a.newValues || '')}"><i class="bi bi-eye"></i></button>` : ''}</td>
        </tr>`).join('');

    const start = (currentPage - 1) * pageSize + 1;
    const end = start + items.length - 1;
    info.textContent = `${start}–${end} of ${total}`;
    document.getElementById('btn-prev').disabled = currentPage <= 1;
    document.getElementById('btn-next').disabled = end >= total;

    // Wire detail buttons
    tbody.querySelectorAll('.btn-audit-detail').forEach(btn => {
        btn.addEventListener('click', () => {
            const oldV = btn.dataset.old;
            const newV = btn.dataset.new;
            const body = document.getElementById('audit-detail-body');
            let html = '<div class="row g-3">';
            if (oldV) html += `<div class="col-md-6"><h6>Old Values</h6><pre class="bg-light p-2 rounded small" style="max-height:300px;overflow:auto;">${escapeHtml(formatJson(oldV))}</pre></div>`;
            if (newV) html += `<div class="col-md-6"><h6>New Values</h6><pre class="bg-light p-2 rounded small" style="max-height:300px;overflow:auto;">${escapeHtml(formatJson(newV))}</pre></div>`;
            html += '</div>';
            body.innerHTML = html;
            new bootstrap.Modal(document.getElementById('auditDetailModal')).show();
        });
    });
}

function formatJson(str) {
    try { return JSON.stringify(JSON.parse(str), null, 2); }
    catch { return str; }
}
