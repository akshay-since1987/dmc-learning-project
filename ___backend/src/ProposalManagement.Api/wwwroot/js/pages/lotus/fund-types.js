/** Lotus > Fund Types management page */
import { renderMasterCrud } from './master-crud.js';

export function renderFundTypes(container) {
  renderMasterCrud(container, {
    apiBase: '/lotus/fund-types',
    entityName: 'Fund Type',
    fields: [
      { key: 'name_En', label: 'Name', type: 'text', required: true, placeholder: 'Fund type in English' },
      { key: 'name_Alt', label: 'Name', type: 'text', required: true, placeholder: 'निधी प्रकार मराठीत' },
      { key: 'code', label: 'Code', type: 'text', required: true, placeholder: 'e.g. GF' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ]
  });
}
