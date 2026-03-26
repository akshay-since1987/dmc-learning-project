/**
 * Generic Lotus master entity CRUD page.
 * Renders a data-table with add/edit/delete modals for any master entity
 * that follows the pattern: { id, name_En, name_Alt, code?, isActive }.
 */

import api from '../../api.js';
import { renderDataTable } from '../../data-table.js';
import { showModal, confirmModal } from '../../modal.js';
import { showToast } from '../../toast.js';

/**
 * @param {HTMLElement} container - content area
 * @param {object} config
 * @param {string} config.apiBase - e.g. '/lotus/departments'
 * @param {string} config.entityName - e.g. 'Department'
 * @param {Array<{key:string, label:string, type:string, required?:boolean, placeholder?:string}>} config.fields
 *   Each field: { key, label, type: 'text'|'number'|'toggle', required }
 *   The 'name_En' and 'name_Alt' fields are always present (added automatically if not specified).
 */
export function renderMasterCrud(container, config) {
  let tableRef = null;

  // Build columns from fields
  const columns = config.fields.map(f => {
    if (f.type === 'toggle') {
      return {
        key: f.key,
        label: f.label,
        render: (row) => {
          const badge = document.createElement('span');
          badge.className = row[f.key] ? 'badge bg-success' : 'badge bg-secondary';
          badge.textContent = row[f.key] ? 'Active' : 'Inactive';
          return badge;
        }
      };
    }
    return { key: f.key, label: f.label };
  });

  tableRef = renderDataTable(container, {
    columns,
    fetchData: async ({ search, pageIndex, pageSize }) => {
      const params = new URLSearchParams({ pageIndex, pageSize });
      if (search) params.set('search', search);
      return await api.get(`${config.apiBase}?${params}`);
    },
    onAdd: () => openForm(null),
    addLabel: `Add ${config.entityName}`,
    onEdit: (row) => openForm(row),
    onDelete: (row) => handleDelete(row),
  });

  async function openForm(item) {
    const isEdit = !!item;
    const formEl = document.createElement('form');
    formEl.noValidate = true;

    let html = '';
    for (const field of config.fields) {
      if (field.key === 'id') continue;

      if (field.type === 'toggle') {
        // Only show toggle in edit mode
        if (isEdit) {
          const checked = item?.[field.key] ? 'checked' : '';
          html += `
            <div class="mb-3 form-check form-switch">
              <input class="form-check-input" type="checkbox" role="switch" id="field-${field.key}" ${checked}>
              <label class="form-check-label" for="field-${field.key}">${field.label}</label>
            </div>`;
        }
        continue;
      }

      const val = isEdit ? (item?.[field.key] ?? '') : '';
      const req = field.required ? 'required' : '';
      const inputType = field.type === 'number' ? 'number' : 'text';
      const isDualEn = field.key.endsWith('_En');
      const isDualAlt = field.key.endsWith('_Alt');
      const langHint = isDualEn ? ' (English)' : isDualAlt ? ' (मराठी)' : '';

      html += `
        <div class="mb-3">
          <label for="field-${field.key}" class="form-label">${field.label}${langHint}</label>
          <input type="${inputType}" class="form-control" id="field-${field.key}"
            value="${escapeAttr(String(val))}" placeholder="${field.placeholder || ''}" ${req}
            ${isDualAlt ? 'lang="mr"' : ''}>
          <div class="invalid-feedback">This field is required.</div>
        </div>`;
    }

    formEl.innerHTML = html;

    const { close } = showModal({
      title: isEdit ? `Edit ${config.entityName}` : `Add ${config.entityName}`,
      body: formEl,
      size: 'lg',
      buttons: [
        { label: 'Cancel', className: 'btn btn-secondary', onClick: (c) => c() },
        {
          label: isEdit ? 'Save Changes' : 'Create',
          className: 'btn btn-primary',
          onClick: async (closeFn) => {
            // Validate
            if (!formEl.checkValidity()) {
              formEl.classList.add('was-validated');
              return;
            }

            // Build payload
            const payload = {};
            for (const field of config.fields) {
              if (field.key === 'id') continue;
              if (field.type === 'toggle') {
                if (isEdit) {
                  payload[field.key] = formEl.querySelector(`#field-${field.key}`)?.checked ?? false;
                }
                continue;
              }
              const input = formEl.querySelector(`#field-${field.key}`);
              let value = input?.value?.trim() ?? '';
              if (field.type === 'number') value = Number(value);
              payload[field.key] = value;
            }

            try {
              if (isEdit) {
                payload.id = item.id;
                await api.put(`${config.apiBase}/${item.id}`, payload);
                showToast(`${config.entityName} updated`, 'success');
              } else {
                await api.post(config.apiBase, payload);
                showToast(`${config.entityName} created`, 'success');
              }
              closeFn();
              tableRef.reload();
            } catch (err) {
              showToast(err.message || 'Operation failed', 'danger');
            }
          }
        }
      ]
    });
  }

  async function handleDelete(item) {
    const name = item.name_En || item.fullName_En || item.number || 'this item';
    const ok = await confirmModal('Delete Confirmation', `Are you sure you want to delete "${name}"? This action cannot be undone.`);
    if (!ok) return;

    try {
      await api.delete(`${config.apiBase}/${item.id}`);
      showToast(`${config.entityName} deleted`, 'success');
      tableRef.reload();
    } catch (err) {
      showToast(err.message || 'Delete failed', 'danger');
    }
  }
}

function escapeAttr(str) {
  return str.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}
