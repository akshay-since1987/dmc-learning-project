/** Lotus > Departments management page */
import { renderMasterCrud } from './master-crud.js';

export function renderDepartments(container) {
  renderMasterCrud(container, {
    apiBase: '/lotus/departments',
    entityName: 'Department',
    fields: [
      { key: 'name_En', label: 'Name', type: 'text', required: true, placeholder: 'Department name in English' },
      { key: 'name_Alt', label: 'Name', type: 'text', required: true, placeholder: 'विभागाचे नाव मराठीत' },
      { key: 'code', label: 'Code', type: 'text', required: true, placeholder: 'e.g. PWD' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ]
  });
}
