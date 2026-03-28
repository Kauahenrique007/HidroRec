import { initializeShell } from '../../app.js';
import { renderAlertCard } from '../../components/alertcard.js';
import { renderLoadingBlock } from '../../components/LoadingSpinner.js';
import { renderStatCard } from '../../components/StatCard.js';
import { alertsService } from '../../services/alertsService.js';
import { incidentsService } from '../../services/incidentsService.js';
import { territoriesService } from '../../services/territoriesService.js';
import { formatDateTime, formatRisk } from '../../utils/formatters.js';
import { renderEmptyState, renderErrorState, serializeForm, syncFormWithQuery, updateUrlQuery } from '../../utils/helpers.js';
import { renderTerritoryMapPanel } from '../mapa/mapa.js';

async function loadData(filters = {}) {
  const territoryFilters = {
    page: 1,
    pageSize: 20,
    sortBy: 'score',
    order: 'desc',
    search: filters.search || '',
    riskLevel: filters.riskLevel || ''
  };
  const incidentFilters = {
    page: 1,
    pageSize: 10,
    search: filters.search || '',
    status: filters.status || ''
  };

  const [territoriesResponse, incidentsResponse, alertsResponse, territoriesSummary, incidentsSummary] = await Promise.all([
    territoriesService.list(territoryFilters),
    incidentsService.list(incidentFilters),
    alertsService.list({ page: 1, pageSize: 6, search: filters.search || '' }),
    territoriesService.getSummary(territoryFilters),
    incidentsService.getSummary(incidentFilters)
  ]);

  return {
    territories: territoriesResponse.data,
    incidents: incidentsResponse.data,
    alerts: alertsResponse.data,
    territoriesSummary,
    incidentsSummary
  };
}

function ensureSummarySection() {
  let section = document.getElementById('lista-summary-section');
  if (section) return section;

  const pageGrid = document.querySelector('.page-grid');
  section = document.createElement('section');
  section.className = 'content-card';
  section.id = 'lista-summary-section';
  section.innerHTML = `
    <div class="section-title">
      <div>
        <span class="section-title__eyebrow">Sintese territorial</span>
        <h2>Leitura rapida do recorte atual</h2>
      </div>
    </div>
    <div class="stats-grid" id="lista-summary-stats"></div>
  `;

  pageGrid.prepend(section);
  return section;
}

function renderTables(data) {
  const territoryTable = document.getElementById('territory-table');
  const incidentTable = document.getElementById('incident-table');
  const alertQueue = document.getElementById('alert-queue');
  const mapTarget = document.getElementById('territory-map');
  const summaryTarget = ensureSummarySection().querySelector('#lista-summary-stats');

  summaryTarget.innerHTML = [
    renderStatCard({
      label: 'Territorios no recorte',
      value: String(data.territoriesSummary.total),
      hint: `${data.territoriesSummary.withAlerts} com alerta associado`,
      tone: 'info'
    }),
    renderStatCard({
      label: 'Score medio',
      value: String(data.territoriesSummary.averageScore),
      hint: `Pico atual em ${data.territoriesSummary.highestScore}`,
      tone: 'moderate'
    }),
    renderStatCard({
      label: 'Ocorrencias retornadas',
      value: String(data.incidentsSummary.total),
      hint: `${data.incidentsSummary.pending} pendentes no recorte`,
      tone: 'critical'
    }),
    renderStatCard({
      label: 'Territorio lider',
      value: data.territoriesSummary.topTerritory?.neighborhoodName || '--',
      hint: data.territoriesSummary.topTerritory?.name || 'Sem territorio priorizado',
      tone: 'info'
    })
  ].join('');

  territoryTable.innerHTML = data.territories.length > 0 ? data.territories.map((territory) => `
    <tr>
      <td><a class="text-link" href="./detalhes.html?id=${territory.id}">${territory.name}</a></td>
      <td>${territory.neighborhoodName}</td>
      <td>${formatRisk(territory.risk.level)}</td>
      <td>${territory.risk.score}</td>
      <td>${territory.activeAlerts}</td>
    </tr>
  `).join('') : `<tr><td colspan="5">${renderEmptyState('Nenhum territorio encontrado', 'Tente remover parte dos filtros para ampliar o mapa operacional.')}</td></tr>`;

  incidentTable.innerHTML = data.incidents.length > 0 ? data.incidents.map((incident) => `
    <tr>
      <td>${incident.neighborhoodName}</td>
      <td>${incident.address}</td>
      <td>${incident.type}</td>
      <td>${incident.status}</td>
      <td>${formatDateTime(incident.updatedAt)}</td>
    </tr>
  `).join('') : `<tr><td colspan="5">${renderEmptyState('Nenhuma ocorrencia encontrada', 'Nao ha registros para o filtro aplicado.')}</td></tr>`;

  alertQueue.innerHTML = data.alerts.length > 0
    ? data.alerts.map(renderAlertCard).join('')
    : renderEmptyState('Nenhum alerta relacionado', 'O recorte atual nao trouxe alertas recentes.');
  renderTerritoryMapPanel(mapTarget, data.territories);
}

async function initListaPage() {
  await initializeShell('lista');

  const territoryTable = document.getElementById('territory-table');
  const incidentTable = document.getElementById('incident-table');
  const alertQueue = document.getElementById('alert-queue');
  const mapTarget = document.getElementById('territory-map');
  const filterForm = document.getElementById('filters-form');
  const summaryTarget = ensureSummarySection().querySelector('#lista-summary-stats');
  const initialFilters = Object.fromEntries(new URLSearchParams(window.location.search).entries());
  syncFormWithQuery(filterForm, initialFilters);

  const loading = renderLoadingBlock();
  territoryTable.innerHTML = `<tr><td colspan="5">${loading}</td></tr>`;
  incidentTable.innerHTML = `<tr><td colspan="5">${loading}</td></tr>`;
  alertQueue.innerHTML = loading;
  mapTarget.innerHTML = loading;
  summaryTarget.innerHTML = loading;

  const refresh = async (filters = {}) => {
    try {
      updateUrlQuery(filters);
      const data = await loadData(filters);
      renderTables(data);
    } catch (error) {
      const failure = renderErrorState(error.message);
      territoryTable.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
      incidentTable.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
      alertQueue.innerHTML = failure;
      mapTarget.innerHTML = failure;
      summaryTarget.innerHTML = failure;
    }
  };

  filterForm.addEventListener('submit', (event) => {
    event.preventDefault();
    const filters = serializeForm(filterForm);
    refresh(filters);
  });

  refresh(initialFilters);
}

initListaPage();
