// Proposal detail page — full 6-tab view with approval timeline
import { api } from '../api.js';
import { getUser, hasRole } from '../auth.js';
import { toast } from '../toast.js';
import { stageBadge, formatDate, formatCurrency, escapeHtml } from '../utils.js';
import { bilingualDisplay, createDualLangInput } from '../dual-lang-input.js';
import { t, tBilingual, onLangChange, translatePage } from '../i18n.js';

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
                    <i class="bi bi-arrow-left me-1"></i><span data-i18n="proposal.detail.back">Back</span>
                </a>
                <h4 class="mb-1 mt-1">${escapeHtml(p.proposalNumber)}</h4>
                <p class="text-muted mb-0" style="font-size:0.85rem;">
                    ${stageBadge(p.currentStage)}
                    <span class="ms-2">${t('proposal.detail.createdBy')} ${escapeHtml(p.createdByName)} ${t('proposal.detail.on')} ${formatDate(p.createdAt)}</span>
                    <span class="ms-2 badge bg-light text-dark">Tab ${p.completedTab || 1}/6</span>
                </p>
            </div>
            <div class="d-flex gap-2 flex-wrap">
                ${canEdit ? `<a href="#/proposals/${id}/edit" class="btn btn-primary btn-sm"><i class="bi bi-pencil me-1"></i><span data-i18n="proposal.detail.editTab1">Edit Tab 1</span></a>` : ''}
                ${canApprove ? `
                    <button class="btn btn-success btn-sm" id="btn-approve"><i class="bi bi-check-lg me-1"></i><span data-i18n="workflow.approve">Approve</span></button>
                    <button class="btn btn-warning btn-sm" id="btn-pushback"><i class="bi bi-arrow-return-left me-1"></i><span data-i18n="workflow.pushBack">Push Back</span></button>
                ` : ''}
                ${isCreator && (p.currentStage === 'Draft' || p.currentStage === 'PushedBack') ? `
                    <button class="btn btn-outline-primary btn-sm" id="btn-submit"><i class="bi bi-send me-1"></i><span data-i18n="workflow.submit">Submit</span></button>
                ` : ''}
            </div>
        </div>

        <!-- Tab Navigation -->
        <ul class="nav nav-tabs proposal-tabs mb-0" role="tablist">
            <li class="nav-item"><button class="nav-link active" data-tab="1"><i class="bi bi-file-text me-1"></i><span data-i18n="proposal.detail.info">Proposal</span></button></li>
            <li class="nav-item"><button class="nav-link" data-tab="2"><i class="bi bi-geo-alt me-1"></i><span data-i18n="proposal.detail.fieldVisit">Field Visit</span></button></li>
            <li class="nav-item"><button class="nav-link" data-tab="3"><i class="bi bi-calculator me-1"></i><span data-i18n="proposal.detail.estimate">Estimate</span></button></li>
            <li class="nav-item"><button class="nav-link" data-tab="4"><i class="bi bi-shield-check me-1"></i><span data-i18n="proposal.detail.techSanction">Tech Sanction</span></button></li>
            <li class="nav-item"><button class="nav-link" data-tab="5"><i class="bi bi-receipt me-1"></i><span data-i18n="proposal.detail.prama">PRAMA</span></button></li>
            <li class="nav-item"><button class="nav-link" data-tab="6"><i class="bi bi-wallet2 me-1"></i><span data-i18n="proposal.detail.budget">Budget</span></button></li>
            <li class="nav-item"><button class="nav-link" data-tab="timeline"><i class="bi bi-clock-history me-1"></i><span data-i18n="proposal.detail.timeline">Timeline</span></button></li>
            <li class="nav-item"><button class="nav-link" data-tab="docs"><i class="bi bi-paperclip me-1"></i><span data-i18n="proposal.detail.documents">Additional Docs</span></button></li>
        </ul>

        <div class="card border-top-0 rounded-0 rounded-bottom">
            <div class="card-body" id="tab-content"></div>
        </div>

        <!-- Approve Modal -->
        <div class="modal fade" id="approveModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg"><div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">${tBilingual('workflow.approveTitle')}</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
                <form id="approve-form"><div class="modal-body">
                    <div class="mb-3"><label for="opinion" class="form-label">${t('workflow.opinion')} (English)</label><textarea class="form-control" id="opinion" rows="2" placeholder="${t('workflow.opinionPlaceholder')}"></textarea></div>
                    <div class="mb-3"><label for="opinion-mr" class="form-label">${t('workflow.opinion', 'mr')} (मराठी)</label><textarea class="form-control" id="opinion-mr" rows="2" lang="mr" placeholder="${t('workflow.opinionPlaceholder', 'mr')}"></textarea></div>
                    <div class="mb-3"><label for="approve-sig-file" class="form-label">${t('workflow.signatureUpload')}</label><input type="file" class="form-control form-control-sm" id="approve-sig-file" accept="image/png,image/jpeg,image/svg+xml"><div class="form-text">${t('workflow.signatureHint')}</div></div>
                    <div class="alert alert-info py-2 small" id="role-disclaimer-text" lang="mr">${t('workflow.disclaimer.' + user.role) || t('workflow.disclaimer')}</div>
                    <div class="form-check"><input class="form-check-input" type="checkbox" id="disclaimer-check" required><label class="form-check-label" for="disclaimer-check">${t('workflow.disclaimerAccept')}</label></div>
                </div><div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal" data-i18n="common.cancel">Cancel</button><button type="submit" class="btn btn-success" id="btn-confirm-approve" data-i18n="workflow.confirmApprove">Confirm Approve</button></div></form>
            </div></div>
        </div>

        <!-- PushBack Modal -->
        <div class="modal fade" id="pushbackModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog"><div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">${tBilingual('workflow.pushBackTitle')}</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
                <form id="pushback-form"><div class="modal-body">
                    <div class="mb-3"><label for="pushback-note" class="form-label">${t('workflow.reason')} (English) <span class="text-danger">*</span></label><textarea class="form-control" id="pushback-note" rows="2" required></textarea></div>
                    <div class="mb-3"><label for="pushback-note-mr" class="form-label">${t('workflow.reason', 'mr')} (मराठी)</label><textarea class="form-control" id="pushback-note-mr" rows="2" lang="mr"></textarea></div>
                </div><div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal" data-i18n="common.cancel">Cancel</button><button type="submit" class="btn btn-warning" data-i18n="workflow.confirmPushBack">Confirm Push Back</button></div></form>
            </div></div>
        </div>`;

    // Wire tab navigation
    translatePage(content);
    const _unsub = onLangChange(() => translatePage(content));
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
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('proposal.detail.department')}</label><div class="fw-medium">${escapeHtml(p.departmentName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('proposal.detail.workCategory')}</label><div class="fw-medium">${escapeHtml(p.workCategoryName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('proposal.detail.priority')}</label><div><span class="badge bg-${p.priority === 'High' ? 'danger' : p.priority === 'Low' ? 'secondary' : 'warning text-dark'}">${p.priority}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('proposal.detail.zone')}</label><div>${escapeHtml(p.zoneName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('proposal.detail.prabhag')}</label><div>${escapeHtml(p.prabhagName || '—')}</div></div>
            ${bilingualDisplay('Area', p.area, p.area_Mr, 'col-md-4', t('proposal.detail.area', 'mr'))}
            ${bilingualDisplay('Work Title', p.workTitle_En, p.workTitle_Mr, 'col-12', t('proposal.detail.workTitle', 'mr'))}
            ${bilingualDisplay('Work Description', p.workDescription_En, p.workDescription_Mr, 'col-12', t('proposal.detail.workDescription', 'mr'))}
            ${bilingualDisplay('Location Address', p.locationAddress_En, p.locationAddress_Mr, 'col-12', t('proposal.detail.locationAddress', 'mr'))}
            ${p.requestSourceName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('proposal.detail.requestSource')}</label><div>${escapeHtml(p.requestSourceName)}</div></div>` : ''}
            ${p.requestorName || p.requestorName_Mr ? bilingualDisplay('Requestor Name', p.requestorName, p.requestorName_Mr, 'col-md-6', t('proposal.detail.requestorName', 'mr')) : ''}
            ${p.requestorMobile ? `<div class="col-md-6"><label class="form-label text-muted small mb-0">${tBilingual('proposal.detail.requestorMobile')}</label><div>${escapeHtml(p.requestorMobile)}</div></div>` : ''}
            ${p.requestorAddress || p.requestorAddress_Mr ? bilingualDisplay('Requestor Address', p.requestorAddress, p.requestorAddress_Mr, 'col-12', t('proposal.detail.requestorAddress', 'mr')) : ''}
            ${p.requestorDesignation || p.requestorDesignation_Mr ? bilingualDisplay('Requestor Designation', p.requestorDesignation, p.requestorDesignation_Mr, 'col-md-6', t('proposal.detail.requestorDesignation', 'mr')) : ''}
            ${p.requestorOrganisation || p.requestorOrganisation_Mr ? bilingualDisplay('Requestor Organisation', p.requestorOrganisation, p.requestorOrganisation_Mr, 'col-md-6', t('proposal.detail.requestorOrganisation', 'mr')) : ''}
        </div>`;
}

// ── Tab 2: Field Visits ──────────────────────────────────────
async function renderTab2(c, pid, canEdit) {
    const res = await api.get(`/proposals/${pid}/field-visits`);
    const visits = res.success ? (res.data || []) : [];
    const totalPhotos = visits.reduce((sum, fv) => sum + (fv.photos?.length || 0), 0);
    const hasCompleted = visits.some(fv => fv.status === 'Completed' && fv.photos?.length > 0);

    // Load site conditions for dropdown
    let siteConditions = [];
    const scRes = await api.get('/masters/site-conditions');
    if (scRes.success && scRes.data) siteConditions = scRes.data;

    const user = getUser();

    let html = `<div class="d-flex justify-content-between align-items-center mb-3">
        <h6 class="mb-0"><i class="bi bi-geo-alt me-1"></i>${t('fieldVisit.title')} (${visits.length})</h6>
        ${canEdit ? `<button class="btn btn-primary btn-sm" id="btn-assign-fv"><i class="bi bi-plus me-1"></i>${t('fieldVisit.assignVisit')}</button>` : ''}
    </div>`;

    // ── Mandatory photo status banner ──
    if (!hasCompleted) {
        html += `<div class="alert alert-warning d-flex align-items-center py-2 mb-3" role="alert">
            <i class="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
            <div><strong>${t('fieldVisit.mandatoryWarning')}</strong> ${t('fieldVisit.mandatoryDetail')}</div>
        </div>`;
    } else {
        html += `<div class="alert alert-success d-flex align-items-center py-2 mb-3" role="alert">
            <i class="bi bi-check-circle-fill me-2 fs-5"></i>
            <div><strong>${totalPhotos} photo(s) uploaded</strong> across ${visits.filter(fv => fv.status === 'Completed').length} completed visit(s). ${t('fieldVisit.photosSatisfied')}</div>
        </div>`;
    }

    if (visits.length === 0) {
        html += `<p class="text-muted">${t('fieldVisit.noVisits')}</p>`;
    } else {
        visits.forEach(fv => {
            const sBg = fv.status === 'Completed' ? 'success' : fv.status === 'InProgress' ? 'primary' : 'secondary';
            const isCompleted = fv.status === 'Completed';
            const canModify = canEdit && !isCompleted;
            const photoCount = fv.photos?.length || 0;

            html += `<div class="card mb-3 ${isCompleted ? 'border-success' : ''}">
                <div class="card-header bg-light py-2 d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${t('fieldVisit.visit')} #${fv.visitNumber}</strong>
                        <span class="badge bg-${sBg} ms-2">${fv.status}</span>
                        <span class="text-muted ms-2 small">${t('fieldVisit.assigned')}: ${escapeHtml(fv.assignedToName || '—')}</span>
                    </div>
                    <small class="text-muted">${formatDate(fv.createdAt)}</small>
                </div>
                <div class="card-body py-3">`;

            // ── EDITABLE FORM (for non-completed visits when user can modify) ──
            if (canModify) {
                html += `<form class="fv-edit-form" data-fv-id="${fv.id}">

                    <!-- Inspection Detail Section -->
                    <h6 class="fw-bold text-primary border-bottom pb-1 mb-3"><i class="bi bi-clipboard-check me-1"></i>${t('fieldVisit.inspectionDetail')}</h6>

                    <!-- Upload PDF -->
                    <div class="mb-3">
                        <label class="form-label fw-medium">${t('fieldVisit.uploadPdf')}</label>
                        ${fv.uploadedPdfPath ? `
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <a href="${fv.uploadedPdfPath}" target="_blank" class="btn btn-outline-primary btn-sm"><i class="bi bi-file-pdf me-1"></i>${t('fieldVisit.viewPdf')}</a>
                                <span class="text-muted small">${t('fieldVisit.pdfUploaded')}</span>
                            </div>` : ''}
                        <input type="file" class="form-control form-control-sm fv-pdf-input" data-fv-id="${fv.id}" accept="application/pdf">
                        <div class="form-text">${t('fieldVisit.pdfHint')}</div>
                    </div>

                    <div class="row g-3 mb-3">
                        <!-- Inspection By (auto-filled) -->
                        <div class="col-md-6">
                            <label class="form-label fw-medium">${t('fieldVisit.inspectionBy')}</label>
                            <input type="text" class="form-control form-control-sm bg-light" value="${escapeHtml(fv.inspectionByName || user?.fullName_En || '')}" readonly aria-label="Inspection by">
                            <div class="form-text">${t('fieldVisit.inspectionByHint')}</div>
                        </div>
                        <!-- Inspection Date -->
                        <div class="col-md-6">
                            <label class="form-label fw-medium">${t('fieldVisit.inspectionDate')}</label>
                            <input type="date" class="form-control form-control-sm fv-inspection-date" data-fv-id="${fv.id}"
                                value="${fv.inspectionDate ? fv.inspectionDate.substring(0,10) : new Date().toISOString().substring(0,10)}" aria-label="Inspection date">
                        </div>
                    </div>

                    <!-- Site Condition (dropdown from master) -->
                    <div class="mb-3">
                        <label class="form-label fw-medium">${t('fieldVisit.siteCondition')}</label>
                        <select class="form-select form-select-sm fv-site-condition" data-fv-id="${fv.id}" aria-label="Site condition">
                            <option value="">— ${t('fieldVisit.selectSiteCondition')} —</option>
                            ${siteConditions.map(sc => `<option value="${sc.id}" ${fv.siteConditionId === sc.id ? 'selected' : ''}>${escapeHtml(sc.name_En)}${sc.name_Mr ? ' / ' + escapeHtml(sc.name_Mr) : ''}</option>`).join('')}
                        </select>
                    </div>

                    <!-- Problem Description (dual-lang textarea) -->
                    <div class="mb-3" id="fv-problem-${fv.id}"></div>

                    <!-- Measurements (optional, dual-lang textarea) -->
                    <div class="mb-3" id="fv-measurements-${fv.id}"></div>

                    <!-- GPS Location -->
                    <div class="row g-3 mb-3">
                        <div class="col-md-6">
                            <label class="form-label fw-medium"><i class="bi bi-geo-alt me-1"></i>${t('fieldVisit.gpsLat')}</label>
                            <input type="number" step="0.000001" class="form-control form-control-sm fv-gps-lat" data-fv-id="${fv.id}"
                                value="${fv.gpsLatitude || ''}" placeholder="e.g. 20.9010" aria-label="GPS Latitude">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-medium">${t('fieldVisit.gpsLong')}</label>
                            <input type="number" step="0.000001" class="form-control form-control-sm fv-gps-long" data-fv-id="${fv.id}"
                                value="${fv.gpsLongitude || ''}" placeholder="e.g. 74.7749" aria-label="GPS Longitude">
                        </div>
                    </div>

                    <!-- Remark (dual-lang) -->
                    <div class="mb-3" id="fv-remark-${fv.id}"></div>

                    <!-- Site Photos Section -->
                    <div class="border rounded p-3 mb-3" style="background:#f8f9fa;">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <h6 class="mb-0 fw-bold"><i class="bi bi-images me-2 text-primary"></i>${t('fieldVisit.sitePhotos')}
                                <span class="badge ${photoCount > 0 ? 'bg-primary' : 'bg-danger'} ms-1">${photoCount}</span>
                            </h6>
                            <label class="btn btn-primary btn-sm mb-0" role="button" tabindex="0">
                                <i class="bi bi-camera-fill me-1"></i>${t('fieldVisit.uploadPhotos')}
                                <input type="file" class="d-none fv-photo-input" data-id="${fv.id}" accept="image/*" multiple>
                            </label>
                        </div>

                        ${photoCount === 0 ? `
                            <div class="text-center py-4 border border-2 border-dashed rounded bg-white fv-drop-zone" data-id="${fv.id}" style="cursor:pointer;">
                                <i class="bi bi-cloud-arrow-up fs-1 text-muted"></i>
                                <p class="text-muted mb-1 fw-medium">${t('fieldVisit.dragDrop')}</p>
                                <p class="text-muted small mb-0">${t('fieldVisit.dragDropHint')} · JPEG, PNG, WebP · max 5 MB each</p>
                                <p class="text-danger small mt-2 mb-0"><i class="bi bi-exclamation-circle me-1"></i>${t('fieldVisit.photosMandatory')}</p>
                            </div>` : ''}

                        ${photoCount > 0 ? `
                            <div class="d-flex flex-wrap gap-2">
                                ${fv.photos.map(p => `
                                    <div class="position-relative border rounded overflow-hidden shadow-sm" style="width:110px;height:110px;">
                                        <img src="${p.storagePath || p.fileName}" alt="${escapeHtml(p.caption || p.fileName)}"
                                            class="w-100 h-100" style="object-fit:cover;cursor:pointer;"
                                            onclick="window.open(this.src,'_blank')" title="Click to view full size">
                                        <button class="btn btn-danger position-absolute top-0 end-0 p-0 lh-1 btn-del-photo rounded-0"
                                            data-fv-id="${fv.id}" data-photo-id="${p.id}" style="width:22px;height:22px;font-size:11px;opacity:0.85;" title="Delete photo">
                                            <i class="bi bi-x-lg"></i></button>
                                    </div>`).join('')}
                                <label class="border border-2 border-dashed rounded d-flex align-items-center justify-content-center bg-white shadow-sm fv-drop-zone"  
                                    data-id="${fv.id}" style="width:110px;height:110px;cursor:pointer;" role="button" tabindex="0" title="Add more photos">
                                    <div class="text-center text-muted">
                                        <i class="bi bi-plus-lg fs-4"></i><br><small>${t('fieldVisit.addMore')}</small>
                                    </div>
                                    <input type="file" class="d-none fv-photo-input" data-id="${fv.id}" accept="image/*" multiple>
                                </label>
                            </div>` : ''}
                    </div>

                    <!-- Recommendation (dual-lang) -->
                    <div class="mb-3" id="fv-recommendation-${fv.id}"></div>

                    <!-- Signature -->
                    <div class="mb-3">
                        <label class="form-label fw-medium"><i class="bi bi-pen me-1"></i>${t('fieldVisit.signature')}</label>
                        ${fv.signaturePath ? `
                            <div class="mb-2">
                                <img src="${fv.signaturePath}" alt="Signature" class="border rounded" style="max-height:60px;">
                            </div>` : ''}
                        <input type="file" class="form-control form-control-sm fv-signature-input" data-fv-id="${fv.id}" accept="image/png,image/jpeg">
                        <div class="form-text">${t('fieldVisit.signatureHint')}</div>
                    </div>

                    <!-- Action Buttons -->
                    <div class="d-flex gap-2 mt-3 pt-2 border-top">
                        <button type="submit" class="btn btn-primary btn-sm"><i class="bi bi-floppy me-1"></i>${t('fieldVisit.saveVisit')}</button>
                        <button type="button" class="btn btn-success btn-sm btn-complete-fv" data-id="${fv.id}"><i class="bi bi-check-circle me-1"></i>${t('fieldVisit.signAndComplete')}</button>
                    </div>
                </form>`;

            } else {
                // ── READ-ONLY VIEW (completed visits or user can't edit) ──
                if (fv.uploadedPdfPath) {
                    html += `<div class="small mb-2"><i class="bi bi-file-pdf text-danger me-1"></i><a href="${fv.uploadedPdfPath}" target="_blank">${t('fieldVisit.viewPdf')}</a></div>`;
                }
                if (fv.inspectionByName || fv.inspectionDate) {
                    html += `<div class="small mb-1"><strong>${t('fieldVisit.inspectionBy')}:</strong> ${escapeHtml(fv.inspectionByName || '—')} <span class="text-muted ms-2">${t('fieldVisit.inspectionDate')}: ${fv.inspectionDate ? formatDate(fv.inspectionDate) : '—'}</span></div>`;
                }
                if (fv.siteConditionName) {
                    html += `<div class="small mb-1"><strong>${t('fieldVisit.siteCondition')}:</strong> ${escapeHtml(fv.siteConditionName)}</div>`;
                }
                if (fv.problemDescription_En || fv.problemDescription_Mr) {
                    html += `<div class="small mb-1"><strong>${t('fieldVisit.problem')}:</strong> ${escapeHtml(fv.problemDescription_En || '')}${fv.problemDescription_Mr ? ` <span class="text-muted" lang="mr">| ${escapeHtml(fv.problemDescription_Mr)}</span>` : ''}</div>`;
                }
                if (fv.measurements_En || fv.measurements_Mr) {
                    html += `<div class="small mb-1"><strong>${t('fieldVisit.measurements')}:</strong> ${escapeHtml(fv.measurements_En || '')}${fv.measurements_Mr ? ` <span class="text-muted" lang="mr">| ${escapeHtml(fv.measurements_Mr)}</span>` : ''}</div>`;
                }
                if (fv.gpsLatitude) {
                    html += `<div class="small mb-2 text-muted"><i class="bi bi-geo-alt me-1"></i>${fv.gpsLatitude}, ${fv.gpsLongitude}</div>`;
                }
                if (fv.remark_En || fv.remark_Mr) {
                    html += `<div class="small mb-1"><strong>${t('fieldVisit.remark')}:</strong> ${escapeHtml(fv.remark_En || '')}${fv.remark_Mr ? ` <span class="text-muted" lang="mr">| ${escapeHtml(fv.remark_Mr)}</span>` : ''}</div>`;
                }
                if (fv.recommendation_En || fv.recommendation_Mr) {
                    html += `<div class="small mb-1 text-success"><i class="bi bi-chat-right-text me-1"></i><strong>${t('fieldVisit.recommendation')}:</strong> ${escapeHtml(fv.recommendation_En || '')}${fv.recommendation_Mr ? ` <span class="text-muted" lang="mr">| ${escapeHtml(fv.recommendation_Mr)}</span>` : ''}</div>`;
                }
                if (fv.signaturePath) {
                    html += `<div class="small mb-2"><strong>${t('fieldVisit.signature')}:</strong> <img src="${fv.signaturePath}" alt="Signature" class="border rounded ms-2" style="max-height:40px;"></div>`;
                }
                if (fv.completedAt) {
                    html += `<div class="small text-success mb-2"><i class="bi bi-check-circle me-1"></i>${t('fieldVisit.completed')}: ${formatDate(fv.completedAt)}</div>`;
                }

                // Read-only photo gallery
                html += `<div class="border rounded p-3 mt-2" style="background:#f8f9fa;">
                    <h6 class="mb-2 fw-bold"><i class="bi bi-images me-2 text-primary"></i>${t('fieldVisit.sitePhotos')}
                        <span class="badge ${photoCount > 0 ? 'bg-primary' : 'bg-secondary'} ms-1">${photoCount}</span>
                    </h6>
                    ${photoCount === 0 ? `<p class="text-muted small mb-0">${t('fieldVisit.noPhotos')}</p>` : ''}
                    ${photoCount > 0 ? `
                        <div class="d-flex flex-wrap gap-2">
                            ${fv.photos.map(p => `
                                <div class="position-relative border rounded overflow-hidden shadow-sm" style="width:110px;height:110px;">
                                    <img src="${p.storagePath || p.fileName}" alt="${escapeHtml(p.caption || p.fileName)}"
                                        class="w-100 h-100" style="object-fit:cover;cursor:pointer;"
                                        onclick="window.open(this.src,'_blank')" title="Click to view full size">
                                </div>`).join('')}
                        </div>` : ''}
                </div>`;
            }

            html += `</div></div>`;
        });
    }

    // Assign Visit modal
    html += `<div class="modal fade" id="assignFvModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-sm">
            <div class="modal-content">
                <div class="modal-header py-2">
                    <h6 class="modal-title mb-0">${t('fieldVisit.assignTitle')}</h6>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <label for="fv-engineer-select" class="form-label">${t('fieldVisit.selectEngineer')}</label>
                    <select class="form-select" id="fv-engineer-select" required>
                        <option value="">Loading...</option>
                    </select>
                </div>
                <div class="modal-footer py-2">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">${t('common.cancel')}</button>
                    <button type="button" class="btn btn-primary btn-sm" id="btn-confirm-assign-fv">${t('fieldVisit.assign')}</button>
                </div>
            </div>
        </div>
    </div>`;

    c.innerHTML = html;

    // ── Create dual-lang inputs for editable visits ──
    const dualInputs = {}; // { fvId: { problem, measurements, remark, recommendation } }
    visits.filter(fv => canEdit && fv.status !== 'Completed').forEach(fv => {
        dualInputs[fv.id] = {};

        const problemContainer = document.getElementById(`fv-problem-${fv.id}`);
        if (problemContainer) {
            dualInputs[fv.id].problem = createDualLangInput({
                name: `fvProblem-${fv.id.substring(0,8)}`, label: t('fieldVisit.problem'), i18nKey: 'fieldVisit.problem',
                type: 'textarea', required: true, maxLength: 2000,
                valueEn: fv.problemDescription_En || '', valueMr: fv.problemDescription_Mr || ''
            });
            problemContainer.appendChild(dualInputs[fv.id].problem);
        }

        const measContainer = document.getElementById(`fv-measurements-${fv.id}`);
        if (measContainer) {
            dualInputs[fv.id].measurements = createDualLangInput({
                name: `fvMeasurements-${fv.id.substring(0,8)}`, label: t('fieldVisit.measurements') + ` (${t('common.optional')})`, i18nKey: 'fieldVisit.measurements',
                type: 'textarea', required: false, maxLength: 2000,
                valueEn: fv.measurements_En || '', valueMr: fv.measurements_Mr || ''
            });
            measContainer.appendChild(dualInputs[fv.id].measurements);
        }

        const remarkContainer = document.getElementById(`fv-remark-${fv.id}`);
        if (remarkContainer) {
            dualInputs[fv.id].remark = createDualLangInput({
                name: `fvRemark-${fv.id.substring(0,8)}`, label: t('fieldVisit.remark'), i18nKey: 'fieldVisit.remark',
                type: 'text', required: false, maxLength: 500,
                valueEn: fv.remark_En || '', valueMr: fv.remark_Mr || ''
            });
            remarkContainer.appendChild(dualInputs[fv.id].remark);
        }

        const recContainer = document.getElementById(`fv-recommendation-${fv.id}`);
        if (recContainer) {
            dualInputs[fv.id].recommendation = createDualLangInput({
                name: `fvRec-${fv.id.substring(0,8)}`, label: t('fieldVisit.recommendation'), i18nKey: 'fieldVisit.recommendation',
                type: 'textarea', required: false, maxLength: 2000,
                valueEn: fv.recommendation_En || '', valueMr: fv.recommendation_Mr || ''
            });
            recContainer.appendChild(dualInputs[fv.id].recommendation);
        }
    });

    // ── Wire: Form submit (Save) ──
    c.querySelectorAll('.fv-edit-form').forEach(form => {
        form.addEventListener('submit', async e => {
            e.preventDefault();
            const fvId = form.dataset.fvId;
            const inputs = dualInputs[fvId] || {};
            const problemVals = inputs.problem?.getValues() || { en: '', alt: '' };
            const measVals = inputs.measurements?.getValues() || { en: '', alt: '' };
            const remarkVals = inputs.remark?.getValues() || { en: '', alt: '' };
            const recVals = inputs.recommendation?.getValues() || { en: '', alt: '' };

            const body = {
                id: fvId,
                siteConditionId: form.querySelector('.fv-site-condition')?.value || null,
                inspectionDate: form.querySelector('.fv-inspection-date')?.value || null,
                problemDescription_En: problemVals.en,
                problemDescription_Mr: problemVals.alt,
                measurements_En: measVals.en,
                measurements_Mr: measVals.alt,
                gpsLatitude: parseFloat(form.querySelector('.fv-gps-lat')?.value) || null,
                gpsLongitude: parseFloat(form.querySelector('.fv-gps-long')?.value) || null,
                remark_En: remarkVals.en,
                remark_Mr: remarkVals.alt,
                recommendation_En: recVals.en,
                recommendation_Mr: recVals.alt
            };

            const r = await api.put(`/proposals/${pid}/field-visits/${fvId}`, body);
            if (r.success) { toast.success(t('fieldVisit.saved')); await renderTab2(c, pid, canEdit); }
            else toast.error(r.error || t('common.loadError'));
        });
    });

    // ── Wire: PDF upload ──
    c.querySelectorAll('.fv-pdf-input').forEach(input => {
        input.addEventListener('change', async e => {
            const file = e.target.files[0];
            if (!file) return;
            if (file.size > 10 * 1024 * 1024) { toast.error(t('fieldVisit.pdfSizeError')); return; }
            const fd = new FormData();
            fd.append('file', file);
            const fvId = input.dataset.fvId;
            const r = await api.upload(`/proposals/${pid}/field-visits/${fvId}/pdf`, fd);
            if (r.success) { toast.success(t('fieldVisit.pdfUploadSuccess')); await renderTab2(c, pid, canEdit); }
            else toast.error(r.error || t('common.loadError'));
        });
    });

    // ── Wire: Signature upload ──
    c.querySelectorAll('.fv-signature-input').forEach(input => {
        input.addEventListener('change', async e => {
            const file = e.target.files[0];
            if (!file) return;
            if (file.size > 2 * 1024 * 1024) { toast.error(t('fieldVisit.signatureSizeError')); return; }
            const fd = new FormData();
            fd.append('file', file);
            const fvId = input.dataset.fvId;
            const r = await api.upload(`/proposals/${pid}/field-visits/${fvId}/signature`, fd);
            if (r.success) { toast.success(t('fieldVisit.signatureUploadSuccess')); await renderTab2(c, pid, canEdit); }
            else toast.error(r.error || t('common.loadError'));
        });
    });

    // ── Wire: Assign Visit button → open modal and load engineers ──
    const btnAssign = document.getElementById('btn-assign-fv');
    if (btnAssign) btnAssign.addEventListener('click', async () => {
        const modal = new bootstrap.Modal(document.getElementById('assignFvModal'));
        const select = document.getElementById('fv-engineer-select');
        select.innerHTML = '<option value="">Loading...</option>';
        modal.show();

        const engRes = await api.get(`/proposals/${pid}/field-visits/assignable-engineers`);
        if (engRes.success && engRes.data) {
            select.innerHTML = `<option value="">— ${t('fieldVisit.selectPlaceholder')} —</option>` +
                engRes.data.map(e => `<option value="${e.id}">${escapeHtml(e.fullName_En)} (${e.role}${e.departmentName ? ' — ' + escapeHtml(e.departmentName) : ''})</option>`).join('');
        } else {
            select.innerHTML = '<option value="">No engineers found</option>';
        }
    });

    // ── Wire: Confirm assign ──
    document.getElementById('btn-confirm-assign-fv')?.addEventListener('click', async () => {
        const engineerId = document.getElementById('fv-engineer-select').value;
        if (!engineerId) { toast.error('Please select an engineer'); return; }
        const r = await api.post(`/proposals/${pid}/field-visits/assign`, { assignedToId: engineerId });
        bootstrap.Modal.getInstance(document.getElementById('assignFvModal'))?.hide();
        if (r.success) { toast.success('Visit assigned'); await renderTab2(c, pid, canEdit); } 
        else toast.error(r.error || 'Failed to assign');
    });

    // ── Wire: Complete buttons (Sign & Complete) ──
    c.querySelectorAll('.btn-complete-fv').forEach(btn =>
        btn.addEventListener('click', async () => {
            const fvId = btn.dataset.id;
            // First save the form data
            const form = btn.closest('.fv-edit-form');
            if (form) {
                const inputs = dualInputs[fvId] || {};
                const problemVals = inputs.problem?.getValues() || { en: '', alt: '' };
                if (!problemVals.en) { toast.error(t('fieldVisit.problemRequired')); return; }

                const measVals = inputs.measurements?.getValues() || { en: '', alt: '' };
                const remarkVals = inputs.remark?.getValues() || { en: '', alt: '' };
                const recVals = inputs.recommendation?.getValues() || { en: '', alt: '' };

                const body = {
                    id: fvId,
                    siteConditionId: form.querySelector('.fv-site-condition')?.value || null,
                    inspectionDate: form.querySelector('.fv-inspection-date')?.value || null,
                    problemDescription_En: problemVals.en,
                    problemDescription_Mr: problemVals.alt,
                    measurements_En: measVals.en,
                    measurements_Mr: measVals.alt,
                    gpsLatitude: parseFloat(form.querySelector('.fv-gps-lat')?.value) || null,
                    gpsLongitude: parseFloat(form.querySelector('.fv-gps-long')?.value) || null,
                    remark_En: remarkVals.en,
                    remark_Mr: remarkVals.alt,
                    recommendation_En: recVals.en,
                    recommendation_Mr: recVals.alt
                };

                const saveRes = await api.put(`/proposals/${pid}/field-visits/${fvId}`, body);
                if (!saveRes.success) { toast.error(saveRes.error || t('common.loadError')); return; }
            }

            // Then complete
            const r = await api.post(`/proposals/${pid}/field-visits/${fvId}/complete`);
            if (r.success) { toast.success(t('fieldVisit.visitCompleted')); await renderTab2(c, pid, canEdit); }
            else toast.error(r.error || 'Failed');
        })
    );

    // ── Wire: Photo upload inputs ──
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

    // ── Wire: Drag-and-drop on drop zones ──
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

    // ── Wire: Delete photo buttons ──
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
        c.innerHTML = `<h6 class="mb-3"><i class="bi bi-calculator me-1"></i>${tBilingual('estimate.title')}</h6>
        <form id="est-form"><div class="row g-3">
            <div class="col-md-6"><label for="estCost" class="form-label">${tBilingual('estimate.cost')} (₹)</label><input type="number" class="form-control" id="estCost" step="0.01" min="0" required></div>
        </div><button type="submit" class="btn btn-primary btn-sm mt-3"><i class="bi bi-floppy me-1"></i>${t('estimate.save')}</button></form>`;
        document.getElementById('est-form').addEventListener('submit', async e => {
            e.preventDefault();
            const r = await api.post(`/proposals/${pid}/estimate`, { proposalId: pid, estimatedCost: parseFloat(document.getElementById('estCost').value) });
            if (r.success) { toast.success('Estimate saved'); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        });
        return;
    }
    if (!est) { c.innerHTML = `<p class="text-muted">${t('estimate.noRecord')}</p>`; return; }

    const sBg = est.status === 'Approved' ? 'success' : est.status === 'SentForApproval' ? 'warning text-dark' : est.status === 'ReturnedWithQuery' ? 'danger' : 'secondary';
    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-calculator me-1"></i>${tBilingual('estimate.title')}</h6>
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('estimate.cost')}</label><div class="fw-medium fs-5">${formatCurrency(est.estimatedCost)}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('estimate.status')}</label><div><span class="badge bg-${sBg}">${est.status}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('estimate.preparedBy')}</label><div>${escapeHtml(est.preparedByName || '—')}</div></div>
            ${est.sentToName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('estimate.sentTo')}</label><div>${escapeHtml(est.sentToName)} (${est.sentToRole})</div></div>` : ''}
            ${est.approvedByName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('estimate.approvedBy')}</label><div>${escapeHtml(est.approvedByName)}</div></div>` : ''}
            ${est.approvedAt ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('estimate.approvedAt')}</label><div>${formatDate(est.approvedAt)}</div></div>` : ''}
            ${est.approverOpinion_En || est.approverOpinion_Mr ? `<div class="col-12"><label class="form-label text-muted small mb-0">${tBilingual('estimate.opinion')}</label><div>${escapeHtml(est.approverOpinion_En || '')}${est.approverOpinion_Mr ? `<br><span class="text-muted" lang="mr">${escapeHtml(est.approverOpinion_Mr)}</span>` : ''}</div></div>` : ''}
            ${est.returnQueryNote_En || est.returnQueryNote_Mr ? `<div class="col-12"><label class="form-label text-muted small mb-0 text-danger">${tBilingual('estimate.returnQuery')}</label><div class="text-danger">${escapeHtml(est.returnQueryNote_En || '')}${est.returnQueryNote_Mr ? `<br><span lang="mr">${escapeHtml(est.returnQueryNote_Mr)}</span>` : ''}</div></div>` : ''}
        </div>

        <!-- File uploads section -->
        <hr class="my-3">
        <h6 class="mb-2"><i class="bi bi-file-earmark-arrow-up me-1"></i>${t('estimate.uploads')}</h6>
        <div class="row g-3">
            <div class="col-md-4">
                <label class="form-label text-muted small mb-1">${tBilingual('estimate.estimatePdf')}</label>
                ${est.estimatePdfPath
                    ? `<div><a href="${est.estimatePdfPath}" target="_blank" class="btn btn-outline-primary btn-sm"><i class="bi bi-file-pdf me-1"></i>${t('common.viewPdf')}</a></div>`
                    : canEdit && est.status === 'Draft' ? `<input type="file" class="form-control form-control-sm" id="est-pdf-upload" accept="application/pdf">` : `<div class="text-muted small">${t('common.notUploaded')}</div>`}
            </div>
            <div class="col-md-4">
                <label class="form-label text-muted small mb-1">${tBilingual('estimate.preparedSignature')}</label>
                ${est.preparedSignaturePath
                    ? `<div><img src="${est.preparedSignaturePath}" alt="${t('estimate.preparedSignature')}" class="border rounded" style="max-height:60px;"></div>`
                    : canEdit && est.status === 'Draft' ? `<input type="file" class="form-control form-control-sm" id="est-prep-sig-upload" accept="image/png,image/jpeg,image/svg+xml">` : `<div class="text-muted small">${t('common.notUploaded')}</div>`}
            </div>
            <div class="col-md-4">
                <label class="form-label text-muted small mb-1">${tBilingual('estimate.approverSignature')}</label>
                ${est.approverSignaturePath
                    ? `<div><img src="${est.approverSignaturePath}" alt="${t('estimate.approverSignature')}" class="border rounded" style="max-height:60px;"></div>`
                    : canEdit && est.status === 'SentForApproval' ? `<input type="file" class="form-control form-control-sm" id="est-appr-sig-upload" accept="image/png,image/jpeg,image/svg+xml">` : `<div class="text-muted small">${t('common.notUploaded')}</div>`}
            </div>
        </div>

        ${canEdit && est.status === 'Draft' ? `<div class="mt-3"><button class="btn btn-outline-primary btn-sm" id="btn-send-est"><i class="bi bi-send me-1"></i>${t('estimate.sendForApproval')}</button></div>` : ''}
        ${canEdit && est.status === 'SentForApproval' ? `<div class="mt-3">
            <button class="btn btn-success btn-sm" id="btn-approve-est"><i class="bi bi-check-lg me-1"></i>${t('estimate.approve')}</button>
            <button class="btn btn-warning btn-sm ms-2" id="btn-return-est"><i class="bi bi-arrow-return-left me-1"></i>${t('estimate.return')}</button>
        </div>` : ''}`;

    // Wire upload handlers for estimate files
    document.getElementById('est-pdf-upload')?.addEventListener('change', async e => {
        const file = e.target.files[0]; if (!file) return;
        const fd = new FormData(); fd.append('file', file);
        const r = await api.upload(`/proposals/${pid}/estimate/${est.id}/pdf`, fd);
        if (r.success) { toast.success(t('estimate.pdfUploaded')); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Upload failed');
    });
    document.getElementById('est-prep-sig-upload')?.addEventListener('change', async e => {
        const file = e.target.files[0]; if (!file) return;
        const fd = new FormData(); fd.append('file', file);
        const r = await api.upload(`/proposals/${pid}/estimate/${est.id}/prepared-signature`, fd);
        if (r.success) { toast.success(t('estimate.signatureUploaded')); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Upload failed');
    });
    document.getElementById('est-appr-sig-upload')?.addEventListener('change', async e => {
        const file = e.target.files[0]; if (!file) return;
        const fd = new FormData(); fd.append('file', file);
        const r = await api.upload(`/proposals/${pid}/estimate/${est.id}/approver-signature`, fd);
        if (r.success) { toast.success(t('estimate.signatureUploaded')); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Upload failed');
    });

    document.getElementById('btn-send-est')?.addEventListener('click', async () => {
        const r = await api.post(`/proposals/${pid}/estimate/${est.id}/send-for-approval`, { targetRole: 'CityEngineer' });
        if (r.success) { toast.success('Sent for approval'); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Failed');
    });
    document.getElementById('btn-approve-est')?.addEventListener('click', async () => {
        const opinion = prompt(t('estimate.opinionPrompt'));
        if (opinion === null) return;
        const r = await api.post(`/proposals/${pid}/estimate/${est.id}/approve`, { estimateId: est.id, disclaimerAccepted: true, opinion_En: opinion || null });
        if (r.success) { toast.success('Estimate approved'); await renderTab3(c, pid, canEdit); } else toast.error(r.error || 'Failed');
    });
    document.getElementById('btn-return-est')?.addEventListener('click', async () => {
        const note = prompt(t('estimate.returnPrompt'));
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
        c.innerHTML = `<h6 class="mb-3"><i class="bi bi-shield-check me-1"></i>${tBilingual('techSanction.title')}</h6>
        <form id="ts-form"><div class="row g-3">
            <div class="col-md-4"><label for="tsNum" class="form-label">${tBilingual('techSanction.number')}</label><input type="text" class="form-control" id="tsNum"></div>
            <div class="col-md-4"><label for="tsDate" class="form-label">${tBilingual('techSanction.date')}</label><input type="date" class="form-control" id="tsDate"></div>
            <div class="col-md-4"><label for="tsAmt" class="form-label">${tBilingual('techSanction.amount')} (₹)</label><input type="number" class="form-control" id="tsAmt" step="0.01"></div>
            <div class="col-12" id="ts-desc-container"></div>
            <div class="col-12" id="ts-sanctBy-container"></div>
            <div class="col-12" id="ts-sanctDept-container"></div>
            <div class="col-12" id="ts-sanctDesig-container"></div>
        </div><button type="submit" class="btn btn-primary btn-sm mt-3"><i class="bi bi-floppy me-1"></i>${t('techSanction.save')}</button></form>`;

        // Create dual-lang inputs for TS form
        const dualDesc = createDualLangInput({ name: 'tsDesc', label: 'Description', i18nKey: 'techSanction.description', type: 'textarea', rows: 3 });
        document.getElementById('ts-desc-container').appendChild(dualDesc);
        const dualSanctBy = createDualLangInput({ name: 'tsSanctBy', label: 'Sanctioned By', i18nKey: 'techSanction.sanctionedBy', type: 'text' });
        document.getElementById('ts-sanctBy-container').appendChild(dualSanctBy);
        const dualSanctDept = createDualLangInput({ name: 'tsSanctDept', label: 'Dept', i18nKey: 'techSanction.dept', type: 'text' });
        document.getElementById('ts-sanctDept-container').appendChild(dualSanctDept);
        const dualSanctDesig = createDualLangInput({ name: 'tsSanctDesig', label: 'Designation', i18nKey: 'techSanction.designation', type: 'text' });
        document.getElementById('ts-sanctDesig-container').appendChild(dualSanctDesig);

        document.getElementById('ts-form').addEventListener('submit', async e => {
            e.preventDefault();
            const descVals = dualDesc.getValues();
            const sanctByVals = dualSanctBy.getValues();
            const sanctDeptVals = dualSanctDept.getValues();
            const sanctDesigVals = dualSanctDesig.getValues();
            const body = { proposalId: pid,
                tsNumber: document.getElementById('tsNum').value || null,
                tsDate: document.getElementById('tsDate').value || null,
                tsAmount: parseFloat(document.getElementById('tsAmt').value) || null,
                description_En: descVals.en || null,
                description_Mr: descVals.mr || null,
                sanctionedByName: sanctByVals.en || null,
                sanctionedByName_Mr: sanctByVals.mr || null,
                sanctionedByDept: sanctDeptVals.en || null,
                sanctionedByDept_Mr: sanctDeptVals.mr || null,
                sanctionedByDesignation: sanctDesigVals.en || null,
                sanctionedByDesignation_Mr: sanctDesigVals.mr || null
            };
            const r = await api.post(`/proposals/${pid}/technical-sanction`, body);
            if (r.success) { toast.success('Saved'); await renderTab4(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        });
        return;
    }
    if (!ts) { c.innerHTML = `<p class="text-muted">${t('techSanction.noRecord')}</p>`; return; }

    const sBg = ts.status === 'Signed' ? 'success' : 'secondary';
    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-shield-check me-1"></i>${tBilingual('techSanction.title')}</h6>
        <div class="row g-3">
            <div class="col-md-3"><label class="form-label text-muted small mb-0">${tBilingual('techSanction.number')}</label><div>${escapeHtml(ts.tsNumber || '—')}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">${tBilingual('techSanction.date')}</label><div>${formatDate(ts.tsDate)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">${tBilingual('techSanction.amount')}</label><div class="fw-medium">${formatCurrency(ts.tsAmount)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">${tBilingual('techSanction.status')}</label><div><span class="badge bg-${sBg}">${ts.status}</span></div></div>
            ${bilingualDisplay('Sanctioned By', ts.sanctionedByName, ts.sanctionedByName_Mr, 'col-md-4', t('techSanction.sanctionedBy', 'mr'))}
            ${bilingualDisplay('Dept', ts.sanctionedByDept, ts.sanctionedByDept_Mr, 'col-md-4', t('techSanction.dept', 'mr'))}
            ${bilingualDisplay('Designation', ts.sanctionedByDesignation, ts.sanctionedByDesignation_Mr, 'col-md-4', t('techSanction.designation', 'mr'))}
            ${ts.description_En || ts.description_Mr ? bilingualDisplay('Description', ts.description_En, ts.description_Mr, 'col-12', t('techSanction.description', 'mr')) : ''}
            ${ts.preparedByName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('techSanction.preparedBy')}</label><div>${escapeHtml(ts.preparedByName)}</div></div>` : ''}
            ${ts.signedByName ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('techSanction.signedBy')}</label><div>${escapeHtml(ts.signedByName)}</div></div>` : ''}
            ${ts.signedAt ? `<div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('techSanction.signedAt')}</label><div>${formatDate(ts.signedAt)}</div></div>` : ''}
        </div>

        <!-- File uploads section -->
        <hr class="my-3">
        <h6 class="mb-2"><i class="bi bi-file-earmark-arrow-up me-1"></i>${t('techSanction.uploads')}</h6>
        <div class="row g-3">
            <div class="col-md-4">
                <label class="form-label text-muted small mb-1">${tBilingual('techSanction.tsPdf')}</label>
                ${ts.tsPdfPath
                    ? `<div><a href="${ts.tsPdfPath}" target="_blank" class="btn btn-outline-primary btn-sm"><i class="bi bi-file-pdf me-1"></i>${t('common.viewPdf')}</a></div>`
                    : canEdit && ts.status === 'Draft' ? `<input type="file" class="form-control form-control-sm" id="ts-pdf-upload" accept="application/pdf">` : `<div class="text-muted small">${t('common.notUploaded')}</div>`}
            </div>
            <div class="col-md-4">
                <label class="form-label text-muted small mb-1">${tBilingual('techSanction.outsideApprovalLetter')}</label>
                ${ts.outsideApprovalLetterPath
                    ? `<div><a href="${ts.outsideApprovalLetterPath}" target="_blank" class="btn btn-outline-secondary btn-sm"><i class="bi bi-file-earmark me-1"></i>${t('common.viewFile')}</a></div>`
                    : canEdit && ts.status === 'Draft' ? `<input type="file" class="form-control form-control-sm" id="ts-letter-upload">` : `<div class="text-muted small">${t('common.notUploaded')}</div>`}
            </div>
            <div class="col-md-4">
                <label class="form-label text-muted small mb-1">${tBilingual('techSanction.signerSignature')}</label>
                ${ts.signerSignaturePath
                    ? `<div><img src="${ts.signerSignaturePath}" alt="${t('techSanction.signerSignature')}" class="border rounded" style="max-height:60px;"></div>`
                    : canEdit && ts.status === 'Draft' ? `<input type="file" class="form-control form-control-sm" id="ts-sig-upload" accept="image/png,image/jpeg,image/svg+xml">` : `<div class="text-muted small">${t('common.notUploaded')}</div>`}
            </div>
        </div>

        ${canEdit && ts.status === 'Draft' ? `<div class="mt-3"><button class="btn btn-success btn-sm" id="btn-sign-ts"><i class="bi bi-pen me-1"></i>${t('techSanction.sign')}</button></div>` : ''}`;

    // Wire upload handlers for TS files
    document.getElementById('ts-pdf-upload')?.addEventListener('change', async e => {
        const file = e.target.files[0]; if (!file) return;
        const fd = new FormData(); fd.append('file', file);
        const r = await api.upload(`/proposals/${pid}/technical-sanction/${ts.id}/pdf`, fd);
        if (r.success) { toast.success(t('techSanction.pdfUploaded')); await renderTab4(c, pid, canEdit); } else toast.error(r.error || 'Upload failed');
    });
    document.getElementById('ts-letter-upload')?.addEventListener('change', async e => {
        const file = e.target.files[0]; if (!file) return;
        const fd = new FormData(); fd.append('file', file);
        const r = await api.upload(`/proposals/${pid}/technical-sanction/${ts.id}/outside-approval-letter`, fd);
        if (r.success) { toast.success(t('techSanction.letterUploaded')); await renderTab4(c, pid, canEdit); } else toast.error(r.error || 'Upload failed');
    });
    document.getElementById('ts-sig-upload')?.addEventListener('change', async e => {
        const file = e.target.files[0]; if (!file) return;
        const fd = new FormData(); fd.append('file', file);
        const r = await api.upload(`/proposals/${pid}/technical-sanction/${ts.id}/signer-signature`, fd);
        if (r.success) { toast.success(t('techSanction.signatureUploaded')); await renderTab4(c, pid, canEdit); } else toast.error(r.error || 'Upload failed');
    });

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
        c.innerHTML = `<h6 class="mb-3"><i class="bi bi-receipt me-1"></i>${tBilingual('prama.title')}</h6>
        <form id="prama-form"><div class="row g-3">
            <div class="col-md-4"><label for="pFT" class="form-label">${tBilingual('prama.fundType')}</label><select class="form-select" id="pFT"><option value="">${t('common.selectOption')}</option>${fts.map(f => `<option value="${f.id}" ${pd?.fundTypeId === f.id ? 'selected' : ''}>${escapeHtml(f.name_En)}</option>`).join('')}</select></div>
            <div class="col-md-4"><label for="pBH" class="form-label">${tBilingual('prama.budgetHead')}</label><select class="form-select" id="pBH"><option value="">${t('common.selectOption')}</option>${bhs.map(b => `<option value="${b.id}" ${pd?.budgetHeadId === b.id ? 'selected' : ''}>${escapeHtml(b.name_En)}</option>`).join('')}</select></div>
            <div class="col-md-4"><label for="pYear" class="form-label">${tBilingual('prama.fundYear')}</label><input type="text" class="form-control" id="pYear" value="${escapeHtml(pd?.fundApprovalYear || '')}" placeholder="2025-26"></div>
            <div class="col-12" id="prama-deptUser-container"></div>
            <div class="col-12" id="prama-ref-container"></div>
            <div class="col-12" id="prama-add-container"></div>
        </div><button type="submit" class="btn btn-primary btn-sm mt-3"><i class="bi bi-floppy me-1"></i>${t('prama.save')}</button></form>`;

        // Create dual-lang inputs for PRAMA form
        const dualDeptUser = createDualLangInput({ name: 'pDeptUser', label: 'Dept User', i18nKey: 'prama.deptUser', type: 'text', valueEn: pd?.deptUserName_En || '', valueMr: pd?.deptUserName_Mr || '' });
        document.getElementById('prama-deptUser-container').appendChild(dualDeptUser);
        const dualRef = createDualLangInput({ name: 'pRef', label: 'References', i18nKey: 'prama.references', type: 'textarea', rows: 2, valueEn: pd?.references_En || '', valueMr: pd?.references_Mr || '' });
        document.getElementById('prama-ref-container').appendChild(dualRef);
        const dualAdd = createDualLangInput({ name: 'pAdd', label: 'Additional Details', i18nKey: 'prama.additionalDetails', type: 'textarea', rows: 2, valueEn: pd?.additionalDetails_En || '', valueMr: pd?.additionalDetails_Mr || '' });
        document.getElementById('prama-add-container').appendChild(dualAdd);

        document.getElementById('prama-form').addEventListener('submit', async e => {
            e.preventDefault();
            const deptVals = dualDeptUser.getValues();
            const refVals = dualRef.getValues();
            const addVals = dualAdd.getValues();
            const body = { proposalId: pid,
                fundTypeId: document.getElementById('pFT').value || null,
                budgetHeadId: document.getElementById('pBH').value || null,
                fundApprovalYear: document.getElementById('pYear').value || null,
                deptUserName_En: deptVals.en || null,
                deptUserName_Mr: deptVals.mr || null,
                references_En: refVals.en || null,
                references_Mr: refVals.mr || null,
                additionalDetails_En: addVals.en || null,
                additionalDetails_Mr: addVals.mr || null };
            const r = await api.post(`/proposals/${pid}/prama`, body);
            if (r.success) toast.success('PRAMA saved'); else toast.error(r.error || 'Failed');
        });
        return;
    }
    if (!pd) { c.innerHTML = `<p class="text-muted">${t('prama.noRecord')}</p>`; return; }

    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-receipt me-1"></i>${tBilingual('prama.title')}</h6>
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('prama.fundType')}</label><div>${escapeHtml(pd.fundTypeName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('prama.budgetHead')}</label><div>${escapeHtml(pd.budgetHeadName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('prama.fundYear')}</label><div>${escapeHtml(pd.fundApprovalYear || '—')}</div></div>
            ${bilingualDisplay('Dept User', pd.deptUserName_En, pd.deptUserName_Mr, 'col-md-6', t('prama.deptUser', 'mr'))}
            ${pd.references_En || pd.references_Mr ? bilingualDisplay('References', pd.references_En, pd.references_Mr, 'col-12', t('prama.references', 'mr')) : ''}
            ${pd.additionalDetails_En || pd.additionalDetails_Mr ? bilingualDisplay('Additional Details', pd.additionalDetails_En, pd.additionalDetails_Mr, 'col-12', t('prama.additionalDetails', 'mr')) : ''}
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
        c.innerHTML = `<h6 class="mb-3"><i class="bi bi-wallet2 me-1"></i>${tBilingual('budget.title')}</h6>
        <form id="budget-form"><div class="row g-3">
            <div class="col-md-4"><label for="bWM" class="form-label">${tBilingual('budget.workMethod')}</label><select class="form-select" id="bWM"><option value="">${t('common.selectOption')}</option>${wms.map(w => `<option value="${w.id}" ${bd?.workExecutionMethodId === w.id ? 'selected' : ''}>${escapeHtml(w.name_En)}</option>`).join('')}</select></div>
            <div class="col-md-4"><label for="bDays" class="form-label">${tBilingual('budget.duration')}</label><input type="number" class="form-control" id="bDays" value="${bd?.workDurationDays || ''}"></div>
            <div class="col-md-4"><label for="bBH" class="form-label">${tBilingual('budget.budgetHead')}</label><select class="form-select" id="bBH"><option value="">${t('common.selectOption')}</option>${bhs.map(b => `<option value="${b.id}" ${bd?.budgetHeadId === b.id ? 'selected' : ''}>${escapeHtml(b.name_En)}</option>`).join('')}</select></div>
            <div class="col-md-3"><label for="bAlloc" class="form-label">${tBilingual('budget.allocated')}</label><input type="number" class="form-control" id="bAlloc" step="0.01" value="${bd?.allocatedFund || ''}"></div>
            <div class="col-md-3"><label for="bAvail" class="form-label">${tBilingual('budget.available')}</label><input type="number" class="form-control" id="bAvail" step="0.01" value="${bd?.currentAvailableFund || ''}"></div>
            <div class="col-md-3"><label for="bOldExp" class="form-label">${tBilingual('budget.oldExpenditure')}</label><input type="number" class="form-control" id="bOldExp" step="0.01" value="${bd?.oldExpenditure || ''}"></div>
            <div class="col-md-3"><label for="bEstCost" class="form-label">${tBilingual('budget.estimatedCost')}</label><input type="number" class="form-control" id="bEstCost" step="0.01" value="${bd?.estimatedCost || ''}"></div>
            <div class="col-md-4"><label for="bSerial" class="form-label">${tBilingual('budget.accountSerial')}</label><input type="text" class="form-control" id="bSerial" value="${escapeHtml(bd?.accountSerialNo || '')}"></div>
            <div class="col-md-4 d-flex align-items-end"><div class="form-check"><input class="form-check-input" type="checkbox" id="bTender" ${bd?.tenderVerificationDone ? 'checked' : ''}><label class="form-check-label" for="bTender">${tBilingual('budget.tenderVerified')}</label></div></div>
            <div class="col-12" id="budget-compliance-container"></div>
        </div><button type="submit" class="btn btn-primary btn-sm mt-3"><i class="bi bi-floppy me-1"></i>${t('budget.save')}</button></form>`;

        const dualCompliance = createDualLangInput({ name: 'bComp', label: 'Compliance Notes', i18nKey: 'budget.compliance', type: 'textarea', rows: 2, valueEn: bd?.complianceNotes_En || '', valueMr: bd?.complianceNotes_Mr || '' });
        document.getElementById('budget-compliance-container').appendChild(dualCompliance);

        document.getElementById('budget-form').addEventListener('submit', async e => {
            e.preventDefault();
            const compVals = dualCompliance.getValues();
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
                complianceNotes_En: compVals.en || null,
                complianceNotes_Mr: compVals.mr || null };
            const r = await api.post(`/proposals/${pid}/budget`, body);
            if (r.success) { toast.success('Budget saved'); await renderTab6(c, pid, canEdit); } else toast.error(r.error || 'Failed');
        });
        return;
    }
    if (!bd) { c.innerHTML = `<p class="text-muted">${t('budget.noRecord')}</p>`; return; }

    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-wallet2 me-1"></i>${tBilingual('budget.title')}</h6>
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('budget.workMethod')}</label><div>${escapeHtml(bd.workExecutionMethodName || '—')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('budget.duration')}</label><div>${bd.workDurationDays || '—'} ${t('common.days')}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('budget.budgetHead')}</label><div>${escapeHtml(bd.budgetHeadName || '—')}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">${tBilingual('budget.allocated')}</label><div>${formatCurrency(bd.allocatedFund)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">${tBilingual('budget.available')}</label><div>${formatCurrency(bd.currentAvailableFund)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">${tBilingual('budget.oldExpenditure')}</label><div>${formatCurrency(bd.oldExpenditure)}</div></div>
            <div class="col-md-3"><label class="form-label text-muted small mb-0">${tBilingual('budget.estimatedCost')}</label><div class="fw-medium">${formatCurrency(bd.estimatedCost)}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('budget.balance')}</label><div class="fw-medium ${(bd.balanceAmount || 0) < 0 ? 'text-danger' : 'text-success'}">${formatCurrency(bd.balanceAmount)}</div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('budget.approvalSlab')}</label><div><span class="badge bg-info">${bd.determinedApprovalSlab || '—'}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('budget.finalAuthority')}</label><div><span class="badge bg-dark">${bd.finalAuthorityRole || '—'}</span></div></div>
            <div class="col-md-4"><label class="form-label text-muted small mb-0">${tBilingual('budget.tenderVerified')}</label><div>${bd.tenderVerificationDone ? `<i class="bi bi-check-circle-fill text-success"></i> ${t('common.yes')}` : `<i class="bi bi-x-circle text-danger"></i> ${t('common.no')}`}</div></div>
            ${bd.complianceNotes_En || bd.complianceNotes_Mr ? bilingualDisplay('Compliance Notes', bd.complianceNotes_En, bd.complianceNotes_Mr, 'col-12', t('budget.compliance', 'mr')) : ''}
        </div>`;
}

// ── Timeline ─────────────────────────────────────────────────
async function renderTimeline(c, pid) {
    const res = await api.get(`/proposals/${pid}/approval-history`);
    if (!res.success || !res.data || res.data.length === 0) {
        c.innerHTML = `<p class="text-muted text-center py-3">${t('timeline.noHistory')}</p>`;
        return;
    }
    c.innerHTML = `<h6 class="mb-3"><i class="bi bi-clock-history me-1"></i>${tBilingual('timeline.title')}</h6>
        <div class="timeline">${res.data.map(h => `
            <div class="timeline-item action-${h.action}">
                <div class="d-flex justify-content-between align-items-start">
                    <div><span class="badge bg-${h.action === 'Approve' ? 'success' : h.action === 'PushBack' ? 'danger' : 'primary'} me-1">${h.action}</span>
                        <strong>${escapeHtml(h.actorName_En || '—')}</strong>${h.actorName_Mr ? ` <span class="text-muted small" lang="mr">(${escapeHtml(h.actorName_Mr)})</span>` : ''} <span class="text-muted">(${h.stageRole})</span></div>
                    <small class="text-muted">${formatDate(h.createdAt)}</small>
                </div>
                ${h.opinion_En || h.opinion_Mr ? `<div class="mt-1 small">${escapeHtml(h.opinion_En || '')}${h.opinion_Mr ? `<br><span class="text-muted" lang="mr">${escapeHtml(h.opinion_Mr)}</span>` : ''}</div>` : ''}
                ${h.pushBackNote_En || h.pushBackNote_Mr ? `<div class="mt-1 small text-danger">${escapeHtml(h.pushBackNote_En || '')}${h.pushBackNote_Mr ? `<br><span lang="mr">${escapeHtml(h.pushBackNote_Mr)}</span>` : ''}</div>` : ''}
            </div>`).join('')}
        </div>`;
}

// ── Documents Tab ────────────────────────────────────────────
async function renderDocuments(c, pid, canEdit) {
    const res = await api.get(`/proposals/${pid}/documents`);
    const docs = res.success ? (res.data || []) : [];

    let html = `<div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h6 class="mb-0"><i class="bi bi-paperclip me-1"></i>${t('documents.title')} (${docs.length})</h6>
        ${canEdit ? `<form id="upload-form" class="d-flex gap-2 align-items-center flex-wrap">
            <select class="form-select form-select-sm" id="uploadDocType" style="width:auto;" required>
                <option value="SupportingDoc">Supporting Doc</option><option value="LocationMap">Location Map</option>
                <option value="SitePhoto">Site Photo</option><option value="EstimateCopy">Estimate Copy</option>
                <option value="TechnicalSanctionDoc">TS Doc</option><option value="Other">Other</option>
            </select>
            <input type="file" class="form-control form-control-sm" id="uploadFile" required style="max-width:250px;">
            <button type="submit" class="btn btn-primary btn-sm"><i class="bi bi-upload me-1"></i>${t('documents.upload')}</button>
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
        html += `<p class="text-muted">${t('documents.noDocuments')}</p>`;
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
            if (!confirm(t('documents.deleteConfirm'))) return;
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
            toast.error(t('fieldVisit.photoRequiredSubmit'));
            return;
        }
        if (!confirm(t('workflow.submitConfirm'))) return;
        const r = await api.post(`/workflow/${id}/submit`);
        if (r.success) { toast.success('Submitted'); renderProposalDetailPage(params); } else toast.error(r.error || 'Failed');
    });

    document.getElementById('btn-approve')?.addEventListener('click', () => new bootstrap.Modal(document.getElementById('approveModal')).show());
    document.getElementById('approve-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const confirmBtn = document.getElementById('btn-confirm-approve');
        confirmBtn.disabled = true;
        try {
            let signaturePath = null;
            const sigFile = document.getElementById('approve-sig-file')?.files[0];
            if (sigFile) {
                const fd = new FormData();
                fd.append('file', sigFile);
                const sigRes = await api.upload(`/workflow/${id}/approval-signature`, fd);
                if (!sigRes.success) { toast.error(sigRes.error || 'Signature upload failed'); return; }
                signaturePath = sigRes.data;
            }
            const r = await api.post(`/workflow/${id}/approve`, {
                proposalId: id, opinion_En: document.getElementById('opinion').value.trim() || null,
                opinion_Mr: document.getElementById('opinion-mr')?.value.trim() || null,
                disclaimerAccepted: document.getElementById('disclaimer-check').checked,
                signaturePath });
            bootstrap.Modal.getInstance(document.getElementById('approveModal'))?.hide();
            if (r.success) { toast.success('Approved'); renderProposalDetailPage(params); } else toast.error(r.error || 'Failed');
        } finally { confirmBtn.disabled = false; }
    });

    document.getElementById('btn-pushback')?.addEventListener('click', () => new bootstrap.Modal(document.getElementById('pushbackModal')).show());
    document.getElementById('pushback-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const note = document.getElementById('pushback-note').value.trim();
        if (!note) { toast.error(t('workflow.reasonRequired')); return; }
        const r = await api.post(`/workflow/${id}/pushback`, {
            proposalId: id, pushBackNote_En: note,
            pushBackNote_Mr: document.getElementById('pushback-note-mr')?.value.trim() || null });
        bootstrap.Modal.getInstance(document.getElementById('pushbackModal'))?.hide();
        if (r.success) { toast.success('Pushed back'); renderProposalDetailPage(params); } else toast.error(r.error || 'Failed');
    });
}
