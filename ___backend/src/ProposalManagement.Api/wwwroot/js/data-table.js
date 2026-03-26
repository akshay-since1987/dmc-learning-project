/** Generic paginated, sortable, searchable data table component */

import { debounce, createElement } from './utils.js';
import { t } from './i18n.js';

/**
 * Render a data table into a container.
 * @param {HTMLElement} container - Target element
 * @param {object} config
 * @param {Array<{key:string, label:string, render?:(row)=>string|HTMLElement}>} config.columns
 * @param {(params:{search:string,pageIndex:number,pageSize:number})=>Promise<{items:Array,totalCount:number}>} config.fetchData
 * @param {((row)=>void)|null} config.onEdit
 * @param {((row)=>void)|null} config.onDelete
 * @param {(()=>void)|null} config.onAdd
 * @param {string} config.addLabel
 */
export function renderDataTable(container, config) {
  const state = { search: '', pageIndex: 1, pageSize: 10, totalCount: 0, items: [] };

  container.innerHTML = `
    <div class="card">
      <div class="card-header d-flex align-items-center justify-content-between flex-wrap gap-2">
        <div class="d-flex align-items-center gap-2">
          <div class="input-group input-group-sm" style="max-width:300px">
            <span class="input-group-text"><i class="bi bi-search"></i></span>
            <input type="search" class="form-control" id="dt-search"
              placeholder="Search..." aria-label="Search">
          </div>
        </div>
        <div class="d-flex gap-2" id="dt-actions"></div>
      </div>
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-hover align-middle mb-0">
            <thead class="table-light">
              <tr id="dt-header"></tr>
            </thead>
            <tbody id="dt-body"></tbody>
          </table>
        </div>
      </div>
      <div class="card-footer d-flex align-items-center justify-content-between flex-wrap gap-2">
        <small class="text-muted" id="dt-info"></small>
        <nav aria-label="Table pagination">
          <ul class="pagination pagination-sm mb-0" id="dt-pagination"></ul>
        </nav>
      </div>
    </div>
  `;

  // Header
  const headerRow = container.querySelector('#dt-header');
  for (const col of config.columns) {
    const th = document.createElement('th');
    th.scope = 'col';
    th.textContent = col.label;
    headerRow.appendChild(th);
  }
  if (config.onEdit || config.onDelete) {
    const th = document.createElement('th');
    th.scope = 'col';
    th.textContent = 'Actions';
    th.style.width = '120px';
    headerRow.appendChild(th);
  }

  // Add button
  if (config.onAdd) {
    const actionsEl = container.querySelector('#dt-actions');
    const btn = document.createElement('button');
    btn.className = 'btn btn-primary btn-sm d-flex align-items-center gap-1';
    btn.innerHTML = `<i class="bi bi-plus-lg"></i>`;
    const span = document.createElement('span');
    span.textContent = config.addLabel || 'Add New';
    btn.appendChild(span);
    btn.addEventListener('click', config.onAdd);
    actionsEl.appendChild(btn);
  }

  // Search
  const searchInput = container.querySelector('#dt-search');
  searchInput.addEventListener('input', debounce(() => {
    state.search = searchInput.value;
    state.pageIndex = 1;
    loadData();
  }, 300));

  // Load data function
  async function loadData() {
    const tbody = container.querySelector('#dt-body');
    tbody.innerHTML = `<tr><td colspan="${config.columns.length + 1}" class="text-center py-4">
      <div class="spinner-border spinner-border-sm text-primary me-2"></div>Loading...
    </td></tr>`;

    try {
      const result = await config.fetchData({
        search: state.search,
        pageIndex: state.pageIndex,
        pageSize: state.pageSize
      });

      state.items = result.items || [];
      state.totalCount = result.totalCount || 0;
      renderRows();
      renderPagination();
      renderInfo();
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="${config.columns.length + 1}" class="text-center text-danger py-4">
        Failed to load data: ${err.message || 'Unknown error'}
      </td></tr>`;
    }
  }

  function renderRows() {
    const tbody = container.querySelector('#dt-body');
    tbody.innerHTML = '';

    if (state.items.length === 0) {
      tbody.innerHTML = `<tr><td colspan="${config.columns.length + 1}" class="text-center py-4 text-muted">
        <i class="bi bi-inbox d-block mb-2" style="font-size:2rem;opacity:0.3"></i>
        No records found
      </td></tr>`;
      return;
    }

    for (const item of state.items) {
      const tr = document.createElement('tr');

      for (const col of config.columns) {
        const td = document.createElement('td');
        if (col.render) {
          const content = col.render(item);
          if (typeof content === 'string') td.textContent = content;
          else if (content instanceof HTMLElement) td.appendChild(content);
          else td.textContent = content?.toString() || '';
        } else {
          td.textContent = item[col.key] ?? '';
        }
        tr.appendChild(td);
      }

      // Action buttons
      if (config.onEdit || config.onDelete) {
        const td = document.createElement('td');
        const group = document.createElement('div');
        group.className = 'btn-group btn-group-sm';

        if (config.onEdit) {
          const editBtn = document.createElement('button');
          editBtn.className = 'btn btn-outline-primary';
          editBtn.title = 'Edit';
          editBtn.innerHTML = '<i class="bi bi-pencil"></i>';
          editBtn.addEventListener('click', () => config.onEdit(item));
          group.appendChild(editBtn);
        }

        if (config.onDelete) {
          const delBtn = document.createElement('button');
          delBtn.className = 'btn btn-outline-danger';
          delBtn.title = 'Delete';
          delBtn.innerHTML = '<i class="bi bi-trash"></i>';
          delBtn.addEventListener('click', () => config.onDelete(item));
          group.appendChild(delBtn);
        }

        td.appendChild(group);
        tr.appendChild(td);
      }

      tbody.appendChild(tr);
    }
  }

  function renderPagination() {
    const pag = container.querySelector('#dt-pagination');
    pag.innerHTML = '';
    const totalPages = Math.ceil(state.totalCount / state.pageSize) || 1;

    // Prev
    const prevLi = document.createElement('li');
    prevLi.className = `page-item ${state.pageIndex <= 1 ? 'disabled' : ''}`;
    const prevA = document.createElement('button');
    prevA.className = 'page-link';
    prevA.textContent = '‹';
    prevA.setAttribute('aria-label', 'Previous');
    prevA.addEventListener('click', () => { if (state.pageIndex > 1) { state.pageIndex--; loadData(); } });
    prevLi.appendChild(prevA);
    pag.appendChild(prevLi);

    // Pages (show max 5)
    const start = Math.max(1, state.pageIndex - 2);
    const end = Math.min(totalPages, start + 4);
    for (let i = start; i <= end; i++) {
      const li = document.createElement('li');
      li.className = `page-item ${i === state.pageIndex ? 'active' : ''}`;
      const a = document.createElement('button');
      a.className = 'page-link';
      a.textContent = i.toString();
      a.addEventListener('click', () => { state.pageIndex = i; loadData(); });
      li.appendChild(a);
      pag.appendChild(li);
    }

    // Next
    const nextLi = document.createElement('li');
    nextLi.className = `page-item ${state.pageIndex >= totalPages ? 'disabled' : ''}`;
    const nextA = document.createElement('button');
    nextA.className = 'page-link';
    nextA.textContent = '›';
    nextA.setAttribute('aria-label', 'Next');
    nextA.addEventListener('click', () => { if (state.pageIndex < totalPages) { state.pageIndex++; loadData(); } });
    nextLi.appendChild(nextA);
    pag.appendChild(nextLi);
  }

  function renderInfo() {
    const infoEl = container.querySelector('#dt-info');
    const from = (state.pageIndex - 1) * state.pageSize + 1;
    const to = Math.min(state.pageIndex * state.pageSize, state.totalCount);
    infoEl.textContent = state.totalCount > 0
      ? `Showing ${from}–${to} of ${state.totalCount}`
      : 'No records';
  }

  // Initial load
  loadData();

  // Return a refresh function
  return { reload: loadData };
}
