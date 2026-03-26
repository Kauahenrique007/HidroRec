import { initializeShell } from '../../app.js';
import { CONFIG } from '../../config.js';
import { adminService } from '../../services/adminService.js';
import { renderLoadingBlock } from '../../components/LoadingSpinner.js';
import { formatDateTime, formatList } from '../../utils/formatters.js';
import { renderErrorState } from '../../utils/helpers.js';

async function initAdminPage() {
  const user = await initializeShell('admin');
  const summaryTarget = document.getElementById('admin-summary');
  const usersTarget = document.getElementById('admin-users');
  const auditTarget = document.getElementById('admin-audit');
  const runsTarget = document.getElementById('admin-runs');

  const loading = renderLoadingBlock('Carregando governanca...');
  summaryTarget.innerHTML = loading;
  usersTarget.innerHTML = `<tr><td colspan="5">${loading}</td></tr>`;
  auditTarget.innerHTML = `<tr><td colspan="4">${loading}</td></tr>`;
  runsTarget.innerHTML = `<tr><td colspan="3">${loading}</td></tr>`;

  if (!user) {
    const failure = renderErrorState('Entre como administrador na area Operacao para acessar governanca.');
    summaryTarget.innerHTML = failure;
    usersTarget.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
    auditTarget.innerHTML = `<tr><td colspan="4">${failure}</td></tr>`;
    runsTarget.innerHTML = `<tr><td colspan="3">${failure}</td></tr>`;
    return;
  }

  try {
    const overview = await adminService.getOverview();

    summaryTarget.innerHTML = `
      <article class="stat-card">
        <span class="stat-card__label">Usuarios ativos</span>
        <strong class="stat-card__value">${overview.summary.activeUsers}</strong>
      </article>
      <article class="stat-card">
        <span class="stat-card__label">Ocorrencias catalogadas</span>
        <strong class="stat-card__value">${overview.summary.incidentVolume}</strong>
      </article>
      <article class="stat-card">
        <span class="stat-card__label">Entradas de auditoria</span>
        <strong class="stat-card__value">${overview.summary.auditEntries}</strong>
      </article>
      <article class="stat-card">
        <span class="stat-card__label">Runs degradados</span>
        <strong class="stat-card__value">${overview.summary.degradedRuns}</strong>
      </article>
    `;

    usersTarget.innerHTML = overview.users.map((item) => `
      <tr>
        <td>${item.name}</td>
        <td>${item.email}</td>
        <td>${item.role}</td>
        <td>${formatList(item.permissions)}</td>
        <td>${item.lastLoginAt ? formatDateTime(item.lastLoginAt) : 'Nunca'}</td>
      </tr>
    `).join('');

    auditTarget.innerHTML = overview.auditLogs.length
      ? overview.auditLogs.map((item) => `
          <tr>
            <td>${item.action}</td>
            <td>${item.actorId || '--'}</td>
            <td>${item.resource || '--'}</td>
            <td>${formatDateTime(item.timestamp)}</td>
          </tr>
        `).join('')
      : '<tr><td colspan="4">Sem eventos recentes de auditoria.</td></tr>';

    runsTarget.innerHTML = overview.integrationRuns.map((item) => `
      <tr>
        <td>${item.source}</td>
        <td>${item.status}</td>
        <td>${formatDateTime(item.collectedAt)}</td>
      </tr>
    `).join('');

    document.getElementById('docs-link').setAttribute('href', `${CONFIG.APP_ORIGIN}${overview.docs.openApiPath}`);
    document.getElementById('status-link').setAttribute('href', `${CONFIG.APP_ORIGIN}${overview.docs.apiStatusPath}`);
  } catch (error) {
    const failure = renderErrorState(error.message);
    summaryTarget.innerHTML = failure;
    usersTarget.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
    auditTarget.innerHTML = `<tr><td colspan="4">${failure}</td></tr>`;
    runsTarget.innerHTML = `<tr><td colspan="3">${failure}</td></tr>`;
  }
}

initAdminPage();
