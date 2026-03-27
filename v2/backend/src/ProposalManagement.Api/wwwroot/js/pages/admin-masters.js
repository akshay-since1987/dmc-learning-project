// Admin Masters page — Lotus-only master data management
import { api } from '../api.js';
import { toast } from '../toast.js';
import { escapeHtml, formatDate } from '../utils.js';
import { t, tBilingual, translatePage, onLangChange } from '../i18n.js';
import { createDualLangInput } from '../dual-lang-input.js';

const MASTER_TYPES = [
    { key: 'departments', i18nKey: 'admin.masters.departments', hasCode: true },
    { key: 'zones', i18nKey: 'admin.masters.zones', hasCode: true },
    { key: 'designations', i18nKey: 'admin.masters.designations', hasCode: false },
    { key: 'fund-types', i18nKey: 'admin.masters.fundTypes', hasCode: false },
    { key: 'work-methods', i18nKey: 'admin.masters.workMethods', hasCode: false },
    { key: 'budget-heads', i18nKey: 'admin.masters.budgetHeads', hasCode: true },
    { key: 'request-sources', i18nKey: 'admin.masters.requestSources', hasCode: false },
    { key: 'work-categories', i18nKey: 'admin.masters.workCategories', hasCode: false },
    { key: 'site-conditions', i18nKey: 'admin.masters.siteConditions', hasCode: false }
];

let activeMasterType = MASTER_TYPES[0];

export async function renderAdminMastersPage() {
    const content = document.getElementById('page-content');

    content.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h4 class="mb-0"><i class="bi bi-database me-2"></i><span data-i18n="admin.masters.title">${t('admin.masters.title')}</span></h4>
        </div>
        <div class="card">
            <div class="card-header p-0">
                <ul class="nav nav-tabs card-header-tabs" id="masterTabs" role="tablist">
                    ${MASTER_TYPES.map((m, i) => `
                        <li class="nav-item" role="presentation">
                            <button class="nav-link ${i === 0 ? 'active' : ''} px-3 py-2" style="font-size:0.8rem;"
                                data-type="${m.key}" role="tab" data-i18n="${m.i18nKey}">${t(m.i18nKey)}</button>
                        </li>`).join('')}
                </ul>
            </div>
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div class="input-group" style="max-width:300px;">
                        <span class="input-group-text"><i class="bi bi-search"></i></span>
                        <input type="text" class="form-control form-control-sm" id="masterSearch" data-i18n-placeholder="admin.masters.search" placeholder="${t('admin.masters.search')}" aria-label="Search masters">
                    </div>
                    <button class="btn btn-primary btn-sm" id="btn-add-master">
                        <i class="bi bi-plus me-1"></i><span data-i18n="admin.masters.addItem">${t('admin.masters.addItem')}</span>
                    </button>
                </div>
                <div class="table-responsive">
                    <table class="table table-hover mb-0 data-table">
                        <thead class="table-light">
                            <tr>
                                <th data-i18n="admin.masters.nameEn">${t('admin.masters.nameEn')}</th>
                                <th data-i18n="admin.masters.nameMr">${t('admin.masters.nameMr')}</th>
                                <th id="codeHeader" data-i18n="admin.masters.code">${t('admin.masters.code')}</th>
                                <th data-i18n="common.active">${t('common.active')}</th>
                                <th data-i18n="common.created">${t('common.created')}</th>
                                <th style="width:100px;" data-i18n="common.actions">${t('common.actions')}</th>
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
                        <h5 class="modal-title" id="masterModalTitle">${t('admin.masters.addItem')}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <form id="master-form">
                        <div class="modal-body">
                            <input type="hidden" id="masterId">
                            <div id="dual-masterName-container"></div>
                            <div class="mb-3" id="codeField">
                                <label for="mCode" class="form-label" data-i18n="admin.masters.code">${t('admin.masters.code')}</label>
                                <input type="text" class="form-control" id="mCode">
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

    // Tab switching
    document.querySelectorAll('#masterTabs button').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('#masterTabs button').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            activeMasterType = MASTER_TYPES.find(m => m.key === btn.dataset.type);
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
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger">${t('common.loadError')}</td></tr>`;
        return;
    }

    const items = res.data;
    if (items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted">${t('admin.masters.noRecords')}</td></tr>`;
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
            if (!confirm(t('admin.masters.deleteConfirm'))) return;
            const r = await api.delete(`/admin/masters/${activeMasterType.key}/${btn.dataset.id}`);
            if (r.success) { toast.success(t('common.deleted')); await loadMasters(); }
            else toast.error(r.error || t('common.loadError'));
        });
    });
}

let masterNameDual = null;

function openMasterModal(item) {
    document.getElementById('masterModalTitle').textContent = item ? t('admin.masters.editItem') : t('admin.masters.addItem');
    document.getElementById('masterId').value = item?.id || '';

    // Recreate dual-lang input each time modal opens
    const container = document.getElementById('dual-masterName-container');
    container.innerHTML = '';
    masterNameDual = createDualLangInput({
        name: 'masterName', label: t('admin.masters.nameLabel'), i18nKey: 'admin.masters.nameLabel',
        type: 'text', required: true, maxLength: 200,
        valueEn: item?.name_En || '', valueMr: item?.name_Mr || ''
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
        toast.success(id ? t('common.updated') : t('common.created'));
        await loadMasters();
    } else {
        toast.error(r.error || t('common.loadError'));
    }
}


