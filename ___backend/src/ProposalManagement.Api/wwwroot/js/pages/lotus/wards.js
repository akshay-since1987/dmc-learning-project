/** Lotus > Wards management page */
import { renderMasterCrud } from './master-crud.js';

export function renderWards(container) {
  renderMasterCrud(container, {
    apiBase: '/lotus/wards',
    entityName: 'Ward',
    fields: [
      { key: 'number', label: 'Ward Number', type: 'number', required: true, placeholder: 'e.g. 1' },
      { key: 'name_En', label: 'Name', type: 'text', required: true, placeholder: 'Ward name in English' },
      { key: 'name_Alt', label: 'Name', type: 'text', required: true, placeholder: 'प्रभाग नाव मराठीत' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ]
  });
}
