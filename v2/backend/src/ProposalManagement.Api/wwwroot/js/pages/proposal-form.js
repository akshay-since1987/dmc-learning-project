// Proposal form — create/edit Tab 1
import { api } from '../api.js';
import { getUser } from '../auth.js';
import { toast } from '../toast.js';
import { escapeHtml } from '../utils.js';
import { createDualLangInput } from '../dual-lang-input.js';

export async function renderProposalFormPage(params = {}) {
    const content = document.getElementById('page-content');
    const user = getUser();
    const editId = params.id;
    const isEdit = !!editId;

    content.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary"></div></div>';

    // Load dropdown data
    const palikaId = user.palikaId;
    const [deptRes, zoneRes, reqSrcRes] = await Promise.all([
        api.get(`/masters/departments?palikaId=${palikaId}`),
        api.get(`/masters/zones?palikaId=${palikaId}`),
        api.get(`/masters/request-sources?palikaId=${palikaId}`)
    ]);

    let existing = null;
    if (isEdit) {
        const res = await api.get(`/proposals/${editId}`);
        if (res.success) existing = res.data;
        else { content.innerHTML = `<div class="alert alert-danger">${res.error}</div>`; return; }
    }

    const departments = deptRes.success ? deptRes.data : [];
    const zones = zoneRes.success ? zoneRes.data : [];
    const requestSources = reqSrcRes.success ? reqSrcRes.data : [];

    content.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <a href="#/dashboard" class="text-decoration-none text-muted" style="font-size:0.85rem;">
                    <i class="bi bi-arrow-left me-1"></i>Back
                </a>
                <h4 class="mb-0 mt-1">${isEdit ? 'Edit Proposal' : 'Create New Proposal'}</h4>
            </div>
        </div>

        <form id="proposal-form" novalidate>
            <div class="card mb-3">
                <div class="card-header"><h6 class="mb-0"><i class="bi bi-building me-2"></i>Department & Category</h6></div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label for="departmentId" class="form-label">Department <span class="text-danger">*</span></label>
                            <select class="form-select" id="departmentId" required>
                                <option value="">Select Department</option>
                                ${departments.map(d => `<option value="${d.id}" ${existing?.departmentId === d.id ? 'selected' : ''}>${escapeHtml(d.name_En)}</option>`).join('')}
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label for="deptWorkCategoryId" class="form-label">Work Category <span class="text-danger">*</span></label>
                            <select class="form-select" id="deptWorkCategoryId" required>
                                <option value="">Select Category</option>
                            </select>
                        </div>
                    </div>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header"><h6 class="mb-0"><i class="bi bi-geo-alt me-2"></i>Work Location</h6></div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-4">
                            <label for="zoneId" class="form-label">Zone <span class="text-danger">*</span></label>
                            <select class="form-select" id="zoneId" required>
                                <option value="">Select Zone</option>
                                ${zones.map(z => `<option value="${z.id}" ${existing?.zoneId === z.id ? 'selected' : ''}>${escapeHtml(z.name_En)}</option>`).join('')}
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label for="prabhagId" class="form-label">Prabhag <span class="text-danger">*</span></label>
                            <select class="form-select" id="prabhagId" required>
                                <option value="">Select Prabhag</option>
                            </select>
                        </div>
                        <div class="col-md-4" id="dual-area-container"></div>
                        <div class="col-12" id="dual-locationAddress-container"></div>
                    </div>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header"><h6 class="mb-0"><i class="bi bi-pencil-square me-2"></i>Work Details</h6></div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-12" id="dual-workTitle-container"></div>
                        <div class="col-12" id="dual-workDescription-container"></div>
                    </div>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header"><h6 class="mb-0"><i class="bi bi-person-lines-fill me-2"></i>Request Source</h6></div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-4">
                            <label for="requestSourceId" class="form-label">Source</label>
                            <select class="form-select" id="requestSourceId">
                                <option value="">Select Source</option>
                                ${requestSources.map(s => `<option value="${s.id}" ${existing?.requestSourceId === s.id ? 'selected' : ''}>${escapeHtml(s.name_En)}</option>`).join('')}
                            </select>
                        </div>
                        <div class="col-md-8" id="dual-requestorName-container"></div>
                        <div class="col-md-4">
                            <label for="requestorMobile" class="form-label">Requestor Mobile</label>
                            <input type="tel" class="form-control" id="requestorMobile" maxlength="10" value="${escapeHtml(existing?.requestorMobile || '')}">
                        </div>
                        <div class="col-md-8" id="dual-requestorAddress-container"></div>
                        <div class="col-md-6" id="dual-requestorDesignation-container"></div>
                        <div class="col-md-6" id="dual-requestorOrganisation-container"></div>
                    </div>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-4">
                            <label for="priority" class="form-label">Priority <span class="text-danger">*</span></label>
                            <select class="form-select" id="priority" required>
                                <option value="High" ${existing?.priority === 'High' ? 'selected' : ''}>High</option>
                                <option value="Medium" ${!existing || existing.priority === 'Medium' ? 'selected' : ''}>Medium</option>
                                <option value="Low" ${existing?.priority === 'Low' ? 'selected' : ''}>Low</option>
                            </select>
                        </div>
                    </div>
                </div>
            </div>

            <div class="d-flex gap-2 justify-content-end">
                <a href="#/dashboard" class="btn btn-outline-secondary">Cancel</a>
                <button type="submit" class="btn btn-primary" id="btn-save">
                    <i class="bi bi-floppy me-1"></i>${isEdit ? 'Update' : 'Save Draft'}
                </button>
            </div>
        </form>`;

    // ── Dual-language input components ──
    const dualArea = createDualLangInput({
        name: 'area', label: 'Area', type: 'text',
        required: false, maxLength: 200,
        valueEn: existing?.area || '', valueMr: existing?.area_Mr || '',
        placeholderEn: 'Area/locality name', placeholderMr: 'भाग/परिसराचे नाव'
    });
    document.getElementById('dual-area-container').appendChild(dualArea);

    const dualLocationAddress = createDualLangInput({
        name: 'locationAddress', label: 'Location Address', type: 'textarea',
        required: false, rows: 2, maxLength: 500,
        valueEn: existing?.locationAddress_En || '', valueMr: existing?.locationAddress_Mr || '',
        placeholderEn: 'Location address in English', placeholderMr: 'स्थळाचा पत्ता मराठीत'
    });
    document.getElementById('dual-locationAddress-container').appendChild(dualLocationAddress);

    const dualWorkTitle = createDualLangInput({
        name: 'workTitle', label: 'Work Title', type: 'text',
        required: true, maxLength: 500,
        valueEn: existing?.workTitle_En || '', valueMr: existing?.workTitle_Mr || '',
        placeholderEn: 'Title of the proposed work', placeholderMr: 'प्रस्तावित कामाचे शीर्षक'
    });
    document.getElementById('dual-workTitle-container').appendChild(dualWorkTitle);

    const dualWorkDescription = createDualLangInput({
        name: 'workDescription', label: 'Work Description', type: 'textarea',
        required: true, rows: 4, maxLength: 4000,
        valueEn: existing?.workDescription_En || '', valueMr: existing?.workDescription_Mr || '',
        placeholderEn: 'Detailed description of the proposed work...', placeholderMr: 'प्रस्तावित कामाचे सविस्तर वर्णन...'
    });
    document.getElementById('dual-workDescription-container').appendChild(dualWorkDescription);

    const dualRequestorName = createDualLangInput({
        name: 'requestorName', label: 'Requestor Name', type: 'text',
        required: false, maxLength: 200,
        valueEn: existing?.requestorName || '', valueMr: existing?.requestorName_Mr || '',
        placeholderEn: 'Requestor name', placeholderMr: 'अर्जदाराचे नाव'
    });
    document.getElementById('dual-requestorName-container').appendChild(dualRequestorName);

    const dualRequestorAddress = createDualLangInput({
        name: 'requestorAddress', label: 'Requestor Address', type: 'text',
        required: false, maxLength: 500,
        valueEn: existing?.requestorAddress || '', valueMr: existing?.requestorAddress_Mr || '',
        placeholderEn: 'Requestor address', placeholderMr: 'अर्जदाराचा पत्ता'
    });
    document.getElementById('dual-requestorAddress-container').appendChild(dualRequestorAddress);

    const dualRequestorDesignation = createDualLangInput({
        name: 'requestorDesignation', label: 'Requestor Designation', type: 'text',
        required: false, maxLength: 200,
        valueEn: existing?.requestorDesignation || '', valueMr: existing?.requestorDesignation_Mr || '',
        placeholderEn: 'Designation', placeholderMr: 'पदनाम'
    });
    document.getElementById('dual-requestorDesignation-container').appendChild(dualRequestorDesignation);

    const dualRequestorOrganisation = createDualLangInput({
        name: 'requestorOrganisation', label: 'Requestor Organisation', type: 'text',
        required: false, maxLength: 300,
        valueEn: existing?.requestorOrganisation || '', valueMr: existing?.requestorOrganisation_Mr || '',
        placeholderEn: 'Organisation', placeholderMr: 'संस्था'
    });
    document.getElementById('dual-requestorOrganisation-container').appendChild(dualRequestorOrganisation);

    // ── Cascading dropdowns ──

    // Dept → Work Categories
    const deptSelect = document.getElementById('departmentId');
    const catSelect = document.getElementById('deptWorkCategoryId');

    async function loadCategories() {
        const deptId = deptSelect.value;
        catSelect.innerHTML = '<option value="">Loading...</option>';
        const r = await api.get(`/masters/work-categories?departmentId=${deptId || ''}`);
        catSelect.innerHTML = '<option value="">Select Category</option>';
        if (r.success) {
            r.data.forEach(c => {
                const opt = document.createElement('option');
                opt.value = c.id;
                opt.textContent = c.name_En;
                if (existing?.deptWorkCategoryId === c.id) opt.selected = true;
                catSelect.appendChild(opt);
            });
        }
    }
    deptSelect.addEventListener('change', loadCategories);
    if (deptSelect.value) loadCategories();

    // Zone → Prabhags
    const zoneSelect = document.getElementById('zoneId');
    const prabhagSelect = document.getElementById('prabhagId');

    async function loadPrabhags() {
        const zoneId = zoneSelect.value;
        prabhagSelect.innerHTML = '<option value="">Loading...</option>';
        let url = `/masters/prabhags?palikaId=${palikaId}`;
        if (zoneId) url += `&zoneId=${zoneId}`;
        const r = await api.get(url);
        prabhagSelect.innerHTML = '<option value="">Select Prabhag</option>';
        if (r.success) {
            r.data.forEach(p => {
                const opt = document.createElement('option');
                opt.value = p.id;
                opt.textContent = `${p.code || ''} — ${p.name_En}`;
                if (existing?.prabhagId === p.id) opt.selected = true;
                prabhagSelect.appendChild(opt);
            });
        }
    }
    zoneSelect.addEventListener('change', loadPrabhags);
    if (zoneSelect.value) loadPrabhags();

    // ── Form submit ──
    document.getElementById('proposal-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const btn = document.getElementById('btn-save');
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Saving...';

        const data = {
            departmentId: deptSelect.value,
            deptWorkCategoryId: catSelect.value,
            zoneId: zoneSelect.value,
            prabhagId: prabhagSelect.value,
            area: dualArea.getValues().en || null,
            area_Mr: dualArea.getValues().mr || null,
            locationAddress_En: dualLocationAddress.getValues().en || null,
            locationAddress_Mr: dualLocationAddress.getValues().mr || null,
            workTitle_En: dualWorkTitle.getValues().en,
            workTitle_Mr: dualWorkTitle.getValues().mr || null,
            workDescription_En: dualWorkDescription.getValues().en,
            workDescription_Mr: dualWorkDescription.getValues().mr || null,
            requestSourceId: document.getElementById('requestSourceId').value || null,
            requestorName: dualRequestorName.getValues().en || null,
            requestorName_Mr: dualRequestorName.getValues().mr || null,
            requestorMobile: document.getElementById('requestorMobile').value.trim() || null,
            requestorAddress: dualRequestorAddress.getValues().en || null,
            requestorAddress_Mr: dualRequestorAddress.getValues().mr || null,
            requestorDesignation: dualRequestorDesignation.getValues().en || null,
            requestorDesignation_Mr: dualRequestorDesignation.getValues().mr || null,
            requestorOrganisation: dualRequestorOrganisation.getValues().en || null,
            requestorOrganisation_Mr: dualRequestorOrganisation.getValues().mr || null,
            priority: document.getElementById('priority').value
        };

        let res;
        if (isEdit) {
            data.id = editId;
            res = await api.put(`/proposals/${editId}`, data);
        } else {
            res = await api.post('/proposals', data);
        }

        btn.disabled = false;
        btn.innerHTML = `<i class="bi bi-floppy me-1"></i>${isEdit ? 'Update' : 'Save Draft'}`;

        if (res.success) {
            toast.success(isEdit ? 'Proposal updated' : 'Proposal saved as draft');
            const navId = isEdit ? editId : res.data?.id;
            window.location.hash = navId ? `#/proposals/${navId}` : '#/proposals/my';
        } else {
            toast.error(res.error || 'Save failed');
        }
    });
}
