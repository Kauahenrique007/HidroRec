import { initializeShell } from '../../app.js';
import { renderLoadingBlock } from '../../components/LoadingSpinner.js';
import { renderStatCard } from '../../components/StatCard.js';
import { dashboardService } from '../../services/dashboardService.js';
import { formatDateTime, formatNumber } from '../../utils/formatters.js';
import { renderEmptyState, renderErrorState } from '../../utils/helpers.js';
import { renderTerritoryMapPanel } from '../mapa/mapa.js';

function renderDashboardKpis(target, data) {
  const floodedAreas = data.metrics.floodedAreas ?? data.latestIncidents.filter((incident) => incident.status !== 'resolvido').length;
  const monitoredTerritories = data.metrics.monitoredTerritories ?? data.topTerritories.length;

  target.innerHTML = [
    renderStatCard({
      label: 'Chuva atual',
      value: `${formatNumber(data.monitoring.climate.observedRainMm, 0)} mm`,
      hint: `${formatNumber(data.monitoring.climate.accumulatedRain24h, 0)} mm nas ultimas 24h`,
      tone: 'info'
    }),
    renderStatCard({
      label: 'Alertas ativos',
      value: String(data.metrics.activeAlerts),
      hint: `${data.metrics.monitoringAlerts} em monitoramento`,
      tone: 'critical'
    }),
    renderStatCard({
      label: 'Areas alagadas',
      value: String(floodedAreas),
      hint: 'Ocorrencias abertas no historico',
      tone: 'danger'
    }),
    renderStatCard({
      label: 'Pontos monitorados',
      value: String(monitoredTerritories),
      hint: `${data.metrics.criticalTerritories} em criticidade maxima`,
      tone: 'safe'
    })
  ].join('');
}

function renderStatusCounts(target, data) {
  const riskCount = (level) => data.riskBreakdown.find((item) => item.level === level)?.count || 0;
  const counts = [
    {
      label: 'Baixo',
      value: riskCount('baixo'),
      tone: 'normal'
    },
    {
      label: 'Medio',
      value: riskCount('moderado'),
      tone: 'attention'
    },
    {
      label: 'Alto',
      value: riskCount('alto'),
      tone: 'flood'
    },
    {
      label: 'Critico',
      value: riskCount('critico'),
      tone: 'critical'
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
  target.innerHTML = data.latestIncidents.length ? data.latestIncidents.slice(0, 5).map((incident) => `
    <article class="attention-item">
      <div>
        <strong>${incident.address}</strong>
        <span>${incident.neighborhoodName} - Nivel: ${incident.waterLevel} - ${formatDateTime(incident.updatedAt)}</span>
      </div>
      <span class="pill-tag pill-tag--${incident.severity === 'severo' ? 'danger' : 'warning'}">
        ${incident.severity === 'severo' ? 'Alagamento' : 'Atencao'}
      </span>
    </article>
  `).join('') : renderEmptyState('Sem ocorrencias recentes', 'Nenhum ponto de atencao foi retornado pela API.');
}

function renderDashboard(data) {
  document.getElementById('scenario-status').textContent = data.operationalStatus;
  document.getElementById('dashboard-toast').textContent = data.monitoring.warning?.summary
    || data.scenario?.summary
    || 'Monitoramento ativo com consolidacao de chuva, mare e ocorrencias.';
  document.getElementById('dashboard-alert-strip').textContent = `${data.metrics.activeAlerts} alertas ativos e ${data.metrics.pendingIncidents} ocorrencias pendentes.`;

  renderDashboardKpis(document.getElementById('dashboard-kpis'), data);
  renderStatusCounts(document.getElementById('city-status-counts'), data);
  renderTerritoryMapPanel(document.getElementById('territory-map'), data.topTerritories, {
    incidents: data.latestIncidents,
    alerts: data.latestAlerts
  });
  renderAttentionList(document.getElementById('attention-points'), data);
}

async function initDashboard() {
  await initializeShell('dashboard');

  const loading = renderLoadingBlock('Carregando painel operacional...');
  document.getElementById('dashboard-kpis').innerHTML = loading;
  document.getElementById('city-status-counts').innerHTML = loading;
  document.getElementById('territory-map').innerHTML = loading;
  document.getElementById('attention-points').innerHTML = loading;

  try {
    const data = await dashboardService.getOverview();
    renderDashboard(data);
  } catch (error) {
    const failure = renderErrorState(error.message);
    document.getElementById('dashboard-kpis').innerHTML = failure;
    document.getElementById('city-status-counts').innerHTML = failure;
    document.getElementById('territory-map').innerHTML = failure;
    document.getElementById('attention-points').innerHTML = failure;
  }
}

initDashboard();
