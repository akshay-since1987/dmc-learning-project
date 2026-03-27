// Admin Users page — Lotus-only user management
import { api } from '../api.js';
import { getUser } from '../auth.js';
import { toast } from '../toast.js';
import { escapeHtml, formatDate } from '../utils.js';
import { t, tBilingual, translatePage, onLangChange } from '../i18n.js';

const ROLES = ['JE','TS','AE','SE','CityEngineer','AccountOfficer','DyCommissioner','Commissioner','StandingCommittee','Collector','Auditor','Lotus'];

export async function renderAdminUsersPage() {
    const content = document.getElementById('page-content');
    const user = getUser();

    content.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h4 class="mb-0"><i class="bi bi-people me-2"></i><span data-i18n="admin.users.title">${t('admin.users.title')}</span></h4>
            <button class="btn btn-primary btn-sm" id="btn-add-user"><i class="bi bi-plus me-1"></i><span data-i18n="admin.users.addUser">${t('admin.users.addUser')}</span></button>
        </div>
        <div class="card">
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover mb-0">
                        <thead class="table-light">
                            <tr>
                                <th data-i18n="admin.users.name">${t('admin.users.name')}</th>
                                <th data-i18n="admin.users.mobile">${t('admin.users.mobile')}</th>
                                <th data-i18n="admin.users.role">${t('admin.users.role')}</th>
                                <th data-i18n="admin.users.department">${t('admin.users.department')}</th>
                                <th data-i18n="admin.users.active">${t('admin.users.active')}</th>
                                <th data-i18n="admin.users.created">${t('admin.users.created')}</th>
                                <th style="width:120px;" data-i18n="admin.users.actions">${t('admin.users.actions')}</th>
                            </tr>
                        </thead>
                        <tbody id="users-tbody">
                            <tr><td colspan="7" class="text-center py-4"><div class="spinner-border spinner-border-sm text-primary"></div></td></tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <!-- User Modal -->
        <div class="modal fade" id="userModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="userModalTitle">${t('admin.users.addUser')}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <form id="user-form">
                        <div class="modal-body">
                            <input type="hidden" id="userId">
                            <div class="row g-3">
                                <div class="col-md-6"><label for="uFullName" class="form-label">${t('admin.users.fullNameEn')} <span class="text-danger">*</span></label><input type="text" class="form-control" id="uFullName" required></div>
                                <div class="col-md-6"><label for="uFullNameMr" class="form-label">${t('admin.users.fullNameMr')}</label><input type="text" class="form-control" id="uFullNameMr" lang="mr"></div>
                                <div class="col-md-6"><label for="uMobile" class="form-label">${t('admin.users.mobile')} <span class="text-danger">*</span></label><input type="tel" class="form-control" id="uMobile" pattern="[0-9]{10}" maxlength="10" required></div>
                                <div class="col-md-6"><label for="uEmail" class="form-label">${t('admin.users.email')}</label><input type="email" class="form-control" id="uEmail"></div>
                                <div class="col-md-4"><label for="uRole" class="form-label">${t('admin.users.role')} <span class="text-danger">*</span></label>
                                    <select class="form-select" id="uRole" required>
                                        <option value="">${t('common.selectOption')}</option>
                                        ${ROLES.map(r => `<option value="${r}">${r}</option>`).join('')}
                                    </select>
                                </div>
                                <div class="col-md-4"><label for="uDept" class="form-label">${t('admin.users.department')}</label>
                                    <select class="form-select" id="uDept"><option value="">${t('common.selectOption')}</option></select>
                                </div>
                                <div class="col-md-4"><label for="uDesig" class="form-label">${t('admin.users.designation')}</label><input type="text" class="form-control" id="uDesig"></div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" data-i18n="common.cancel">${t('common.cancel')}</button>
                            <button type="submit" class="btn btn-primary" data-i18n="common.save">${t('common.save')}</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>`;

    translatePage(content);
    onLangChange(() => translatePage(content));

    // Load departments for dropdown
    const deptRes = await api.get(`/masters/departments?palikaId=${user.palikaId}`);
    const depts = deptRes.success ? deptRes.data : [];
    const deptSelect = document.getElementById('uDept');
    depts.forEach(d => {
        const opt = document.createElement('option');
        opt.value = d.id;
        opt.textContent = d.name_En;
        deptSelect.appendChild(opt);
    });

    await loadUsers();

    document.getElementById('btn-add-user').addEventListener('click', () => openUserModal(null, depts));
    document.getElementById('user-form').addEventListener('submit', saveUser);
}

async function loadUsers() {
    const res = await api.get('/admin/users?pageSize=100');
    const tbody = document.getElementById('users-tbody');
    if (!res.success || !res.data) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger">Failed to load users</td></tr>`;
        return;
    }
    const users = res.data.items || res.data;
    if (users.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">No users found</td></tr>`;
        return;
    }
    tbody.innerHTML = users.map(u => `
        <tr>
            <td>${escapeHtml(u.fullName_En || u.fullName || '—')}${u.fullName_Mr ? ` <span class="text-muted small" lang="mr">(${escapeHtml(u.fullName_Mr)})</span>` : ''}</td>
            <td>${escapeHtml(u.mobile || '—')}</td>
            <td><span class="badge bg-primary">${u.role}</span></td>
            <td>${escapeHtml(u.departmentName || '—')}</td>
            <td>${u.isActive ? '<i class="bi bi-check-circle-fill text-success"></i>' : '<i class="bi bi-x-circle text-danger"></i>'}</td>
            <td><small>${formatDate(u.createdAt)}</small></td>
            <td>
                <button class="btn btn-outline-primary btn-sm me-1 btn-edit-user" data-id="${u.id}" title="Edit"><i class="bi bi-pencil"></i></button>
                <button class="btn btn-outline-${u.isActive ? 'warning' : 'success'} btn-sm me-1 btn-toggle-user" data-id="${u.id}" title="${u.isActive ? 'Deactivate' : 'Activate'}"><i class="bi bi-${u.isActive ? 'pause' : 'play'}"></i></button>
                <button class="btn btn-outline-danger btn-sm btn-del-user" data-id="${u.id}" title="Delete"><i class="bi bi-trash"></i></button>
            </td>
        </tr>`).join('');

    // Wire actions
    tbody.querySelectorAll('.btn-edit-user').forEach(btn => btn.addEventListener('click', async () => {
        const r = await api.get(`/admin/users/${btn.dataset.id}`);
        if (r.success) openUserModal(r.data);
        else toast.error('Failed to load user');
    }));

    tbody.querySelectorAll('.btn-toggle-user').forEach(btn => btn.addEventListener('click', async () => {
        const r = await api.post(`/admin/users/${btn.dataset.id}/toggle-active`);
        if (r.success) { toast.success('Updated'); await loadUsers(); } else toast.error(r.error || 'Failed');
    }));

    tbody.querySelectorAll('.btn-del-user').forEach(btn => btn.addEventListener('click', async () => {
        if (!confirm('Delete this user?')) return;
        const r = await api.delete(`/admin/users/${btn.dataset.id}`);
        if (r.success) { toast.success('Deleted'); await loadUsers(); } else toast.error(r.error || 'Failed');
    }));
}

