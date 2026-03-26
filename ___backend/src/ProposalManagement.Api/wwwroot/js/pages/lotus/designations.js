/** Lotus > Designations management page */
import { renderMasterCrud } from './master-crud.js';

export function renderDesignations(container) {
  renderMasterCrud(container, {
    apiBase: '/lotus/designations',
    entityName: 'Designation',
    fields: [
      { key: 'name_En', label: 'Name', type: 'text', required: true, placeholder: 'Designation in English' },
      { key: 'name_Alt', label: 'Name', type: 'text', required: true, placeholder: 'पदनाम मराठीत' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ]
  });
}
