// Proposal detail page — full 6-tab view with approval timeline
import { api } from '../api.js';
import { getUser, hasRole } from '../auth.js';
import { toast } from '../toast.js';
import { stageBadge, formatDate, formatCurrency, escapeHtml } from '../utils.js';

let currentProposal = null;
let currentTab = '1';

export async function renderProposalDetailPage(params) {
    const content = document.getElementById('page-content');
    const user = getUser();
    const id = params.id;

    content.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary"></div></div>';

    const res = await api.get(`/proposals/${id}`);
    if (!res.success) {
        content.innerHTML = `<div class="alert alert-danger">${escapeHtml(res.error || 'Failed to load proposal')}</div>`;
        return;
    }

    currentProposal = res.data;
    const p = currentProposal;
    const isOwner = p.currentOwnerId === user.id;
    const isCreator = p.createdById === user.id;
    const canEdit = isCreator && (p.currentStage === 'Draft' || p.currentStage === 'PushedBack');
    const canApprove = isOwner && !['Draft', 'PushedBack', 'Approved', 'Cancelled'].includes(p.currentStage);
    const canEditTabs = isCreator || isOwner || hasRole('Lotus');

    content.innerHTML = `
        <div class="d-flex justify-content-between align-items-start mb-3 flex-wrap gap-2">
            <div>
                <a href="#/dashboard" class="text-decoration-none text-muted" style="font-size:0.85rem;">
                    <i class="bi bi-arrow-left me-1"></i>Back
                </a>
                <h4 class="mb-1 mt-1">${escapeHtml(p.proposalNumber)}</h4>
                <p class="text-muted mb-0" style="font-size:0.85rem;">
                    ${stageBadge(p.currentStage)}
                    <span class="ms-2">Created by ${escapeHtml(p.createdByName)} on ${formatDate(p.createdAt)}</span>
                    <span class="ms-2 badge bg-light text-dark">Tab ${p.completedTab || 1}/6</span>
                </p>
            </div>
            <div class="d-flex gap-2 flex-wrap">
                ${canEdit ? `<a href="#/proposals/${id}/edit" class="btn btn-primary btn-sm"><i class="bi bi-pencil me-1"></i>Edit Tab 1</a>` : ''}
                ${canApprove ? `
                    <button class="btn btn-success btn-sm" id="btn-approve"><i class="bi bi-check-lg me-1"></i>Approve</button>
                    <button class="btn btn-warning btn-sm" id="btn-pushback"><i class="bi bi-arrow-return-left me-1"></i>Push Back</button>
                ` : ''}
                ${isCreator && (p.currentStage === 'Draft' || p.currentStage === 'PushedBack') ? `
                    <button class="btn btn-outline-primary btn-sm" id="btn-submit"><i class="bi bi-send me-1"></i>Submit</button>
                ` : ''}
            </div>
        </div>

        <!-- Tab Navigation -->
        <ul class="nav nav-tabs proposal-tabs mb-0" role="tablist">
            <li class="nav-item"><button class="nav-link active" data-tab="1"><i class="bi bi-file-text me-1"></i>Proposal</button></li>
            <li class="nav-item"><button class="nav-link" data-tab="2"><i class="bi bi-geo-alt me-1"></i>Field Visit</button></li>
            <li class="nav-item"><button class="nav-link" data-tab="3"><i class="bi bi-calculator me-1"></i>Estimate</button></li>
            <li class="nav-item"><button class="nav-link" data-tab="4"><i class="bi bi-shield-check me-1"></i>Tech Sanction</button></li>
            <li class="nav-item"><button class="nav-link" data-tab="5"><i class="bi bi-receipt me-1"></i>PRAMA</button></li>
            <li class="nav-item"><button class="nav-link" data-tab="6"><i class="bi bi-wallet2 me-1"></i>Budget</button></li>
            <li class="nav-item"><button class="nav-link" data-tab="timeline"><i class="bi bi-clock-history me-1"></i>Timeline</button></li>
            <li class="nav-item"><button class="nav-link" data-tab="docs"><i class="bi bi-paperclip me-1"></i>Additional Docs</button></li>
        </ul>

        <div class="card border-top-0 rounded-0 rounded-bottom">
            <div class="card-body" id="tab-content"></div>
        </div>

        <!-- Approve Modal -->
        <div class="modal fade" id="approveModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog"><div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">Approve Proposal</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
                <form id="approve-form"><div class="modal-body">
                    <div class="mb-3"><label for="opinion" class="form-label">Opinion / Remarks</label><textarea class="form-control" id="opinion" rows="3"></textarea></div>
                    <div class="form-check"><input class="form-check-input" type="checkbox" id="disclaimer-check" required><label class="form-check-label" for="disclaimer-check">I accept the disclaimer and confirm this approval.</label></div>
                </div><div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button><button type="submit" class="btn btn-success">Confirm Approve</button></div></form>
            </div></div>
        </div>

        <!-- PushBack Modal -->
        <div class="modal fade" id="pushbackModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog"><div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">Push Back Proposal</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
                <form id="pushback-form"><div class="modal-body">
                    <div class="mb-3"><label for="pushback-note" class="form-label">Reason <span class="text-danger">*</span></label><textarea class="form-control" id="pushback-note" rows="3" required></textarea></div>
                </div><div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button><button type="submit" class="btn btn-warning">Confirm Push Back</button></div></form>
            </div></div>
        </div>`;

    // Wire tab navigation
    document.querySelectorAll('.proposal-tabs .nav-link').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.proposal-tabs .nav-link').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            currentTab = btn.dataset.tab;
            loadTab(id, currentTab, canEditTabs);
        });
    });

    // Wire workflow buttons
    wireWorkflowButtons(id, params);

    // Load first tab
    loadTab(id, '1', canEditTabs);
}

