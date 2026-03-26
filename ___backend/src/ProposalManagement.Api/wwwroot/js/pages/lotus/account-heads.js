/** Lotus > Account Heads management page */
import { renderMasterCrud } from './master-crud.js';

export function renderAccountHeads(container) {
  renderMasterCrud(container, {
    apiBase: '/lotus/account-heads',
    entityName: 'Account Head',
    fields: [
      { key: 'name_En', label: 'Name', type: 'text', required: true, placeholder: 'Account head in English' },
      { key: 'name_Alt', label: 'Name', type: 'text', required: true, placeholder: 'लेखा शीर्ष मराठीत' },
      { key: 'code', label: 'Code', type: 'text', required: true, placeholder: 'e.g. 3054' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ]
  });
}
