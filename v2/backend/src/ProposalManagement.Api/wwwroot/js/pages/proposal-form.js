// Proposal form — create/edit Tab 1
import { api } from '../api.js';
import { getUser } from '../auth.js';
import { toast } from '../toast.js';
import { escapeHtml } from '../utils.js';

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
                        <div class="col-md-4">
                            <label for="area" class="form-label">Area</label>
                            <input type="text" class="form-control" id="area" value="${escapeHtml(existing?.area || '')}" placeholder="Area/locality name">
                        </div>
                        <div class="col-md-6">
                            <label for="locationAddress_En" class="form-label">Location Address (English)</label>
                            <textarea class="form-control" id="locationAddress_En" rows="2">${escapeHtml(existing?.locationAddress_En || '')}</textarea>
                        </div>
                        <div class="col-md-6">
                            <label for="locationAddress_Mr" class="form-label">स्थळाचा पत्ता (मराठी)</label>
                            <textarea class="form-control" id="locationAddress_Mr" rows="2" lang="mr">${escapeHtml(existing?.locationAddress_Mr || '')}</textarea>
                        </div>
                    </div>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header"><h6 class="mb-0"><i class="bi bi-pencil-square me-2"></i>Work Details</h6></div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label for="workTitle_En" class="form-label">Work Title (English) <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="workTitle_En" required maxlength="500"
                                value="${escapeHtml(existing?.workTitle_En || '')}" placeholder="Title of the proposed work">
                        </div>
                        <div class="col-md-6">
                            <label for="workTitle_Mr" class="form-label">कामाचे शीर्षक (मराठी)</label>
                            <input type="text" class="form-control" id="workTitle_Mr" maxlength="500" lang="mr"
                                value="${escapeHtml(existing?.workTitle_Mr || '')}">
                        </div>
                        <div class="col-md-6">
                            <label for="workDescription_En" class="form-label">Work Description (English) <span class="text-danger">*</span></label>
                            <textarea class="form-control" id="workDescription_En" rows="4" required maxlength="4000"
                                placeholder="Detailed description of the proposed work...">${escapeHtml(existing?.workDescription_En || '')}</textarea>
                        </div>
                        <div class="col-md-6">
                            <label for="workDescription_Mr" class="form-label">कामाचे वर्णन (मराठी)</label>
                            <textarea class="form-control" id="workDescription_Mr" rows="4" maxlength="4000" lang="mr">${escapeHtml(existing?.workDescription_Mr || '')}</textarea>
                        </div>
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
                        <div class="col-md-4">
                            <label for="requestorName" class="form-label">Requestor Name</label>
                            <input type="text" class="form-control" id="requestorName" value="${escapeHtml(existing?.requestorName || '')}">
                        </div>
                        <div class="col-md-4">
                            <label for="requestorMobile" class="form-label">Requestor Mobile</label>
                            <input type="tel" class="form-control" id="requestorMobile" maxlength="10" value="${escapeHtml(existing?.requestorMobile || '')}">
                        </div>
                        <div class="col-md-4">
                            <label for="requestorAddress" class="form-label">Address</label>
                            <input type="text" class="form-control" id="requestorAddress" value="${escapeHtml(existing?.requestorAddress || '')}">
                        </div>
                        <div class="col-md-4">
                            <label for="requestorDesignation" class="form-label">Designation</label>
                            <input type="text" class="form-control" id="requestorDesignation" value="${escapeHtml(existing?.requestorDesignation || '')}">
                        </div>
                        <div class="col-md-4">
                            <label for="requestorOrganisation" class="form-label">Organisation</label>
                            <input type="text" class="form-control" id="requestorOrganisation" value="${escapeHtml(existing?.requestorOrganisation || '')}">
                        </div>
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
            area: document.getElementById('area').value.trim() || null,
            locationAddress_En: document.getElementById('locationAddress_En').value.trim() || null,
            locationAddress_Mr: document.getElementById('locationAddress_Mr').value.trim() || null,
            workTitle_En: document.getElementById('workTitle_En').value.trim(),
            workTitle_Mr: document.getElementById('workTitle_Mr').value.trim() || null,
            workDescription_En: document.getElementById('workDescription_En').value.trim(),
            workDescription_Mr: document.getElementById('workDescription_Mr').value.trim() || null,
            requestSourceId: document.getElementById('requestSourceId').value || null,
            requestorName: document.getElementById('requestorName').value.trim() || null,
            requestorMobile: document.getElementById('requestorMobile').value.trim() || null,
            requestorAddress: document.getElementById('requestorAddress').value.trim() || null,
            requestorDesignation: document.getElementById('requestorDesignation').value.trim() || null,
            requestorOrganisation: document.getElementById('requestorOrganisation').value.trim() || null,
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