// ── Tab Loading ──────────────────────────────────────────────
async function loadTab(proposalId, tab, canEdit) {
    const c = document.getElementById('tab-content');
    c.innerHTML = '<div class="text-center py-4"><div class="spinner-border spinner-border-sm text-primary"></div></div>';

    switch (tab) {
        case '1': renderTab1(c); break;
        case '2': await renderTab2(c, proposalId, canEdit); break;
        case '3': await renderTab3(c, proposalId, canEdit); break;
        case '4': await renderTab4(c, proposalId, canEdit); break;
        case '5': await renderTab5(c, proposalId, canEdit); break;
        case '6': await renderTab6(c, proposalId, canEdit); break;
        case 'timeline': await renderTimeline(c, proposalId); break;
        case 'docs': await renderDocuments(c, proposalId, canEdit); break;
        default: c.innerHTML = '<p class="text-muted">Unknown tab</p>';
    }
}

// ── Tab 1: Proposal Details (read-only) ──────────────────────
function renderTab1(c) {
    const p = currentProposal;
    c.innerHTML = `
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Department</label><div class="fw-medium">${escapeHtml(p.departmentName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Work Category</label><div class="fw-medium">${escapeHtml(p.workCategoryName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Priority</label><div><span class="badge bg-${p.priority === 'High' ? 'danger' : p.priority === 'Low' ? 'secondary' : 'warning text-dark'}">${p.priority}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Zone</label><div>${escapeHtml(p.zoneName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Prabhag</label><div>${escapeHtml(p.prabhagName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Area</label><div>${escapeHtml(p.area || '—')}</div></div>
            <div class="col-12"><label class="form-label text-muted small mb-0">Work Title</label><div class="fw-medium">${escapeHtml(p.workTitle_En)}</div>${p.workTitle_Mr ? `<div class="text-muted" lang="mr">${escapeHtml(p.workTitle_Mr)}</div>` : ''}</div>
            <div class="col-12"><label class="form-label text-muted small mb-0">Description</label><div>${escapeHtml(p.workDescription_En)}</div>${p.workDescription_Mr ? `<div class="text-muted" lang="mr">${escapeHtml(p.workDescription_Mr)}</div>` : ''}</div>
            <div class="col-12"><label class="form-label text-muted small mb-0">Location Address</label><div>${escapeHtml(p.locationAddress_En || '—')}</div></div>
            ${p.requestSourceName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Request Source</label><div>${escapeHtml(p.requestSourceName)}</div></div>` : ''}
            ${p.requestorName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Requestor</label><div>${escapeHtml(p.requestorName)}</div></div>` : ''}
            ${p.requestorMobile ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Requestor Mobile</label><div>${escapeHtml(p.requestorMobile)}</div></div>` : ''}
        </div>`;
}

// ── Tab 2: Field Visits ──────────────────────────────────────
async function renderTab2(c, pid, canEdit) {
    const res = await api.get(`/proposals/${pid}/field-visits`);
    const visits = res.success ? (res.data || []) : [];
    const totalPhotos = visits.reduce((sum, fv) => sum + (fv.photos?.length || 0), 0);
    const hasCompleted = visits.some(fv => fv.status === 'Completed' && fv.photos?.length > 0);

    let html = `<div class="d-flex justify-content-between align-items-center mb-3">
        <h6 class="mb-0"><i class="bi bi-geo-alt me-1"></i>Field Visits (${visits.length})</h6>
        ${canEdit ? `<button class="btn btn-primary btn-sm" id="btn-assign-fv"><i class="bi bi-plus me-1"></i>Assign Visit</button>` : ''}
    </div>`;

    // ── Mandatory photo status banner ──
    if (!hasCompleted) {
        html += `<div class="alert alert-warning d-flex align-items-center py-2 mb-3" role="alert">
            <i class="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
            <div><strong>Field visit photos are mandatory.</strong> At least one completed field visit with photos is required before the proposal can be submitted for approval.</div>
        </div>`;
    } else {
        html += `<div class="alert alert-success d-flex align-items-center py-2 mb-3" role="alert">
            <i class="bi bi-check-circle-fill me-2 fs-5"></i>
            <div><strong>${totalPhotos} photo(s) uploaded</strong> across ${visits.filter(fv => fv.status === 'Completed').length} completed visit(s). Field visit requirement satisfied.</div>
        </div>`;
    }

    if (visits.length === 0) {
        html += '<p class="text-muted">No field visits assigned yet. Assign a visit to begin site inspection.</p>';
    } else {
        visits.forEach(fv => {
            const sBg = fv.status === 'Completed' ? 'success' : fv.status === 'InProgress' ? 'primary' : 'secondary';
            const isAssignee = canEdit && fv.status !== 'Completed';
            const photoCount = fv.photos?.length || 0;

            html += `<div class="card mb-3 ${fv.status === 'Completed' ? 'border-success' : ''}">
                <div class="card-header bg-light py-2 d-flex justify-content-between align-items-center">
                    <div>
                        <strong>Visit #${fv.visitNumber}</strong>
                        <span class="badge bg-${sBg} ms-2">${fv.status}</span>
                        <span class="text-muted ms-2 small">Assigned: ${escapeHtml(fv.assignedToName || '—')}</span>
                    </div>
                    <small class="text-muted">${formatDate(fv.createdAt)}</small>
                </div>
                <div class="card-body py-3">
                    ${fv.siteConditionName ? `<div class="small mb-1">Site Condition: <strong>${escapeHtml(fv.siteConditionName)}</strong></div>` : ''}
                    ${fv.problemDescription_En ? `<div class="small mb-1">${escapeHtml(fv.problemDescription_En)}</div>` : ''}
                    ${fv.recommendation_En ? `<div class="small mb-1 text-success"><i class="bi bi-chat-right-text me-1"></i>Rec: ${escapeHtml(fv.recommendation_En)}</div>` : ''}
                    ${fv.gpsLatitude ? `<div class="small mb-2 text-muted"><i class="bi bi-geo-alt me-1"></i>${fv.gpsLatitude}, ${fv.gpsLongitude}</div>` : ''}
                    ${fv.completedAt ? `<div class="small text-success mb-2"><i class="bi bi-check-circle me-1"></i>Completed: ${formatDate(fv.completedAt)}</div>` : ''}

                    <!-- ── Site Photos Section ── -->
                    <div class="border rounded p-3 bg-light mt-2">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <h6 class="mb-0 small fw-bold"><i class="bi bi-images me-1"></i>Site Photos (${photoCount})</h6>
                            ${isAssignee ? `<label class="btn btn-primary btn-sm mb-0" role="button" tabindex="0">
                                <i class="bi bi-camera-fill me-1"></i>Upload Photos
                                <input type="file" class="d-none fv-photo-input" data-id="${fv.id}" accept="image/*" multiple>
                            </label>` : ''}
                        </div>

                        ${photoCount === 0 && isAssignee ? `
                            <div class="text-center py-4 border border-2 border-dashed rounded bg-white fv-drop-zone" data-id="${fv.id}">
                                <i class="bi bi-cloud-arrow-up fs-1 text-muted"></i>
                                <p class="text-muted mb-1">Drag & drop site photos here</p>
                                <p class="text-muted small mb-0">or click "Upload Photos" above (JPEG, PNG, WebP — max 5 MB each)</p>
                            </div>` : ''}

                        ${photoCount === 0 && !isAssignee ? `<p class="text-muted small mb-0">No photos uploaded yet.</p>` : ''}

                        ${photoCount > 0 ? `
                            <div class="d-flex flex-wrap gap-2">
                                ${fv.photos.map(p => `
                                    <div class="position-relative border rounded overflow-hidden" style="width:110px;height:110px;">
                                        <img src="${p.storagePath || p.fileName}" alt="${escapeHtml(p.caption || p.fileName)}"
                                            class="w-100 h-100" style="object-fit:cover;cursor:pointer;"
                                            onclick="window.open(this.src,'_blank')" title="Click to view full size">
                                        ${isAssignee ? `<button class="btn btn-danger position-absolute top-0 end-0 p-0 lh-1 btn-del-photo rounded-0"
                                            data-fv-id="${fv.id}" data-photo-id="${p.id}" style="width:22px;height:22px;font-size:11px;opacity:0.85;" title="Delete photo">
                                            <i class="bi bi-x-lg"></i></button>` : ''}
                                    </div>`).join('')}
                                ${isAssignee ? `
                                    <label class="border border-2 border-dashed rounded d-flex align-items-center justify-content-center bg-white" 
                                        style="width:110px;height:110px;cursor:pointer;" role="button" tabindex="0" title="Add more photos">
                                        <div class="text-center text-muted">
                                            <i class="bi bi-plus-lg fs-4"></i><br><small>Add more</small>
                                        </div>
                                        <input type="file" class="d-none fv-photo-input" data-id="${fv.id}" accept="image/*" multiple>
                                    </label>` : ''}
                            </div>` : ''}
                    </div>

                    ${isAssignee && !fv.completedAt ? `
                        <div class="mt-3">
                            <button class="btn btn-success btn-sm btn-complete-fv" data-id="${fv.id}"><i class="bi bi-check-circle me-1"></i>Mark as Complete</button>
                        </div>` : ''}
                </div>
            </div>`;
        });
    }

    // Assign Visit modal
    html += `<div class="modal fade" id="assignFvModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-sm">
            <div class="modal-content">
                <div class="modal-header py-2">
                    <h6 class="modal-title mb-0">Assign Field Visit</h6>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <label for="fv-engineer-select" class="form-label">Select Engineer (JE / TS)</label>
                    <select class="form-select" id="fv-engineer-select" required>
                        <option value="">Loading...</option>
                    </select>
                </div>
                <div class="modal-footer py-2">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-primary btn-sm" id="btn-confirm-assign-fv">Assign</button>
                </div>
            </div>
        </div>
    </div>`;

    c.innerHTML = html;

    // Wire: Assign Visit button → open modal and load engineers
    const btnAssign = document.getElementById('btn-assign-fv');
    if (btnAssign) btnAssign.addEventListener('click', async () => {
        const modal = new bootstrap.Modal(document.getElementById('assignFvModal'));
        const select = document.getElementById('fv-engineer-select');
        select.innerHTML = '<option value="">Loading...</option>';
        modal.show();

        const engRes = await api.get(`/proposals/${pid}/field-visits/assignable-engineers`);
        if (engRes.success && engRes.data) {
            select.innerHTML = '<option value="">— Select an Engineer —</option>' +
                engRes.data.map(e => `<option value="${e.id}">${escapeHtml(e.fullName_En)} (${e.role}${e.departmentName ? ' — ' + escapeHtml(e.departmentName) : ''})</option>`).join('');
        } else {
            select.innerHTML = '<option value="">No engineers found</option>';
        }
    });

    // Wire: Confirm assign
    document.getElementById('btn-confirm-assign-fv')?.addEventListener('click', async () => {
        const engineerId = document.getElementById('fv-engineer-select').value;
        if (!engineerId) { toast.error('Please select an engineer'); return; }
        const r = await api.post(`/proposals/${pid}/field-visits/assign`, { assignedToId: engineerId });
        bootstrap.Modal.getInstance(document.getElementById('assignFvModal'))?.hide();
        if (r.success) { toast.success('Visit assigned'); await renderTab2(c, pid, canEdit); } 
        else toast.error(r.error || 'Failed to assign');
    });

    // Wire: Complete buttons
    c.querySelectorAll('.btn-complete-fv').forEach(btn =>
        btn.addEventListener('click', async () => {
            const r = await api.post(`/proposals/${pid}/field-visits/${btn.dataset.id}/complete`);
            if (r.success) { toast.success('Visit completed'); await renderTab2(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        })
    );

    // Wire: Photo upload inputs (both "Upload Photos" buttons and "Add more" tiles)
    c.querySelectorAll('.fv-photo-input').forEach(input =>
        input.addEventListener('change', async (e) => {
            const fvId = input.dataset.id;
            const files = e.target.files;
            if (!files.length) return;

            for (const file of files) {
                if (file.size > 5 * 1024 * 1024) { toast.error(`${file.name} exceeds 5 MB limit`); return; }
                const fd = new FormData();
                fd.append('file', file);
                fd.append('caption', '');
                const r = await api.upload(`/proposals/${pid}/field-visits/${fvId}/photos`, fd);
                if (!r.success) { toast.error(r.error || `Failed to upload ${file.name}`); return; }
            }
            toast.success(`${files.length} photo(s) uploaded`);
            await renderTab2(c, pid, canEdit);
        })
    );

    // Wire: Drag-and-drop on drop zones
    c.querySelectorAll('.fv-drop-zone').forEach(zone => {
        zone.addEventListener('dragover', e => { e.preventDefault(); zone.classList.add('border-primary', 'bg-primary', 'bg-opacity-10'); });
        zone.addEventListener('dragleave', () => { zone.classList.remove('border-primary', 'bg-primary', 'bg-opacity-10'); });
        zone.addEventListener('drop', async e => {
            e.preventDefault();
            zone.classList.remove('border-primary', 'bg-primary', 'bg-opacity-10');
            const fvId = zone.dataset.id;
            const files = e.dataTransfer.files;
            if (!files.length) return;

            for (const file of files) {
                if (!file.type.startsWith('image/')) { toast.error(`${file.name} is not an image`); continue; }
                if (file.size > 5 * 1024 * 1024) { toast.error(`${file.name} exceeds 5 MB limit`); continue; }
                const fd = new FormData();
                fd.append('file', file);
                fd.append('caption', '');
                const r = await api.upload(`/proposals/${pid}/field-visits/${fvId}/photos`, fd);
                if (!r.success) { toast.error(r.error || `Failed to upload ${file.name}`); return; }
            }
            toast.success('Photo(s) uploaded');
            await renderTab2(c, pid, canEdit);
        });
    });

    // Wire: Delete photo buttons
    c.querySelectorAll('.btn-del-photo').forEach(btn =>
        btn.addEventListener('click', async () => {
            if (!confirm('Delete this photo?')) return;
            const r = await api.delete(`/proposals/${pid}/field-visits/${btn.dataset.fvId}/photos/${btn.dataset.photoId}`);
            if (r.success) { toast.success('Photo deleted'); await renderTab2(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        })
    );
}

// ── Tab 3: Estimate ──────────────────────────────────────────
async function renderTab3(c, pid, canEdit) {
    const res = await api.get(`/proposals/${pid}/estimate`);
    const est = res.success ? res.data : null;

    if (!est && canEdit) {
        c.innerHTML = `<h6 class="mb-3"><i class="bi bi-calculator me-1"></i>Estimate</h6>
        <form id="est-form"><div class="row g-3">
            <div class="col-md-6"><label for="estCost" class="form-label">Estimated Cost (₹)</label><input type="number" class="form-control" id="estCost" step="0.01" min="0" required></div>
        </div><button type="submit" class="btn btn-primary btn-sm mt-3"><i class="bi bi-floppy me-1"></i>Save Estimate</button></form>`;
        document.getElementById('est-form').addEventListener('submit', async e => {
            e.preventDefault();
            const r = await api.post(`/proposals/${pid}/estimate`, { proposalId: pid, estimatedCost: parseFloat(document.getElementById('estCost').value) });
            if (r.success) { toast.success('Estimate saved'); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        });
        return;
    }
    if (!est) { c.innerHTML = '<p class="text-muted">No estimate created yet.</p>'; return; }

    const sBg = est.status === 'Approved' ? 'success' : est.status === 'SentForApproval' ? 'warning text-dark' : 'secondary';
    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-calculator me-1"></i>Estimate</h6>
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Estimated Cost</label><div class="fw-medium fs-5">${formatCurrency(est.estimatedCost)}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Status</label><div><span class="badge bg-${sBg}">${est.status}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Prepared By</label><div>${escapeHtml(est.preparedByName || '—')}</div></div>
            ${est.sentToName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Sent To</label><div>${escapeHtml(est.sentToName)} (${est.sentToRole})</div></div>` : ''}
            ${est.approvedByName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Approved By</label><div>${escapeHtml(est.approvedByName)}</div></div>` : ''}
            ${est.approvedAt ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Approved At</label><div>${formatDate(est.approvedAt)}</div></div>` : ''}
            ${est.approverOpinion_En ? `<div class="col-12"><label class="form-label text-muted small mb-0">Opinion</label><div>${escapeHtml(est.approverOpinion_En)}</div></div>` : ''}
            ${est.returnQueryNote_En ? `<div class="col-12"><label class="form-label text-muted small mb-0 text-danger">Return Query</label><div class="text-danger">${escapeHtml(est.returnQueryNote_En)}</div></div>` : ''}
        </div>
        ${canEdit && est.status === 'Draft' ? `<div class="mt-3"><button class="btn btn-outline-primary btn-sm" id="btn-send-est"><i class="bi bi-send me-1"></i>Send for Approval</button></div>` : ''}
        ${canEdit && est.status === 'SentForApproval' ? `<div class="mt-3">
            <button class="btn btn-success btn-sm" id="btn-approve-est"><i class="bi bi-check-lg me-1"></i>Approve</button>
            <button class="btn btn-warning btn-sm ms-2" id="btn-return-est"><i class="bi bi-arrow-return-left me-1"></i>Return</button>
        </div>` : ''}`;

    document.getElementById('btn-send-est')?.addEventListener('click', async () => {
        const r = await api.post(`/proposals/${pid}/estimate/${est.id}/send-for-approval`, { targetRole: 'CityEngineer' });
        if (r.success) { toast.success('Sent for approval'); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Failed');
    });
    document.getElementById('btn-approve-est')?.addEventListener('click', async () => {
        const r = await api.post(`/proposals/${pid}/estimate/${est.id}/approve`, { estimateId: est.id, disclaimerAccepted: true, opinion_En: '' });
        if (r.success) { toast.success('Estimate approved'); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Failed');
    });
    document.getElementById('btn-return-est')?.addEventListener('click', async () => {
        const note = prompt('Enter return query note:');
        if (!note) return;
        const r = await api.post(`/proposals/${pid}/estimate/${est.id}/return`, { queryNote_En: note });
        if (r.success) { toast.success('Estimate returned'); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Failed');
    });
}

// ── Tab 4: Technical Sanction ────────────────────────────────
async function renderTab4(c, pid, canEdit) {
    const res = await api.get(`/proposals/${pid}/technical-sanction`);
    const ts = res.success ? res.data : null;

    if (!ts && canEdit) {
        c.innerHTML = `<h6 class="mb-3"><i class="bi bi-shield-check me-1"></i>Technical Sanction</h6>
        <form id="ts-form"><div class="row g-3">
            <div class="col-md-4"><label for="tsNum" class="form-label">TS Number</label><input type="text" class="form-control" id="tsNum"></div>
            <div class="col-md-4"><label for="tsDate" class="form-label">TS Date</label><input type="date" class="form-control" id="tsDate"></div>
            <div class="col-md-4"><label for="tsAmt" class="form-label">TS Amount (₹)</label><input type="number" class="form-control" id="tsAmt" step="0.01"></div>
            <div class="col-md-6"><label for="tsDesc" class="form-label">Description (English)</label><textarea class="form-control" id="tsDesc" rows="3"></textarea></div>
            <div class="col-md-6"><label for="tsDescMr" class="form-label">वर्णन (मराठी)</label><textarea class="form-control" id="tsDescMr" rows="3" lang="mr"></textarea></div>
            <div class="col-md-4"><label for="tsSanctBy" class="form-label">Sanctioned By</label><input type="text" class="form-control" id="tsSanctBy"></div>
            <div class="col-md-4"><label for="tsSanctDept" class="form-label">Dept</label><input type="text" class="form-control" id="tsSanctDept"></div>
            <div class="col-md-4"><label for="tsSanctDesig" class="form-label">Designation</label><input type="text" class="form-control" id="tsSanctDesig"></div>
        </div><button type="submit" class="btn btn-primary btn-sm mt-3"><i class="bi bi-floppy me-1"></i>Save</button></form>`;
        document.getElementById('ts-form').addEventListener('submit', async e => {
            e.preventDefault();
            const body = { proposalId: pid,
                tsNumber: document.getElementById('tsNum').value || null,
                tsDate: document.getElementById('tsDate').value || null,
                tsAmount: parseFloat(document.getElementById('tsAmt').value) || null,
                description_En: document.getElementById('tsDesc').value || null,
                description_Mr: document.getElementById('tsDescMr').value || null,
                sanctionedByName: document.getElementById('tsSanctBy').value || null,
                sanctionedByDept: document.getElementById('tsSanctDept').value || null,
                sanctionedByDesignation: document.getElementById('tsSanctDesig').value || null
            };
            const r = await api.post(`/proposals/${pid}/technical-sanction`, body);
            if (r.success) { toast.success('Saved'); await renderTab4(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        });
        return;
    }
    if (!ts) { c.innerHTML = '<p class="text-muted">No technical sanction record yet.</p>'; return; }

    const sBg = ts.status === 'Signed' ? 'success' : 'secondary';
    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-shield-check me-1"></i>Technical Sanction</h6>
        <div class="row g-3">
            <div class="col-md-3"><label class="form-label text-muted small mb-0">TS Number</label><div>${escapeHtml(ts.tsNumber || '—')}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">TS Date</label><div>${formatDate(ts.tsDate)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">TS Amount</label><div class="fw-medium">${formatCurrency(ts.tsAmount)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">Status</label><div><span class="badge bg-${sBg}">${ts.status}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Sanctioned By</label><div>${escapeHtml(ts.sanctionedByName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Dept</label><div>${escapeHtml(ts.sanctionedByDept || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Designation</label><div>${escapeHtml(ts.sanctionedByDesignation || '—')}</div></div>
            ${ts.description_En ? `<div class="col-12"><label class="form-label text-muted small mb-0">Description</label><div>${escapeHtml(ts.description_En)}</div></div>` : ''}
            ${ts.preparedByName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Prepared By</label><div>${escapeHtml(ts.preparedByName)}</div></div>` : ''}
            ${ts.signedByName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Signed By</label><div>${escapeHtml(ts.signedByName)}</div></div>` : ''}
            ${ts.signedAt ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">Signed At</label><div>${formatDate(ts.signedAt)}</div></div>` : ''}
        </div>
        ${canEdit && ts.status === 'Draft' ? `<div class="mt-3"><button class="btn btn-success btn-sm" id="btn-sign-ts"><i class="bi bi-pen me-1"></i>Sign</button></div>` : ''}`;

    document.getElementById('btn-sign-ts')?.addEventListener('click', async () => {
        const r = await api.post(`/proposals/${pid}/technical-sanction/${ts.id}/sign`);
        if (r.success) { toast.success('Signed'); await renderTab4(c, pid, canEdit); } else toast.error(r.error || 'Failed');
    });
}

// ── Tab 5: PRAMA ─────────────────────────────────────────────
async function renderTab5(c, pid, canEdit) {
    const user = getUser();
    const [pramaRes, ftRes, bhRes] = await Promise.all([
        api.get(`/proposals/${pid}/prama`),
        api.get(`/masters/fund-types?palikaId=${user.palikaId}`),
        api.get(`/masters/budget-heads?palikaId=${user.palikaId}`)
    ]);
    const pd = pramaRes.success ? pramaRes.data : null;
    const fts = ftRes.success ? ftRes.data : [];
    const bhs = bhRes.success ? bhRes.data : [];

    if (canEdit) {
        c.innerHTML = `<h6 class="mb-3"><i class="bi bi-receipt me-1"></i>PRAMA (प्रमा) Details</h6>
        <form id="prama-form"><div class="row g-3">
            <div class="col-md-4"><label for="pFT" class="form-label">Fund Type</label><select class="form-select" id="pFT"><option value="">Select</option>${fts.map(f => `<option value="${f.id}" ${pd?.fundTypeId === f.id ? 'selected' : ''}>${escapeHtml(f.name_En)}</option>`).join('')}</select></div>
            <div class="col-md-4"><label for="pBH" class="form-label">Budget Head</label><select class="form-select" id="pBH"><option value="">Select</option>${bhs.map(b => `<option value="${b.id}" ${pd?.budgetHeadId === b.id ? 'selected' : ''}>${escapeHtml(b.name_En)}</option>`).join('')}</select></div>
            <div class="col-md-4"><label for="pYear" class="form-label">Fund Year</label><input type="text" class="form-control" id="pYear" value="${escapeHtml(pd?.fundApprovalYear || '')}" placeholder="2025-26"></div>
            <div class="col-md-6"><label for="pDeptEn" class="form-label">Dept User (English)</label><input type="text" class="form-control" id="pDeptEn" value="${escapeHtml(pd?.deptUserName_En || '')}"></div>
            <div class="col-md-6"><label for="pDeptMr" class="form-label">विभाग वापरकर्ता (मराठी)</label><input type="text" class="form-control" id="pDeptMr" lang="mr" value="${escapeHtml(pd?.deptUserName_Mr || '')}"></div>
            <div class="col-md-6"><label for="pRefEn" class="form-label">References (English)</label><textarea class="form-control" id="pRefEn" rows="2">${escapeHtml(pd?.references_En || '')}</textarea></div>
            <div class="col-md-6"><label for="pRefMr" class="form-label">संदर्भ (मराठी)</label><textarea class="form-control" id="pRefMr" rows="2" lang="mr">${escapeHtml(pd?.references_Mr || '')}</textarea></div>
            <div class="col-md-6"><label for="pAddEn" class="form-label">Additional (English)</label><textarea class="form-control" id="pAddEn" rows="2">${escapeHtml(pd?.additionalDetails_En || '')}</textarea></div>
            <div class="col-md-6"><label for="pAddMr" class="form-label">अतिरिक्त (मराठी)</label><textarea class="form-control" id="pAddMr" rows="2" lang="mr">${escapeHtml(pd?.additionalDetails_Mr || '')}</textarea></div>
        </div><button type="submit" class="btn btn-primary btn-sm mt-3"><i class="bi bi-floppy me-1"></i>Save</button></form>`;

        document.getElementById('prama-form').addEventListener('submit', async e => {
            e.preventDefault();
            const body = { proposalId: pid,
                fundTypeId: document.getElementById('pFT').value || null,
                budgetHeadId: document.getElementById('pBH').value || null,
                fundApprovalYear: document.getElementById('pYear').value || null,
                deptUserName_En: document.getElementById('pDeptEn').value || null,
                deptUserName_Mr: document.getElementById('pDeptMr').value || null,
                references_En: document.getElementById('pRefEn').value || null,
                references_Mr: document.getElementById('pRefMr').value || null,
                additionalDetails_En: document.getElementById('pAddEn').value || null,
                additionalDetails_Mr: document.getElementById('pAddMr').value || null };
            const r = await api.post(`/proposals/${pid}/prama`, body);
            if (r.success) toast.success('PRAMA saved'); else toast.error(r.error || 'Failed');
        });
        return;
    }
    if (!pd) { c.innerHTML = '<p class="text-muted">No PRAMA details yet.</p>'; return; }

    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-receipt me-1"></i>PRAMA Details</h6>
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Fund Type</label><div>${escapeHtml(pd.fundTypeName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Budget Head</label><div>${escapeHtml(pd.budgetHeadName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Fund Year</label><div>${escapeHtml(pd.fundApprovalYear || '—')}</div></div>
            <div class="col-md-6"><label class="form-label text-muted small mb-0">Dept User</label><div>${escapeHtml(pd.deptUserName_En || '—')}</div></div>
            ${pd.references_En ? `<div class="col-12"><label class="form-label text-muted small mb-0">References</label><div>${escapeHtml(pd.references_En)}</div></div>` : ''}
            ${pd.additionalDetails_En ? `<div class="col-12"><label class="form-label text-muted small mb-0">Additional</label><div>${escapeHtml(pd.additionalDetails_En)}</div></div>` : ''}
        </div>`;
}

