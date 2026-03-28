import { initializeShell } from '../../app.js';
import { renderAlertCard } from '../../components/alertcard.js';
import { renderLoadingBlock } from '../../components/LoadingSpinner.js';
import { renderStatCard } from '../../components/StatCard.js';
import { alertsService } from '../../services/alertsService.js';
import { incidentsService } from '../../services/incidentsService.js';
import { getSession } from '../../services/authService.js';
import { formatDateTime } from '../../utils/formatters.js';
import { renderEmptyState, renderErrorState, serializeForm, syncFormWithQuery, updateUrlQuery } from '../../utils/helpers.js';

function ensureSummarySection() {
  let section = document.getElementById('alerts-summary-section');
  if (section) return section;

  const pageGrid = document.querySelector('.page-grid');
  section = document.createElement('section');
  section.className = 'content-card';
  section.id = 'alerts-summary-section';
  section.innerHTML = `
    <div class="section-title">
      <div>
        <span class="section-title__eyebrow">Resumo da fila</span>
        <h2>Panorama rapido de alertas e triagem</h2>
      </div>
    </div>
    <div class="stats-grid" id="alerts-summary-stats"></div>
  `;

  pageGrid.prepend(section);
  return section;
}

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
  const summaryTarget = ensureSummarySection().querySelector('#alerts-summary-stats');
  const initialFilters = Object.fromEntries(new URLSearchParams(window.location.search).entries());
  syncFormWithQuery(filtersForm, initialFilters);

  const loading = renderLoadingBlock('Carregando fila de alertas...');
  alertsTarget.innerHTML = loading;
  incidentsTarget.innerHTML = `<tr><td colspan="5">${loading}</td></tr>`;
  summaryTarget.innerHTML = loading;

  const refresh = async (filters = {}) => {
    try {
      updateUrlQuery(filters);

      const [alertsResponse, incidentsResponse, alertsSummary, incidentsSummary] = await Promise.all([
        alertsService.list({ page: 1, pageSize: 12, ...filters }),
        incidentsService.list({
          page: 1,
          pageSize: 12,
          status: filters.incidentStatus || '',
          search: filters.search || ''
        }),
        alertsService.getSummary(filters),
        incidentsService.getSummary({
          status: filters.incidentStatus || '',
          search: filters.search || ''
        })
      ]);

      summaryTarget.innerHTML = [
        renderStatCard({
          label: 'Alertas filtrados',
          value: String(alertsSummary.total),
          hint: `${alertsSummary.active} ativos e ${alertsSummary.monitoring} monitorados`,
          tone: 'critical'
        }),
        renderStatCard({
          label: 'Ocorrencias na fila',
          value: String(incidentsSummary.total),
          hint: `${incidentsSummary.pending} pendentes e ${incidentsSummary.inAnalysis} em analise`,
          tone: 'moderate'
        }),
        renderStatCard({
          label: 'Bairros afetados',
          value: String(alertsSummary.neighborhoods),
          hint: alertsSummary.latestUpdatedAt ? `Ultima atualizacao ${formatDateTime(alertsSummary.latestUpdatedAt)}` : 'Sem atualizacao recente',
          tone: 'info'
        }),
        renderStatCard({
          label: 'Ocorrencias severas',
          value: String(incidentsSummary.severe),
          hint: `${incidentsSummary.collaborative} vindas do canal colaborativo`,
          tone: 'critical'
        })
      ].join('');

      alertsTarget.innerHTML = alertsResponse.data.length > 0
        ? alertsResponse.data.map(renderAlertCard).join('')
        : renderEmptyState('Nenhum alerta encontrado', 'Ajuste os filtros para ampliar a consulta.');

      incidentsTarget.innerHTML = incidentsResponse.data.length > 0 ? incidentsResponse.data.map((incident) => `
        <tr>
          <td>${incident.neighborhoodName}</td>
          <td>${incident.address}</td>
          <td>${incident.type}</td>
          <td>${renderIncidentActions(incident, isOperator)}</td>
          <td>${formatDateTime(incident.updatedAt)}</td>
        </tr>
      `).join('') : `<tr><td colspan="5">${renderEmptyState('Nenhuma ocorrencia encontrada', 'Nao ha itens na triagem para os filtros atuais.')}</td></tr>`;

      bindIncidentActions(user, () => refresh(filters));
    } catch (error) {
      const failure = renderErrorState(error.message);
      alertsTarget.innerHTML = failure;
      incidentsTarget.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
      summaryTarget.innerHTML = failure;
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

  refresh(initialFilters);
}

initAlertasPage();