function openUserModal(user) {
    document.getElementById('userModalTitle').textContent = user ? t('admin.users.editUser') : t('admin.users.addUser');
    document.getElementById('userId').value = user?.id || '';
    document.getElementById('uFullName').value = user?.fullName_En || user?.fullName || '';
    document.getElementById('uFullNameMr').value = user?.fullName_Mr || '';
    document.getElementById('uMobile').value = user?.mobile || '';
    document.getElementById('uEmail').value = user?.email || '';
    document.getElementById('uRole').value = user?.role || '';
    document.getElementById('uDept').value = user?.departmentId || '';
    document.getElementById('uDesig').value = user?.designation || '';
    new bootstrap.Modal(document.getElementById('userModal')).show();
}

async function saveUser(e) {
    e.preventDefault();
    const id = document.getElementById('userId').value;
    const body = {
        fullName_En: document.getElementById('uFullName').value,
        fullName_Mr: document.getElementById('uFullNameMr').value || null,
        mobile: document.getElementById('uMobile').value,
        email: document.getElementById('uEmail').value || null,
        role: document.getElementById('uRole').value,
        departmentId: document.getElementById('uDept').value || null,
        designation: document.getElementById('uDesig').value || null
    };

    const r = id
        ? await api.put(`/admin/users/${id}`, body)
        : await api.post('/admin/users', body);

    if (r.success) {
        bootstrap.Modal.getInstance(document.getElementById('userModal'))?.hide();
        toast.success(id ? 'User updated' : 'User created');
        await loadUsers();
    } else {
        toast.error(r.error || 'Failed to save');
    }
}
