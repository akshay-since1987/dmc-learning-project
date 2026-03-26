/** Lotus > Procurement Methods management page */
import { renderMasterCrud } from './master-crud.js';

export function renderProcurementMethods(container) {
  renderMasterCrud(container, {
    apiBase: '/lotus/procurement-methods',
    entityName: 'Procurement Method',
    fields: [
      { key: 'name_En', label: 'Name', type: 'text', required: true, placeholder: 'Method name in English' },
      { key: 'name_Alt', label: 'Name', type: 'text', required: true, placeholder: 'खरेदी पद्धत मराठीत' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ]
  });
}
