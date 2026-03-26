import { initializeShell } from '../../app.js';
import { renderAlertCard } from '../../components/alertcard.js';
import { renderLoadingBlock } from '../../components/LoadingSpinner.js';
import { alertsService } from '../../services/alertsService.js';
import { incidentsService } from '../../services/incidentsService.js';
import { territoriesService } from '../../services/territoriesService.js';
import { formatDateTime, formatRisk } from '../../utils/formatters.js';
import { renderErrorState, serializeForm } from '../../utils/helpers.js';
import { renderTerritoryMapPanel } from '../mapa/mapa.js';

async function loadData(filters = {}) {
  const [territoriesResponse, incidentsResponse, alertsResponse] = await Promise.all([
    territoriesService.list({ page: 1, pageSize: 20, sortBy: 'score', order: 'desc', ...filters }),
    incidentsService.list({ page: 1, pageSize: 10, ...filters }),
    alertsService.list({ page: 1, pageSize: 6 })
  ]);

  return {
    territories: territoriesResponse.data,
    incidents: incidentsResponse.data,
    alerts: alertsResponse.data
  };
}

function renderTables(data) {
  const territoryTable = document.getElementById('territory-table');
  const incidentTable = document.getElementById('incident-table');
  const alertQueue = document.getElementById('alert-queue');
  const mapTarget = document.getElementById('territory-map');

  territoryTable.innerHTML = data.territories.map((territory) => `
    <tr>
      <td><a class="text-link" href="./detalhes.html?id=${territory.id}">${territory.name}</a></td>
      <td>${territory.neighborhoodName}</td>
      <td>${formatRisk(territory.risk.level)}</td>
      <td>${territory.risk.score}</td>
      <td>${territory.activeAlerts}</td>
    </tr>
  `).join('');

  incidentTable.innerHTML = data.incidents.map((incident) => `
    <tr>
      <td>${incident.neighborhoodName}</td>
      <td>${incident.address}</td>
      <td>${incident.type}</td>
      <td>${incident.status}</td>
      <td>${formatDateTime(incident.updatedAt)}</td>
    </tr>
  `).join('');

  alertQueue.innerHTML = data.alerts.map(renderAlertCard).join('');
  renderTerritoryMapPanel(mapTarget, data.territories);
}

async function initListaPage() {
  await initializeShell('lista');

  const territoryTable = document.getElementById('territory-table');
  const incidentTable = document.getElementById('incident-table');
  const alertQueue = document.getElementById('alert-queue');
  const mapTarget = document.getElementById('territory-map');
  const filterForm = document.getElementById('filters-form');

  const loading = renderLoadingBlock();
  territoryTable.innerHTML = `<tr><td colspan="5">${loading}</td></tr>`;
  incidentTable.innerHTML = `<tr><td colspan="5">${loading}</td></tr>`;
  alertQueue.innerHTML = loading;
  mapTarget.innerHTML = loading;

  const refresh = async (filters = {}) => {
    try {
      const data = await loadData(filters);
      renderTables(data);
    } catch (error) {
      const failure = renderErrorState(error.message);
      territoryTable.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
      incidentTable.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
      alertQueue.innerHTML = failure;
      mapTarget.innerHTML = failure;
    }
  };

  filterForm.addEventListener('submit', (event) => {
    event.preventDefault();
    const filters = serializeForm(filterForm);
    refresh(filters);
  });

  refresh();
}

initListaPage();
