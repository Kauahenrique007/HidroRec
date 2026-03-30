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
      alertsList.innerHTML = '<div class="empty-state">Nenhum alerta ativo para esse filtro.</div>';
      return;
    }

    alertsList.innerHTML = alerts.map((alerta) => `
      <article class="list-item list-item--${String(alerta.criticidade || '').toLowerCase()}">
        <div class="list-item__row">
          <div>
            <strong>${alerta.titulo}</strong>
            <div class="muted">${alerta.areaAfetada}</div>
          </div>
          ${createBadge(alerta.criticidade)}
        </div>
        <p class="alert-description">${alerta.descricao}</p>
        <small class="muted">${alerta.orientacaoResumida} | ${formatDateTime(alerta.dataCriacao)}</small>
      </article>
    `).join('');
  } catch (error) {
    renderError(alertsList, error.message);
  }
}

filterButtons.forEach((button) => {
  button.addEventListener('click', () => loadAlerts(button.dataset.criticidade || ''));
});

loadAlerts();
