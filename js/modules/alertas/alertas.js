import { initializeShell } from '../../app.js';
import { renderAlertCard } from '../../components/alertcard.js';
import { renderLoadingBlock } from '../../components/LoadingSpinner.js';
import { alertsService } from '../../services/alertsService.js';
import { incidentsService } from '../../services/incidentsService.js';
import { getSession } from '../../services/authService.js';
import { formatDateTime } from '../../utils/formatters.js';
import { renderErrorState, serializeForm } from '../../utils/helpers.js';

function renderIncidentActions(incident, isOperator) {
  if (!isOperator || incident.status === 'resolvido') {
    return incident.status;
  }

  return `
    <div class="table-actions">
      <span>${incident.status}</span>
      <button type="button" class="button button--ghost js-incident-status" data-incident-id="${incident.id}" data-status="validado">Validar</button>
      <button type="button" class="button button--ghost js-incident-status" data-incident-id="${incident.id}" data-status="resolvido">Resolver</button>
    </div>
  `;
}

function bindIncidentActions(user, refresh) {
  document.querySelectorAll('.js-incident-status').forEach((button) => {
    button.addEventListener('click', async () => {
      try {
        await incidentsService.updateStatus(button.dataset.incidentId, {
          status: button.dataset.status,
          note: `Atualizado via console por ${user.name}`
        });
        refresh();
      } catch (error) {
        window.alert(`Nao foi possivel atualizar a ocorrencia: ${error.message}`);
      }
    });
  });
}

async function initAlertasPage() {
  const user = await initializeShell('alertas');
  const isOperator = Boolean(user);
  const alertsTarget = document.getElementById('alerts-grid');
  const incidentsTarget = document.getElementById('incident-queue');
  const filtersForm = document.getElementById('alerts-filter-form');

  const loading = renderLoadingBlock('Carregando fila de alertas...');
  alertsTarget.innerHTML = loading;
  incidentsTarget.innerHTML = `<tr><td colspan="5">${loading}</td></tr>`;

  const refresh = async (filters = {}) => {
    try {
      const [alertsResponse, incidentsResponse] = await Promise.all([
        alertsService.list({ page: 1, pageSize: 12, ...filters }),
        incidentsService.list({ page: 1, pageSize: 12, status: filters.incidentStatus || '' })
      ]);

      alertsTarget.innerHTML = alertsResponse.data.map(renderAlertCard).join('');
      incidentsTarget.innerHTML = incidentsResponse.data.map((incident) => `
        <tr>
          <td>${incident.neighborhoodName}</td>
          <td>${incident.address}</td>
          <td>${incident.type}</td>
          <td>${renderIncidentActions(incident, isOperator)}</td>
          <td>${formatDateTime(incident.updatedAt)}</td>
        </tr>
      `).join('');

      bindIncidentActions(user, () => refresh(filters));
    } catch (error) {
      const failure = renderErrorState(error.message);
      alertsTarget.innerHTML = failure;
      incidentsTarget.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
    }
  };

  filtersForm.addEventListener('submit', (event) => {
    event.preventDefault();
    refresh(serializeForm(filtersForm));
  });

  const session = getSession();
  const hint = document.getElementById('alerts-session-hint');
  hint.textContent = session?.user
    ? 'Sessao operacional ativa: voce pode atualizar status de ocorrencias nesta tela.'
    : 'Entre na area Operacao para validar ou resolver ocorrencias pela interface.';

  refresh();
}

initAlertasPage();