// ── Tab 6: Budget ────────────────────────────────────────────
async function renderTab6(c, pid, canEdit) {
    const user = getUser();
    const [budgetRes, wemRes, bhRes] = await Promise.all([
        api.get(`/proposals/${pid}/budget`),
        api.get(`/masters/work-execution-methods?palikaId=${user.palikaId}`),
        api.get(`/masters/budget-heads?palikaId=${user.palikaId}`)
    ]);
    const bd = budgetRes.success ? budgetRes.data : null;
    const wms = wemRes.success ? wemRes.data : [];
    const bhs = bhRes.success ? bhRes.data : [];

    if (canEdit) {
        c.innerHTML = `<h6 class="mb-3"><i class="bi bi-wallet2 me-1"></i>Budget & Compliance</h6>
        <form id="budget-form"><div class="row g-3">
            <div class="col-md-4"><label for="bWM" class="form-label">Work Execution Method</label><select class="form-select" id="bWM"><option value="">Select</option>${wms.map(w => `<option value="${w.id}" ${bd?.workExecutionMethodId === w.id ? 'selected' : ''}>${escapeHtml(w.name_En)}</option>`).join('')}</select></div>
            <div class="col-md-4"><label for="bDays" class="form-label">Duration (Days)</label><input type="number" class="form-control" id="bDays" value="${bd?.workDurationDays || ''}"></div>
            <div class="col-md-4"><label for="bBH" class="form-label">Budget Head</label><select class="form-select" id="bBH"><option value="">Select</option>${bhs.map(b => `<option value="${b.id}" ${bd?.budgetHeadId === b.id ? 'selected' : ''}>${escapeHtml(b.name_En)}</option>`).join('')}</select></div>
            <div class="col-md-3"><label for="bAlloc" class="form-label">Allocated (₹)</label><input type="number" class="form-control" id="bAlloc" step="0.01" value="${bd?.allocatedFund || ''}"></div>
            <div class="col-md-3"><label for="bAvail" class="form-label">Available (₹)</label><input type="number" class="form-control" id="bAvail" step="0.01" value="${bd?.currentAvailableFund || ''}"></div>
            <div class="col-md-3"><label for="bOldExp" class="form-label">Old Expenditure (₹)</label><input type="number" class="form-control" id="bOldExp" step="0.01" value="${bd?.oldExpenditure || ''}"></div>
            <div class="col-md-3"><label for="bEstCost" class="form-label">Estimated Cost (₹)</label><input type="number" class="form-control" id="bEstCost" step="0.01" value="${bd?.estimatedCost || ''}"></div>
            <div class="col-md-4"><label for="bSerial" class="form-label">Account Serial No</label><input type="text" class="form-control" id="bSerial" value="${escapeHtml(bd?.accountSerialNo || '')}"></div>
            <div class="col-md-4 d-flex align-items-end"><div class="form-check"><input class="form-check-input" type="checkbox" id="bTender" ${bd?.tenderVerificationDone ? 'checked' : ''}><label class="form-check-label" for="bTender">Tender Verified</label></div></div>
            <div class="col-md-6"><label for="bComp" class="form-label">Compliance (English)</label><textarea class="form-control" id="bComp" rows="2">${escapeHtml(bd?.complianceNotes_En || '')}</textarea></div>
            <div class="col-md-6"><label for="bCompMr" class="form-label">अनुपालन (मराठी)</label><textarea class="form-control" id="bCompMr" rows="2" lang="mr">${escapeHtml(bd?.complianceNotes_Mr || '')}</textarea></div>
        </div><button type="submit" class="btn btn-primary btn-sm mt-3"><i class="bi bi-floppy me-1"></i>Save Budget</button></form>`;

        document.getElementById('budget-form').addEventListener('submit', async e => {
            e.preventDefault();
            const body = { proposalId: pid,
                workExecutionMethodId: document.getElementById('bWM').value || null,
                workDurationDays: parseInt(document.getElementById('bDays').value) || null,
                tenderVerificationDone: document.getElementById('bTender').checked,
                budgetHeadId: document.getElementById('bBH').value || null,
                allocatedFund: parseFloat(document.getElementById('bAlloc').value) || null,
                currentAvailableFund: parseFloat(document.getElementById('bAvail').value) || null,
                oldExpenditure: parseFloat(document.getElementById('bOldExp').value) || null,
                estimatedCost: parseFloat(document.getElementById('bEstCost').value) || null,
                accountSerialNo: document.getElementById('bSerial').value || null,
                complianceNotes_En: document.getElementById('bComp').value || null,
                complianceNotes_Mr: document.getElementById('bCompMr').value || null };
            const r = await api.post(`/proposals/${pid}/budget`, body);
            if (r.success) { toast.success('Budget saved'); await renderTab6(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        });
        return;
    }
    if (!bd) { c.innerHTML = '<p class="text-muted">No budget details yet.</p>'; return; }

    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-wallet2 me-1"></i>Budget & Compliance</h6>
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Work Method</label><div>${escapeHtml(bd.workExecutionMethodName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Duration</label><div>${bd.workDurationDays || '—'} days</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Budget Head</label><div>${escapeHtml(bd.budgetHeadName || '—')}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">Allocated</label><div>${formatCurrency(bd.allocatedFund)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">Available</label><div>${formatCurrency(bd.currentAvailableFund)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">Old Exp.</label><div>${formatCurrency(bd.oldExpenditure)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">Est. Cost</label><div class="fw-medium">${formatCurrency(bd.estimatedCost)}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Balance</label><div class="fw-medium ${(bd.balanceAmount || 0) < 0 ? 'text-danger' : 'text-success'}">${formatCurrency(bd.balanceAmount)}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Approval Slab</label><div><span class="badge bg-info">${bd.determinedApprovalSlab || '—'}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Final Authority</label><div><span class="badge bg-dark">${bd.finalAuthorityRole || '—'}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">Tender Verified</label><div>${bd.tenderVerificationDone ? '<i class="bi bi-check-circle-fill text-success"></i> Yes' : '<i class="bi bi-x-circle text-danger"></i> No'}</div></div>
            ${bd.complianceNotes_En ? `<div class="col-12"><label class="form-label text-muted small mb-0">Compliance Notes</label><div>${escapeHtml(bd.complianceNotes_En)}</div></div>` : ''}
        </div>`;
}

// ── Timeline ─────────────────────────────────────────────────
async function renderTimeline(c, pid) {
    const res = await api.get(`/proposals/${pid}/approval-history`);
    if (!res.success || !res.data || res.data.length === 0) {
        c.innerHTML = '<p class="text-muted text-center py-3">No approval history yet.</p>';
        return;
    }
    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-clock-history me-1"></i>Approval Timeline</h6>
        <div class="timeline">${res.data.map(h => `
            <div class="timeline-item action-${h.action}">
                <div class="d-flex justify-content-between align-items-start">
                    <div><span class="badge bg-${h.action === 'Approve' ? 'success' : h.action === 'PushBack' ? 'danger' : 'primary'} me-1">${h.action}</span>
                        <strong>${escapeHtml(h.actorName_En || '—')}</strong> <span class="text-muted">(${h.stageRole})</span></div>
                    <small class="text-muted">${formatDate(h.createdAt)}</small>
                </div>
                ${h.opinion_En ? `<div class="mt-1 small">${escapeHtml(h.opinion_En)}</div>` : ''}
                ${h.pushBackNote_En ? `<div class="mt-1 small text-danger">${escapeHtml(h.pushBackNote_En)}</div>` : ''}
            </div>`).join('')}
        </div>`;
}

// ── Documents Tab ────────────────────────────────────────────
async function renderDocuments(c, pid, canEdit) {
    const res = await api.get(`/proposals/${pid}/documents`);
    const docs = res.success ? (res.data || []) : [];

    let html = `<div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h6 class="mb-0"><i class="bi bi-paperclip me-1"></i>Documents (${docs.length})</h6>
        ${canEdit ? `<form id="upload-form" class="d-flex gap-2 align-items-center flex-wrap">
            <select class="form-select form-select-sm" id="uploadDocType" style="width:auto;" required>
                <option value="SupportingDoc">Supporting Doc</option><option value="LocationMap">Location Map</option>
                <option value="SitePhoto">Site Photo</option><option value="EstimateCopy">Estimate Copy</option>
                <option value="TechnicalSanctionDoc">TS Doc</option><option value="Other">Other</option>
            </select>
            <input type="file" class="form-control form-control-sm" id="uploadFile" required style="max-width:250px;">
            <button type="submit" class="btn btn-primary btn-sm"><i class="bi bi-upload me-1"></i>Upload</button>
        </form>` : ''}
    </div>`;

    if (docs.length > 0) {
        html += `<div class="list-group">${docs.map(d => `
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <div><i class="bi bi-file-earmark me-2"></i>${escapeHtml(d.docName || d.fileName)}<small class="text-muted ms-2">${(d.fileSize / 1024).toFixed(0)} KB · Tab ${d.tabNumber}</small></div>
                <div class="d-flex gap-2 align-items-center"><span class="badge bg-light text-dark">${d.documentType}</span>
                    ${canEdit ? `<button class="btn btn-outline-danger btn-sm btn-del-doc" data-id="${d.id}"><i class="bi bi-trash"></i></button>` : ''}
                </div>
            </div>`).join('')}</div>`;
    } else {
        html += '<p class="text-muted">No documents uploaded yet.</p>';
    }
    c.innerHTML = html;

    document.getElementById('upload-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const file = document.getElementById('uploadFile').files[0];
        if (!file) return;
        const fd = new FormData();
        fd.append('file', file);
        fd.append('tabNumber', currentTab === 'docs' ? '1' : currentTab);
        fd.append('documentType', document.getElementById('uploadDocType').value);
        fd.append('docName', file.name);
        const r = await api.upload(`/proposals/${pid}/documents/upload`, fd);
        if (r.success) { toast.success('Uploaded'); await renderDocuments(c, pid, canEdit); } else toast.error(r.error || 'Upload failed');
    });

    c.querySelectorAll('.btn-del-doc').forEach(btn =>
        btn.addEventListener('click', async () => {
            if (!confirm('Delete this document?')) return;
            const r = await api.delete(`/proposals/${pid}/documents/${btn.dataset.id}`);
            if (r.success) { toast.success('Deleted'); await renderDocuments(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        })
    );
}

// ── Workflow button wiring ───────────────────────────────────
function wireWorkflowButtons(id, params) {
    document.getElementById('btn-submit')?.addEventListener('click', async () => {
        // Pre-check: field visit with photos required
        const fvRes = await api.get(`/proposals/${id}/field-visits`);
        const visits = fvRes.success ? (fvRes.data || []) : [];
        const hasCompletedWithPhotos = visits.some(fv => fv.status === 'Completed' && fv.photos?.length > 0);
        if (!hasCompletedWithPhotos) {
            toast.error('At least one completed field visit with photos is required before submission. Go to the Field Visit tab to upload site photos.');
            return;
        }
        if (!confirm('Submit this proposal for approval?')) return;
        const r = await api.post(`/workflow/${id}/submit`);
        if (r.success) { toast.success('Submitted'); renderProposalDetailPage(params); } else toast.error(r.error || 'Failed');
    });

    document.getElementById('btn-approve')?.addEventListener('click', () => new bootstrap.Modal(document.getElementById('approveModal')).show());
    document.getElementById('approve-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const r = await api.post(`/workflow/${id}/approve`, {
            proposalId: id, opinion_En: document.getElementById('opinion').value.trim() || null,
            disclaimerAccepted: document.getElementById('disclaimer-check').checked });
        bootstrap.Modal.getInstance(document.getElementById('approveModal'))?.hide();
        if (r.success) { toast.success('Approved'); renderProposalDetailPage(params); } else toast.error(r.error || 'Failed');
    });

    document.getElementById('btn-pushback')?.addEventListener('click', () => new bootstrap.Modal(document.getElementById('pushbackModal')).show());
    document.getElementById('pushback-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const note = document.getElementById('pushback-note').value.trim();
        if (!note) { toast.error('Reason is required'); return; }
        const r = await api.post(`/workflow/${id}/pushback`, { proposalId: id, pushBackNote_En: note });
        bootstrap.Modal.getInstance(document.getElementById('pushbackModal'))?.hide();
        if (r.success) { toast.success('Pushed back'); renderProposalDetailPage(params); } else toast.error(r.error || 'Failed');
    });
}
