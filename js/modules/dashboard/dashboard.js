import { initializeShell } from '../../app.js';
import { renderAlertCard } from '../../components/alertcard.js';
import { renderLoadingBlock } from '../../components/LoadingSpinner.js';
import { renderStatCard } from '../../components/StatCard.js';
import { dashboardService } from '../../services/dashboardService.js';
import { formatDateTime, formatNumber, formatRisk } from '../../utils/formatters.js';
import { renderErrorState } from '../../utils/helpers.js';

function renderOverview(data) {
  const statsTarget = document.getElementById('overview-stats');
  const monitoringTarget = document.getElementById('monitoring-summary');
  const territoriesTarget = document.getElementById('territories-hotlist');
  const alertsTarget = document.getElementById('alerts-list');
  const incidentsTarget = document.getElementById('incidents-table');
  const statusTarget = document.getElementById('scenario-status');
  const updateTarget = document.getElementById('scenario-updated-at');

  statsTarget.innerHTML = [
    renderStatCard({
      label: 'Alertas ativos',
      value: formatNumber(data.metrics.activeAlerts),
      hint: 'Sinais operacionais vigentes',
      tone: 'critical'
    }),
    renderStatCard({
      label: 'Ocorrencias pendentes',
      value: formatNumber(data.metrics.pendingIncidents),
      hint: 'Demandas aguardando validacao',
      tone: 'moderate'
    }),
    renderStatCard({
      label: 'Territorios criticos',
      value: formatNumber(data.metrics.criticalTerritories),
      hint: 'Maior score de risco atual',
      tone: 'critical'
    }),
    renderStatCard({
      label: 'Acumulado 24h',
      value: `${formatNumber(data.metrics.averageAccumulatedRain24h, 1)} mm`,
      hint: 'Leitura hidrometeorologica',
      tone: 'info'
    })
  ].join('');

  monitoringTarget.innerHTML = `
    <article class="panel-card">
      <h3>Contexto hidrometeorologico</h3>
      <dl class="data-list">
        <div><dt>Chuva prevista</dt><dd>${formatNumber(data.monitoring.climate.forecastRainMm, 1)} mm</dd></div>
        <div><dt>Chuva observada</dt><dd>${formatNumber(data.monitoring.climate.observedRainMm, 1)} mm</dd></div>
        <div><dt>Mare</dt><dd>${formatNumber(data.monitoring.tide.levelMeters, 2)} m - ${data.monitoring.tide.influence}</dd></div>
        <div><dt>Aviso vigente</dt><dd>${data.monitoring.warning?.summary || 'Sem aviso formal ativo'}</dd></div>
      </dl>
    </article>
  `;

  territoriesTarget.innerHTML = data.topTerritories.map((territory) => `
    <article class="territory-row">
      <div>
        <strong><a class="text-link" href="./detalhes.html?id=${territory.id}">${territory.name}</a></strong>
        <p>${territory.neighborhoodName}</p>
      </div>
      <div class="territory-row__meta">
        <span class="badge badge--${territory.risk.level}">${formatRisk(territory.risk.level)}</span>
        <strong>${territory.risk.score}</strong>
      </div>
    </article>
  `).join('');

  alertsTarget.innerHTML = data.latestAlerts.map(renderAlertCard).join('');

  incidentsTarget.innerHTML = data.latestIncidents.map((incident) => `
    <tr>
      <td>${incident.neighborhoodName}</td>
      <td>${incident.address}</td>
      <td>${incident.type}</td>
      <td>${incident.status}</td>
      <td>${formatDateTime(incident.updatedAt)}</td>
    </tr>
  `).join('');

  statusTarget.textContent = data.operationalStatus;
  updateTarget.textContent = formatDateTime(data.updatedAt);
}

async function initDashboard() {
  await initializeShell('dashboard');

  const statsTarget = document.getElementById('overview-stats');
  const territoriesTarget = document.getElementById('territories-hotlist');
  const alertsTarget = document.getElementById('alerts-list');
  const incidentsTarget = document.getElementById('incidents-table');

  const loading = renderLoadingBlock();
  statsTarget.innerHTML = loading;
  territoriesTarget.innerHTML = loading;
  alertsTarget.innerHTML = loading;
  incidentsTarget.innerHTML = `<tr><td colspan="5">${loading}</td></tr>`;

  try {
    const data = await dashboardService.getOverview();
    renderOverview(data);
  } catch (error) {
    const failure = renderErrorState(error.message);
    statsTarget.innerHTML = failure;
    territoriesTarget.innerHTML = failure;
    alertsTarget.innerHTML = failure;
    incidentsTarget.innerHTML = `<tr><td colspan="5">${failure}</td></tr>`;
  }
}

initDashboard();
