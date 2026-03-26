/** Profile page — edit profile and signature upload */

import api from '../../api.js';
import { getCurrentUser, setCurrentUser } from '../../auth.js';
import { showToast } from '../../toast.js';
import { getInitials } from '../../utils.js';
import { translateDOM } from '../../i18n.js';

export async function renderProfile(container) {
  let user = getCurrentUser();
  if (!user) return;

  try {
    const freshUser = await api.get('/auth/me');
    if (freshUser) {
      user = freshUser;
      setCurrentUser(freshUser);
    }
  } catch {
    // Keep local user if refresh fails.
  }

  const initials = getInitials(user.fullName_En || 'U');
  const roleBadgeClass = getRoleBadgeClass(user.role);

  container.innerHTML = `
    <div class="row g-4 justify-content-center">
      <div class="col-xl-8">
        <div class="card shadow-sm">
          <div class="card-body p-4 p-md-5">
            <div class="d-flex align-items-center gap-3 mb-4 flex-wrap">
              <div class="profile-avatar-lg">${initials}</div>
              <div>
                <h4 class="mb-1">${esc(user.fullName_En)}</h4>
                ${user.fullName_Alt ? `<p class="text-muted mb-2" lang="mr">${esc(user.fullName_Alt)}</p>` : ''}
                <span class="badge ${roleBadgeClass}">${esc(user.role)}</span>
              </div>
            </div>

            <form id="profile-form" novalidate>
              <div class="row g-3">
                <div class="col-md-6">
                  <label for="p-fullname-en" class="form-label">Full Name (English)</label>
                  <input id="p-fullname-en" class="form-control" maxlength="200" required value="${escAttr(user.fullName_En || '')}">
                </div>
                <div class="col-md-6">
                  <label for="p-fullname-alt" class="form-label">Full Name (Alternate)</label>
                  <input id="p-fullname-alt" class="form-control" maxlength="200" required value="${escAttr(user.fullName_Alt || '')}">
                </div>
                <div class="col-md-6">
                  <label for="p-mobile" class="form-label">Mobile Number</label>
                  <input id="p-mobile" class="form-control" value="${escAttr(user.mobileNumber || '')}" disabled>
                </div>
                <div class="col-md-6">
                  <label for="p-email" class="form-label">Email</label>
                  <input id="p-email" type="email" class="form-control" maxlength="200" value="${escAttr(user.email || '')}">
                </div>
              </div>

              <div class="mt-4 d-flex justify-content-end">
                <button type="submit" class="btn btn-primary" id="profile-save-btn">
                  <i class="bi bi-check2-circle me-1"></i>Save Profile
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <div class="col-xl-4">
        <div class="card shadow-sm h-100">
          <div class="card-body p-4">
            <h6 class="fw-semibold mb-3">Signature Image</h6>
            <p class="text-muted small mb-3">Upload PNG, JPG, or WebP image (max 2 MB). This signature is used in PDF approval flow.</p>

            <div class="signature-preview-box mb-3">
              ${user.signaturePath
                ? `<img id="signature-preview" src="${escAttr(user.signaturePath)}" alt="Current signature" class="signature-preview-img">`
                : `<div id="signature-empty" class="text-muted small">No signature uploaded yet.</div>`}
            </div>

            <form id="signature-form" novalidate>
              <label for="signature-file" class="form-label">Choose Signature</label>
              <input id="signature-file" type="file" class="form-control" accept="image/png,image/jpeg,image/webp" required>
              <div class="form-text">Transparent PNG is recommended.</div>

              <div class="mt-3 d-grid">
                <button type="submit" class="btn btn-outline-primary" id="signature-upload-btn">
                  <i class="bi bi-upload me-1"></i>Upload Signature
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>

    <style>
      .profile-avatar-lg {
        width: 72px;
        height: 72px;
        border-radius: 50%;
        background: var(--bs-primary);
        color: #fff;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.35rem;
        font-weight: 700;
      }
      .signature-preview-box {
        min-height: 96px;
        border: 1px dashed var(--bs-border-color);
        border-radius: 0.5rem;
        background: #fff;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 0.75rem;
      }
      .signature-preview-img {
        max-width: 100%;
        max-height: 85px;
        object-fit: contain;
      }
    </style>
  `;

  wireProfileForm(container);
  wireSignatureForm(container);
  translateDOM();
}

function wireProfileForm(container) {
  const form = container.querySelector('#profile-form');
  const saveBtn = container.querySelector('#profile-save-btn');
  if (!form || !saveBtn) return;

  form.addEventListener('submit', async (ev) => {
    ev.preventDefault();

    const fullName_En = (container.querySelector('#p-fullname-en')?.value || '').trim();
    const fullName_Alt = (container.querySelector('#p-fullname-alt')?.value || '').trim();
    const emailRaw = (container.querySelector('#p-email')?.value || '').trim();

    if (!fullName_En || !fullName_Alt) {
      showToast('Both name fields are required.', 'warning');
      return;
    }

    saveBtn.disabled = true;
    const oldHtml = saveBtn.innerHTML;
    saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>Saving...';

    try {
      const updatedUser = await api.put('/auth/me', {
        fullName_En,
        fullName_Alt,
        email: emailRaw || null,
      });

      setCurrentUser(updatedUser);
      showToast('Profile updated successfully.', 'success');
      await renderProfile(container);
    } catch (err) {
      showToast(err?.message || 'Failed to update profile.', 'danger');
    } finally {
      saveBtn.disabled = false;
      saveBtn.innerHTML = oldHtml;
    }
  });
}

function wireSignatureForm(container) {
  const form = container.querySelector('#signature-form');
  const input = container.querySelector('#signature-file');
  const btn = container.querySelector('#signature-upload-btn');
  if (!form || !input || !btn) return;

  input.addEventListener('change', () => {
    const file = input.files?.[0];
    if (!file) return;

    const existing = container.querySelector('#signature-preview');
    if (existing) existing.remove();

    const empty = container.querySelector('#signature-empty');
    if (empty) empty.remove();

    const img = document.createElement('img');
    img.id = 'signature-preview';
    img.className = 'signature-preview-img';
    img.alt = 'Signature preview';
    img.src = URL.createObjectURL(file);
    container.querySelector('.signature-preview-box')?.appendChild(img);
  });

  form.addEventListener('submit', async (ev) => {
    ev.preventDefault();

    const file = input.files?.[0];
    if (!file) {
      showToast('Please choose a signature image first.', 'warning');
      return;
    }

    const fd = new FormData();
    fd.append('file', file);

    btn.disabled = true;
    const oldHtml = btn.innerHTML;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>Uploading...';

    try {
      const updatedUser = await api.upload('/auth/me/signature', fd);
      setCurrentUser(updatedUser);
      showToast('Signature uploaded successfully.', 'success');
      await renderProfile(container);
    } catch (err) {
      showToast(err?.message || 'Failed to upload signature.', 'danger');
    } finally {
      btn.disabled = false;
      btn.innerHTML = oldHtml;
    }
  });
}

function getRoleBadgeClass(role) {
  const map = {
    Lotus: 'bg-danger',
    Commissioner: 'bg-primary',
    DeputyCommissioner: 'bg-info text-dark',
    ChiefAccountant: 'bg-success',
    CityEngineer: 'bg-warning text-dark',
    Auditor: 'bg-secondary',
    Submitter: 'bg-dark',
  };
  return map[role] || 'bg-secondary';
}

function esc(str) {
  const d = document.createElement('div');
  d.textContent = str ?? '';
  return d.innerHTML;
}

function escAttr(str) {
  return esc(str).replace(/"/g, '&quot;');
}
