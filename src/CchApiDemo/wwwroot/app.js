const steps = [...document.querySelectorAll('[data-step]')];
const result = document.querySelector('#result');
const message = document.querySelector('#message');
const title = document.querySelector('#result-title');
const duration = document.querySelector('#duration');
const count = document.querySelector('#step-count');
const returnStatusSelect = document.querySelector('#return-status');
const returnsTable = document.querySelector('#returns-table');
let availableStatuses = [];

function setSteps(done, active) {
  steps.forEach((step) => {
    step.classList.toggle('done', done.includes(step.dataset.step));
    step.classList.toggle('active', step.dataset.step === active);
  });
  count.textContent = `${done.length} / ${steps.length}`;
}

function showResult(label, data, elapsed) {
  title.textContent = label;
  duration.textContent = `${elapsed} ms`;
  returnsTable.hidden = true;
  result.hidden = false;
  result.textContent = JSON.stringify(data, null, 2);
}

function renderReturns(data) {
  const returns = Array.isArray(data.Returns) ? data.Returns : [];
  returnsTable.replaceChildren();
  returnsTable.hidden = false;
  result.hidden = true;

  const heading = document.createElement('div');
  heading.className = 'panel-heading';
  heading.innerHTML = '<div><p class="eyebrow">Return manager view</p><h2>Choose a status per return</h2></div>';
  returnsTable.append(heading);

  if (returns.length === 0) {
    const empty = document.createElement('p');
    empty.className = 'table-empty';
    empty.textContent = 'No returns matched the selected tax year and return type.';
    returnsTable.append(empty);
    return;
  }

  const table = document.createElement('table');
  table.innerHTML = '<thead><tr><th>Return ID</th><th>Client</th><th>Tax year</th><th>Current status</th><th>New status</th><th></th></tr></thead>';
  const body = document.createElement('tbody');
  returns.forEach((taxReturn) => {
    const row = document.createElement('tr');
    row.dataset.returnId = taxReturn.ReturnID ?? '';
    row.innerHTML = `<td><strong>${escapeHtml(taxReturn.ReturnID ?? '')}</strong></td><td>${escapeHtml(taxReturn.ClientName ?? '')}</td><td>${escapeHtml(taxReturn.TaxYear ?? '')}</td><td class="current-status">${escapeHtml(taxReturn.ReturnStatus ?? '')}</td>`;
    const selectCell = document.createElement('td');
    const select = document.createElement('select');
    select.className = 'row-status';
    availableStatuses.forEach((status) => select.add(new Option(status.Name, status.Name)));
    select.value = taxReturn.ReturnStatus ?? '';
    selectCell.append(select);
    row.append(selectCell);
    const actionCell = document.createElement('td');
    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'row-action';
    button.dataset.returnId = taxReturn.ReturnID ?? '';
    button.textContent = 'Apply';
    actionCell.append(button);
    row.append(actionCell);
    body.append(row);
  });
  table.append(body);
  returnsTable.append(table);
}

