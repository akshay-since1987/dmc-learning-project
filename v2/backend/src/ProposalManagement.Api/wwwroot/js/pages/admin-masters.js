// Admin Masters page — Lotus-only master data management
import { api } from '../api.js';
import { toast } from '../toast.js';
import { escapeHtml, formatDate } from '../utils.js';

const MASTER_TYPES = [
    { key: 'departments', label: 'Departments', hasCode: true },
    { key: 'zones', label: 'Zones', hasCode: true },
    { key: 'designations', label: 'Designations', hasCode: false },
    { key: 'fund-types', label: 'Fund Types', hasCode: false },
    { key: 'work-methods', label: 'Work Methods', hasCode: false },
    { key: 'budget-heads', label: 'Budget Heads', hasCode: true },
    { key: 'request-sources', label: 'Request Sources', hasCode: false },
    { key: 'work-categories', label: 'Work Categories', hasCode: false },
    { key: 'site-conditions', label: 'Site Conditions', hasCode: false }
];

let activeMasterType = MASTER_TYPES[0];

export async function renderAdminMastersPage() {
    const content = document.getElementById('page-content');

    content.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h4 class="mb-0"><i class="bi bi-database me-2"></i>Master Data Management</h4>
        </div>
        <div class="card">
            <div class="card-header p-0">
                <ul class="nav nav-tabs card-header-tabs" id="masterTabs" role="tablist">
                    ${MASTER_TYPES.map((m, i) => `
                        <li class="nav-item" role="presentation">
                            <button class="nav-link ${i === 0 ? 'active' : ''} px-3 py-2" style="font-size:0.8rem;"
                                data-type="${m.key}" role="tab">${m.label}</button>
                        </li>`).join('')}
                </ul>
            </div>
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div class="input-group" style="max-width:300px;">
                        <span class="input-group-text"><i class="bi bi-search"></i></span>
                        <input type="text" class="form-control form-control-sm" id="masterSearch" placeholder="Search..." aria-label="Search masters">
                    </div>
                    <button class="btn btn-primary btn-sm" id="btn-add-master">
                        <i class="bi bi-plus me-1"></i>Add <span id="addMasterLabel">${MASTER_TYPES[0].label.slice(0, -1)}</span>
                    </button>
                </div>
                <div class="table-responsive">
                    <table class="table table-hover mb-0 data-table">
                        <thead class="table-light">
                            <tr>
                                <th>Name (English)</th>
                                <th>नाव (मराठी)</th>
                                <th id="codeHeader">Code</th>
                                <th>Active</th>
                                <th>Created</th>
                                <th style="width:100px;">Actions</th>
                            </tr>
                        </thead>
                        <tbody id="masters-tbody">
                            <tr><td colspan="6" class="text-center py-4">
                                <div class="spinner-border spinner-border-sm text-primary"></div>
                            </td></tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <!-- Master Modal -->
        <div class="modal fade" id="masterModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="masterModalTitle">Add Item</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <form id="master-form">
                        <div class="modal-body">
                            <input type="hidden" id="masterId">
                            <div id="dual-masterName-container"></div>
                            <div class="mb-3" id="codeField">
                                <label for="mCode" class="form-label">Code</label>
                                <input type="text" class="form-control" id="mCode">
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="submit" class="btn btn-primary">Save</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>`;

    // Tab switching
    document.querySelectorAll('#masterTabs button').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('#masterTabs button').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            activeMasterType = MASTER_TYPES.find(m => m.key === btn.dataset.type);
            document.getElementById('addMasterLabel').textContent = singularize(activeMasterType.label);
            document.getElementById('codeHeader').style.display = activeMasterType.hasCode ? '' : 'none';
            document.getElementById('masterSearch').value = '';
            loadMasters();
        });
    });

    // Search
    let searchTimer;
    document.getElementById('masterSearch').addEventListener('input', (e) => {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(() => loadMasters(e.target.value), 300);
    });

    // Add button
    document.getElementById('btn-add-master').addEventListener('click', () => openMasterModal(null));
    document.getElementById('master-form').addEventListener('submit', saveMaster);

    await loadMasters();
}

async function loadMasters(search) {
    const tbody = document.getElementById('masters-tbody');
    tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4"><div class="spinner-border spinner-border-sm text-primary"></div></td></tr>`;

    const qs = search ? `?search=${encodeURIComponent(search)}` : '';
    const res = await api.get(`/admin/masters/${activeMasterType.key}${qs}`);

    if (!res.success || !res.data) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger">Failed to load</td></tr>`;
        return;
    }

    const items = res.data;
    if (items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted">No records found</td></tr>`;
        return;
    }

    const hasCode = activeMasterType.hasCode;
    tbody.innerHTML = items.map(item => `
        <tr>
            <td>${escapeHtml(item.name_En)}</td>
            <td lang="mr">${escapeHtml(item.name_Mr || '—')}</td>
            <td ${hasCode ? '' : 'style="display:none"'}>${escapeHtml(item.code || '—')}</td>
            <td>${item.isActive ? '<i class="bi bi-check-circle-fill text-success"></i>' : '<i class="bi bi-x-circle text-danger"></i>'}</td>
            <td><small>${formatDate(item.createdAt)}</small></td>
            <td>
                <button class="btn btn-outline-primary btn-sm me-1 btn-edit-master" data-id="${item.id}"
                    data-name-en="${escapeHtml(item.name_En)}" data-name-mr="${escapeHtml(item.name_Mr || '')}"
                    data-code="${escapeHtml(item.code || '')}" title="Edit">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-outline-danger btn-sm btn-del-master" data-id="${item.id}" title="Delete">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        </tr>`).join('');

    // Wire actions
    tbody.querySelectorAll('.btn-edit-master').forEach(btn => {
        btn.addEventListener('click', () => {
            openMasterModal({
                id: btn.dataset.id,
                name_En: btn.dataset.nameEn,
                name_Mr: btn.dataset.nameMr,
                code: btn.dataset.code
            });
        });
    });

    tbody.querySelectorAll('.btn-del-master').forEach(btn => {
        btn.addEventListener('click', async () => {
            if (!confirm(`Delete this ${singularize(activeMasterType.label)}?`)) return;
            const r = await api.delete(`/admin/masters/${activeMasterType.key}/${btn.dataset.id}`);
            if (r.success) { toast.success('Deleted'); await loadMasters(); }
            else toast.error(r.error || 'Failed to delete');
        });
    });
}

