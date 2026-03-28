import { initializeShell } from '../../app.js';
import { dashboardService } from '../../services/dashboardService.js';
import { formatDateTime, formatNumber, formatRisk } from '../../utils/formatters.js';
import { renderErrorState } from '../../utils/helpers.js';
import { renderTerritoryMapPanel } from '../mapa/mapa.js';

function renderStatusCounts(target, data) {
  const counts = [
    {
      label: 'Normal',
      value: data.riskBreakdown.find((item) => item.level === 'baixo')?.count || 0,
      tone: 'normal'
    },
    {
      label: 'Atencao',
      value: (data.riskBreakdown.find((item) => item.level === 'moderado')?.count || 0)
        + (data.riskBreakdown.find((item) => item.level === 'alto')?.count || 0),
      tone: 'attention'
    },
    {
      label: 'Alagamento',
      value: data.metrics.activeAlerts,
      tone: 'flood'
    }
  ];

  target.innerHTML = counts.map((item) => `
    <article class="status-counter status-counter--${item.tone}">
      <strong>${formatNumber(item.value)}</strong>
      <span>${item.label}</span>
    </article>
  `).join('');
}

function renderAttentionList(target, data) {
  target.innerHTML = data.latestIncidents.slice(0, 3).map((incident) => `
    <article class="attention-item">
      <div>
        <strong>${incident.address}</strong>
        <span>Nivel: ${incident.waterLevel} • ${formatDateTime(incident.updatedAt)}</span>
      </div>
      <span class="pill-tag pill-tag--${incident.severity === 'severo' ? 'danger' : 'warning'}">
        ${incident.severity === 'severo' ? 'Alagamento' : 'Atencao'}
      </span>
    </article>
  `).join('');
}

function renderDashboard(data) {
  document.getElementById('scenario-status').textContent = data.operationalStatus;
  document.getElementById('dashboard-toast').textContent = data.monitoring.warning?.summary || 'Chuva moderada detectada. Monitorando pontos criticos.';
  document.getElementById('dashboard-alert-strip').textContent = `${data.metrics.activeAlerts} alagamentos ativos detectados. Evite as areas afetadas.`;
  document.getElementById('dashboard-tide-level').textContent = data.monitoring.tide.influence === 'alta' ? 'Alta' : formatRisk(data.monitoring.tide.influence);
  document.getElementById('dashboard-tide-meta').textContent = `${formatNumber(data.monitoring.tide.levelMeters, 1)}m • Atualizado ${formatDateTime(data.updatedAt)}`;
  document.getElementById('dashboard-rain-level').textContent = `${formatNumber(data.monitoring.climate.observedRainMm, 0)}mm/h`;
  document.getElementById('dashboard-rain-meta').textContent = `${data.monitoring.climate.conditionText || 'Monitoramento ativo'} • Fonte ${((data.monitoring.climate.sourceDetails || []).join(' + ')) || data.monitoring.climate.source}`;

  renderStatusCounts(document.getElementById('city-status-counts'), data);
  renderTerritoryMapPanel(document.getElementById('territory-map'), data.topTerritories);
  renderAttentionList(document.getElementById('attention-points'), data);
}

async function initDashboard() {
  await initializeShell('dashboard');

  try {
    const data = await dashboardService.getOverview();
    renderDashboard(data);
  } catch (error) {
    const failure = renderErrorState(error.message);
    document.getElementById('city-status-counts').innerHTML = failure;
    document.getElementById('territory-map').innerHTML = failure;
    document.getElementById('attention-points').innerHTML = failure;
  }
}

initDashboard();
