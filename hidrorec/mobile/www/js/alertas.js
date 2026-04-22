import { initializePage } from './app.js';
import { alertsApi } from './api.js';
import { createBadge, formatDateTime, renderError, renderLoading } from './utils.js';

initializePage();

const alertsList = document.getElementById('alerts-list');
const filterButtons = document.querySelectorAll('[data-criticidade]');

async function loadAlerts(criticidade = '') {
  renderLoading(alertsList);

  try {
    const alerts = await alertsApi.getAll(criticidade);

    if (!alerts.length) {
      alertsList.innerHTML = '<div class="empty-state">Sem alertas.</div>';
      return;
    }

    alertsList.innerHTML = alerts.map((alerta) => `
      <article class="list-item list-item--${String(alerta.criticidade || '').toLowerCase()} ${resolveAlertSurfaceClass(alerta.criticidade)}">
        <div class="list-item__row">
          <div>
            <strong>${alerta.titulo}</strong>
            <div class="muted">${alerta.areaAfetada}</div>
          </div>
          ${createBadge(alerta.criticidade)}
        </div>
        <small class="muted">${formatDateTime(alerta.dataCriacao)}</small>
      </article>
    `).join('');
  } catch (error) {
    renderError(alertsList, error.message);
  }
}

function resolveAlertSurfaceClass(criticidade) {
  const normalized = String(criticidade || '').toLowerCase();

  if (normalized.includes('critico') || normalized.includes('alagamento')) {
    return 'surface--critical';
  }

  return '';
}

filterButtons.forEach((button) => {
  button.addEventListener('click', () => loadAlerts(button.dataset.criticidade || ''));
});

loadAlerts();