let masterNameDual = null;

function openMasterModal(item) {
    const singular = singularize(activeMasterType.label);
    document.getElementById('masterModalTitle').textContent = item ? `Edit ${singular}` : `Add ${singular}`;
    document.getElementById('masterId').value = item?.id || '';

    // Recreate dual-lang input each time modal opens
    const container = document.getElementById('dual-masterName-container');
    container.innerHTML = '';
    masterNameDual = createDualLangInput({
        name: 'masterName', label: 'Name', type: 'text',
        required: true, maxLength: 200,
        valueEn: item?.name_En || '', valueMr: item?.name_Mr || '',
        placeholderEn: 'Name in English', placeholderMr: 'नाव मराठीत'
    });
    container.appendChild(masterNameDual);

    document.getElementById('mCode').value = item?.code || '';
    document.getElementById('codeField').style.display = activeMasterType.hasCode ? '' : 'none';
    new bootstrap.Modal(document.getElementById('masterModal')).show();
}

async function saveMaster(e) {
    e.preventDefault();
    const id = document.getElementById('masterId').value;
    const vals = masterNameDual.getValues();
    const body = {
        name_En: vals.en,
        name_Mr: vals.mr || null,
        code: document.getElementById('mCode').value || null
    };

    const r = id
        ? await api.put(`/admin/masters/${activeMasterType.key}/${id}`, body)
        : await api.post(`/admin/masters/${activeMasterType.key}`, body);

    if (r.success) {
        bootstrap.Modal.getInstance(document.getElementById('masterModal'))?.hide();
        toast.success(id ? 'Updated' : 'Created');
        await loadMasters();
    } else {
        toast.error(r.error || 'Failed to save');
    }
}

function singularize(label) {
    if (label.endsWith('ies')) return label.slice(0, -3) + 'y';
    if (label.endsWith('ses')) return label.slice(0, -2);
    if (label.endsWith('s')) return label.slice(0, -1);
    return label;
}
