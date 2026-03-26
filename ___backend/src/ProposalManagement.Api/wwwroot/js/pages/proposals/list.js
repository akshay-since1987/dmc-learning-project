/** Proposals list page — works for "My Proposals", "All Proposals", and "Pending Approvals" */
import api from '../../api.js';
import { renderDataTable } from '../../data-table.js';
import { showToast } from '../../toast.js';
import { confirmModal } from '../../modal.js';
import { navigate } from '../../router.js';
import { getUserRole } from '../../auth.js';

const STAGE_BADGES = {
  Draft: 'bg-secondary',
  Submitted: 'bg-info',
  AtCityEngineer: 'bg-primary',
  AtChiefAccountant: 'bg-warning text-dark',
  AtDeputyCommissioner: 'bg-info',
  AtCommissioner: 'bg-dark',
  PushedBack: 'bg-danger',
  Approved: 'bg-success',
  Cancelled: 'bg-secondary',
};

const STAGE_LABELS = {
  Draft: 'Draft',
  Submitted: 'Submitted',
  AtCityEngineer: 'City Engineer',
  AtChiefAccountant: 'Chief Accountant',
  AtDeputyCommissioner: 'Dy. Commissioner',
  AtCommissioner: 'Commissioner',
  PushedBack: 'Pushed Back',
  Approved: 'Approved',
  Cancelled: 'Cancelled',
};

/**
 * @param {HTMLElement} container
 * @param {'my'|'all'|'pending'} mode
 */
export function renderProposalList(container, mode = 'my') {
  const role = getUserRole();
  const canCreate = role === 'Submitter' || role === 'Lotus';

  const columns = [
    { key: 'proposalNumber', label: 'Proposal No.' },
    { key: 'date', label: 'Date', render: (r) => formatDate(r.date) },
    { key: 'subject_En', label: 'Subject' },
    { key: 'departmentName_En', label: 'Department' },
    {
      key: 'currentStage', label: 'Stage',
      render: (r) => {
        const badge = document.createElement('span');
        badge.className = `badge ${STAGE_BADGES[r.currentStage] || 'bg-secondary'}`;
        badge.textContent = STAGE_LABELS[r.currentStage] || r.currentStage;
        return badge;
      }
    },
    {
      key: 'estimatedCost', label: 'Est. Cost',
      render: (r) => `₹ ${Number(r.estimatedCost).toLocaleString('en-IN')}`
    },
  ];

  if (mode === 'all') {
    columns.splice(3, 0, { key: 'submittedByName_En', label: 'Submitted By' });
  }

  const apiPath = mode === 'my' ? '/proposals/my'
    : mode === 'pending' ? '/proposals/pending'
    : '/proposals';

  const tableRef = renderDataTable(container, {
    columns,
    fetchData: async ({ search, pageIndex, pageSize }) => {
      const params = new URLSearchParams({ pageIndex, pageSize });
      if (search) params.set('search', search);
      return await api.get(`${apiPath}?${params}`);
    },
    onAdd: canCreate ? () => navigate('/proposals/new') : null,
    addLabel: 'New Proposal',
    onEdit: (row) => {
      // Pending approvals → approval console
      if (mode === 'pending') {
        navigate(`/proposals/${row.id}/approve`);
        return;
      }
      // Navigate to detail or edit based on stage
      if ((row.currentStage === 'Draft' || row.currentStage === 'PushedBack') && mode === 'my') {
        navigate(`/proposals/${row.id}/edit`);
      } else {
        navigate(`/proposals/${row.id}`);
      }
    },
    onDelete: (mode === 'my') ? async (row) => {
      if (row.currentStage !== 'Draft') {
        showToast('Only draft proposals can be deleted', 'warning');
        return;
      }
      const ok = await confirmModal('Delete Proposal', `Delete proposal "${row.proposalNumber}"?`);
      if (!ok) return;
      try {
        await api.delete(`/proposals/${row.id}`);
        showToast('Proposal deleted', 'success');
        tableRef.reload();
      } catch (err) {
        showToast(err.message || 'Delete failed', 'danger');
      }
    } : null,
  });
}

function formatDate(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
}