function escapeHtml(value) {
  return String(value).replace(/[&<>'"]/g, (character) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character]));
}

async function callApi(url, options = {}) {
  const response = await fetch(url, { headers: { 'Content-Type': 'application/json' }, ...options });
  const data = await response.json().catch(() => ({ detail: response.statusText }));
  if (!response.ok) {
    const error = new Error(data.detail || data.title || 'Request failed');
    error.status = response.status;
    error.response = data;
    throw error;
  }
  return data;
}

async function loadReturnStatuses() {
  availableStatuses = await callApi('/api/tax/return-status');
  if (returnStatusSelect) {
    returnStatusSelect.replaceChildren(new Option('Choose a status', ''));
    availableStatuses.forEach((status) => returnStatusSelect.add(new Option(status.Name, status.Name)));
  }
}

document.querySelector('#workflow-form').addEventListener('submit', async (event) => {
  event.preventDefault();
  const started = performance.now();
  const clientId = document.querySelector('#client-id').value.trim();
  const clientSubId = document.querySelector('#client-sub-id').value.trim();
  setSteps([], 'authenticate');
  message.className = 'message';
  message.textContent = 'Running authentication, client lookup, and tax status lookup...';
  try {
    const data = await callApi('/api/workflow', { method: 'POST', body: JSON.stringify({ clientId, clientSubId }) });
    setSteps(['authenticate', 'client', 'tax'], null);
    message.textContent = 'The workflow completed successfully. The response below is the combined result.';
    showResult('Workflow complete', data, Math.round(performance.now() - started));
  } catch (error) {
    message.className = 'message error';
    message.textContent = error.message;
    showResult('Workflow stopped', { error: error.message }, Math.round(performance.now() - started));
  }
});

document.querySelector('#authenticate').addEventListener('click', async () => {
  const started = performance.now();
  setSteps([], 'authenticate');
  message.className = 'message';
  message.textContent = 'Authenticating on the server...';
  try {
    const data = await callApi('/api/authenticate', { method: 'POST' });
    setSteps(['authenticate'], 'client');
    message.textContent = data.Message;
    showResult('Authentication complete', data, Math.round(performance.now() - started));
    await loadReturnStatuses();
  } catch (error) {
    message.className = 'message error';
    message.textContent = `Authentication failed (${error.status || 'client error'}): ${error.message}`;
    showResult('Authentication failed', error.response || { error: error.message }, Math.round(performance.now() - started));
  }
});

document.querySelector('#find-returns').addEventListener('click', async () => {
  const started = performance.now();
  const taxYear = document.querySelector('#tax-year').value.trim();
  const returnType = document.querySelector('#return-type').value;
  message.className = 'message';
  message.textContent = 'Finding returns from Tax Services v2...';
  try {
    const query = new URLSearchParams({ taxYear, returnType });
    const data = await callApi(`/api/tax/returns?${query}`);
    const total = data.TotalCount ?? data.Returns?.length ?? 0;
    message.textContent = `Found ${total} return${total === 1 ? '' : 's'} for ${taxYear} / ${returnType}.`;
    showResult('Returns found', data, Math.round(performance.now() - started));
    renderReturns(data);
  } catch (error) {
    message.className = 'message error';
    message.textContent = `Return lookup failed (${error.status || 'client error'}): ${error.message}`;
    showResult('Return lookup failed', error.response || { error: error.message }, Math.round(performance.now() - started));
  }
});

returnsTable.addEventListener('click', async (event) => {
  const button = event.target.closest('.row-action');
  if (!button) return;
  const row = button.closest('tr');
  const status = row.querySelector('.row-status').value;
  const returnId = button.dataset.returnId;
  const started = performance.now();
  button.disabled = true;
  message.className = 'message';
  message.textContent = `Assigning ${status} to ${returnId}...`;
  try {
    const data = await callApi('/api/tax/return-status', {
      method: 'POST',
      body: JSON.stringify({ ReturnId: [returnId], Status: status })
    });
    row.querySelector('.current-status').textContent = status;
    message.textContent = `Assigned ${status} to ${returnId}.`;
    showResult('Status assigned', data, Math.round(performance.now() - started));
    returnsTable.hidden = false;
    result.hidden = true;
  } catch (error) {
    message.className = 'message error';
    message.textContent = `Status assignment failed (${error.status || 'client error'}): ${error.message}`;
    showResult('Status assignment failed', error.response || { error: error.message }, Math.round(performance.now() - started));
  } finally {
    button.disabled = false;
  }
});

fetch('/api/config').then((response) => response.json()).then((config) => {
  const ready = config.HasIntegratorKey && config.HasCredentials;
  const status = document.querySelector('#env-status');
  status.textContent = ready ? 'Environment ready' : 'Complete .env first';
  status.style.color = ready ? 'var(--green)' : 'var(--orange)';
}).catch(() => { document.querySelector('#env-status').textContent = 'Server unavailable'; });
