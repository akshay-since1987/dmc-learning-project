/** Lotus > Users management page */
import api from '../../api.js';
import { renderDataTable } from '../../data-table.js';
import { showModal, confirmModal } from '../../modal.js';
import { showToast } from '../../toast.js';

const ROLES = ['Submitter', 'CityEngineer', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner', 'Auditor', 'Lotus'];

let departmentsCache = null;
let designationsCache = null;

async function loadDepartments() {
  if (departmentsCache) return departmentsCache;
  const data = await api.get('/lotus/departments?pageSize=200');
  departmentsCache = data.items || [];
  return departmentsCache;
}

async function loadDesignations() {
  if (designationsCache) return designationsCache;
  const data = await api.get('/lotus/designations?pageSize=200');
  designationsCache = data.items || [];
  return designationsCache;
}

export function renderUsers(container) {
  let tableRef = null;

  const columns = [
    { key: 'fullName_En', label: 'Name (EN)' },
    { key: 'fullName_Alt', label: 'Name (Alt)' },
    { key: 'mobileNumber', label: 'Mobile' },
    {
      key: 'role', label: 'Role',
      render: (row) => {
        const badge = document.createElement('span');
        const roleBg = {
          Lotus: 'bg-danger', Commissioner: 'bg-primary', DeputyCommissioner: 'bg-info',
          ChiefAccountant: 'bg-warning text-dark', CityEngineer: 'bg-secondary',
          Submitter: 'bg-success', Auditor: 'bg-dark'
        };
        badge.className = `badge ${roleBg[row.role] || 'bg-secondary'}`;
        badge.textContent = row.role;
        return badge;
      }
    },
    { key: 'departmentName_En', label: 'Department' },
    {
      key: 'signaturePath', label: 'Signature',
      render: (row) => {
        if (!row.signaturePath) {
          const span = document.createElement('span');
          span.className = 'badge bg-warning text-dark';
          span.textContent = 'Missing';
          return span;
        }
        const img = document.createElement('img');
        img.src = row.signaturePath;
        img.alt = 'Signature';
        img.style.cssText = 'max-height:32px;max-width:80px;object-fit:contain;border:1px solid #dee2e6;border-radius:4px;background:#fff;padding:2px;';
        return img;
      }
    },
    {
      key: 'isActive', label: 'Status',
      render: (row) => {
        const badge = document.createElement('span');
        badge.className = row.isActive ? 'badge bg-success' : 'badge bg-secondary';
        badge.textContent = row.isActive ? 'Active' : 'Inactive';
        return badge;
      }
    },
  ];

  tableRef = renderDataTable(container, {
    columns,
    fetchData: async ({ search, pageIndex, pageSize }) => {
      const params = new URLSearchParams({ pageIndex, pageSize });
      if (search) params.set('search', search);
      return await api.get(`/lotus/users?${params}`);
    },
    onAdd: () => openUserForm(null),
    addLabel: 'Add User',
    onEdit: (row) => openUserForm(row),
    onDelete: (row) => handleDelete(row),
  });

  async function openUserForm(user) {
    const isEdit = !!user;
    const [departments, designations] = await Promise.all([loadDepartments(), loadDesignations()]);

    const formEl = document.createElement('form');
    formEl.noValidate = true;

    const deptOptions = departments.map(d =>
      `<option value="${d.id}" ${user?.departmentId === d.id ? 'selected' : ''}>${escapeHtml(d.name_En)}</option>`
    ).join('');

    const desigOptions = designations.map(d =>
      `<option value="${d.id}" ${user?.designationId === d.id ? 'selected' : ''}>${escapeHtml(d.name_En)}</option>`
    ).join('');

    const roleOptions = ROLES.map(r =>
      `<option value="${r}" ${user?.role === r ? 'selected' : ''}>${r}</option>`
    ).join('');

    formEl.innerHTML = `
      <div class="row g-3">
        <div class="col-md-6">
          <label for="u-fullNameEn" class="form-label">Full Name (English) <span class="text-danger">*</span></label>
          <input type="text" class="form-control" id="u-fullNameEn" required maxlength="200"
            value="${escapeAttr(user?.fullName_En || '')}" placeholder="Full name in English">
          <div class="invalid-feedback">Full name (English) is required.</div>
        </div>
        <div class="col-md-6">
          <label for="u-fullNameAlt" class="form-label">Full Name (मराठी) <span class="text-danger">*</span></label>
          <input type="text" class="form-control" id="u-fullNameAlt" required maxlength="200" lang="mr"
            value="${escapeAttr(user?.fullName_Alt || '')}" placeholder="पूर्ण नाव मराठीत">
          <div class="invalid-feedback">Full name (Alt) is required.</div>
        </div>
        <div class="col-md-6">
          <label for="u-mobile" class="form-label">Mobile Number <span class="text-danger">*</span></label>
          <input type="tel" class="form-control" id="u-mobile" required pattern="\\d{10}" maxlength="10"
            value="${escapeAttr(user?.mobileNumber || '')}" placeholder="10-digit mobile">
          <div class="invalid-feedback">Valid 10-digit mobile number is required.</div>
        </div>
        <div class="col-md-6">
          <label for="u-email" class="form-label">Email</label>
          <input type="email" class="form-control" id="u-email" maxlength="200"
            value="${escapeAttr(user?.email || '')}" placeholder="user@example.com">
        </div>
        <div class="col-md-4">
          <label for="u-role" class="form-label">Role <span class="text-danger">*</span></label>
          <select class="form-select" id="u-role" required>
            <option value="">Select role...</option>
            ${roleOptions}
          </select>
          <div class="invalid-feedback">Role is required.</div>
        </div>
        <div class="col-md-4">
          <label for="u-dept" class="form-label">Department</label>
          <select class="form-select" id="u-dept">
            <option value="">None</option>
            ${deptOptions}
          </select>
        </div>
        <div class="col-md-4">
          <label for="u-desig" class="form-label">Designation</label>
          <select class="form-select" id="u-desig">
            <option value="">None</option>
            ${desigOptions}
          </select>
        </div>
        <div class="col-md-6" id="password-group" style="display:none">
          <label for="u-password" class="form-label">Password ${isEdit ? '(leave blank to keep)' : ''} <span class="text-danger" id="pwd-required-star">*</span></label>
          <input type="password" class="form-control" id="u-password" minlength="6" placeholder="Min 6 characters">
          <div class="invalid-feedback">Password is required for Lotus users (min 6 chars).</div>
        </div>
        <div class="col-md-6">
          <label for="u-signature" class="form-label">Signature Image <span class="text-danger">*</span></label>
          <input type="file" class="form-control" id="u-signature" accept="image/png,image/jpeg,image/webp" ${!isEdit ? 'required' : ''}>
          <div class="form-text">PNG, JPEG, or WebP — max 2 MB</div>
          <div class="invalid-feedback">Signature image is required.</div>
          ${user?.signaturePath ? `
          <div class="mt-2" id="sig-preview-container">
            <label class="form-label text-muted small">Current Signature:</label>
            <img src="${escapeAttr(user.signaturePath)}" alt="Current signature" class="d-block border rounded" style="max-height:80px;max-width:200px;object-fit:contain;background:#fff;padding:4px;">
          </div>` : ''}
        </div>
        ${isEdit ? `
        <div class="col-md-6">
          <div class="form-check form-switch mt-4">
            <input class="form-check-input" type="checkbox" role="switch" id="u-active" ${user?.isActive ? 'checked' : ''}>
            <label class="form-check-label" for="u-active">Active</label>
          </div>
        </div>` : ''}
      </div>
    `;

    // Show/hide password field based on role
    const roleSelect = formEl.querySelector('#u-role');
    const pwdGroup = formEl.querySelector('#password-group');
    const pwdInput = formEl.querySelector('#u-password');
    const pwdStar = formEl.querySelector('#pwd-required-star');
    const sigInput = formEl.querySelector('#u-signature');

    // Live preview for signature file
    sigInput.addEventListener('change', () => {
      const existing = formEl.querySelector('#sig-live-preview');
      if (existing) existing.remove();
      const file = sigInput.files[0];
      if (!file) return;
      const preview = document.createElement('div');
      preview.id = 'sig-live-preview';
      preview.className = 'mt-2';
      const img = document.createElement('img');
      img.alt = 'Signature preview';
      img.className = 'd-block border rounded';
      img.style.cssText = 'max-height:80px;max-width:200px;object-fit:contain;background:#fff;padding:4px;';
      img.src = URL.createObjectURL(file);
      preview.appendChild(img);
      sigInput.parentElement.appendChild(preview);
    });

    function togglePassword() {
      const isLotus = roleSelect.value === 'Lotus';
      pwdGroup.style.display = isLotus ? '' : 'none';
      if (isLotus && !isEdit) {
        pwdInput.required = true;
        pwdStar.style.display = '';
      } else {
        pwdInput.required = false;
        pwdStar.style.display = 'none';
      }
    }
    roleSelect.addEventListener('change', togglePassword);
    togglePassword();

    showModal({
      title: isEdit ? 'Edit User' : 'Add User',
      body: formEl,
      size: 'lg',
      buttons: [
        { label: 'Cancel', className: 'btn btn-secondary', onClick: (c) => c() },
        {
          label: isEdit ? 'Save Changes' : 'Create User',
          className: 'btn btn-primary',
          onClick: async (closeFn) => {
            if (!formEl.checkValidity()) {
              formEl.classList.add('was-validated');
              return;
            }

            const payload = {
              fullName_En: formEl.querySelector('#u-fullNameEn').value.trim(),
              fullName_Alt: formEl.querySelector('#u-fullNameAlt').value.trim(),
              mobileNumber: formEl.querySelector('#u-mobile').value.trim(),
              email: formEl.querySelector('#u-email').value.trim() || null,
              role: roleSelect.value,
              departmentId: formEl.querySelector('#u-dept').value || null,
              designationId: formEl.querySelector('#u-desig').value || null,
              password: pwdInput.value || null,
            };

            const sigFile = sigInput.files[0];
            // Signature required on create
            if (!isEdit && !sigFile) {
              sigInput.classList.add('is-invalid');
              return;
            }

            try {
              let userId;
              if (isEdit) {
                payload.id = user.id;
                payload.isActive = formEl.querySelector('#u-active')?.checked ?? true;
                await api.put(`/lotus/users/${user.id}`, payload);
                userId = user.id;
              } else {
                const created = await api.post('/lotus/users', payload);
                userId = created.id;
              }

              // Upload signature if file selected
              if (sigFile) {
                const fd = new FormData();
                fd.append('file', sigFile);
                await api.post(`/lotus/users/${userId}/signature`, fd);
              }

              showToast(isEdit ? 'User updated' : 'User created', 'success');
              closeFn();
              departmentsCache = null; // bust cache
              designationsCache = null;
              tableRef.reload();
            } catch (err) {
              showToast(err.message || 'Operation failed', 'danger');
            }
          }
        }
      ]
    });
  }

  async function handleDelete(user) {
    const ok = await confirmModal('Delete User', `Are you sure you want to delete "${user.fullName_En}"?`);
    if (!ok) return;

    try {
      await api.delete(`/lotus/users/${user.id}`);
      showToast('User deleted', 'success');
      tableRef.reload();
    } catch (err) {
      showToast(err.message || 'Delete failed', 'danger');
    }
  }
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str || '';
  return div.innerHTML;
}

function escapeAttr(str) {
  return (str || '').replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}
