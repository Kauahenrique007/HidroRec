import { initializeShell } from '../../app.js';
import { CONFIG } from '../../config.js';
import { adminService } from '../../services/adminService.js';
import { formatDateTime } from '../../utils/formatters.js';
import { renderErrorState } from '../../utils/helpers.js';

function renderSummary(target, overview) {
  target.innerHTML = [
    {
      label: 'Sensores Ativos',
      value: `${overview.sensorsSummary.online}/${overview.sensorsSummary.sensors.length}`,
      hint: '+2 desde ontem',
      tone: 'green'
    },
    {
      label: 'Alertas Ativos',
      value: String(overview.summary.alertsVolume),
      hint: 'Alta prioridade',
      tone: 'red'
    },
    {
      label: 'Tempo Medio',
      value: `${overview.summary.averageResponseMinutes} min`,
      hint: '-5min vs. meta',
      tone: 'blue'
    },
    {
      label: 'Reportes Hoje',
      value: String(overview.summary.reportsToday),
      hint: '+12 vs. ontem',
      tone: 'blue'
    },
    {
      label: 'Eficiencia',
      value: `${overview.summary.efficiencyPct}%`,
      hint: 'Meta: 85%',
      tone: 'green'
    },
    {
      label: 'Tendencia',
      value: `↓ ${overview.summary.trendPct}%`,
      hint: 'vs. semana passada',
      tone: 'green'
    }
  ].map((item) => `
    <article class="summary-mini-card">
      <span class="summary-mini-card__label">${item.label}</span>
      <strong class="summary-mini-card__value">${item.value}</strong>
      <small class="summary-mini-card__hint summary-mini-card__hint--${item.tone}">${item.hint}</small>
    </article>
  `).join('');
}

function renderHotspots(target, hotspots) {
  const max = Math.max(...hotspots.map((item) => item.occurrences), 1);
  target.innerHTML = hotspots.map((item) => `
    <article class="hotspot-row">
      <div class="hotspot-row__title">
        <strong>${item.territoryName}</strong>
        <span>${item.occurrences} ocorrencias</span>
      </div>
      <div class="hotspot-row__bar">
        <span style="width:${(item.occurrences / max) * 100}%"></span>
      </div>
      <small>${item.responseMinutes}min</small>
    </article>
  `).join('');
}

function renderSensors(overview) {
  const donut = document.getElementById('sensor-donut');
  const legend = document.getElementById('sensor-legend');
  const { online, offline, maintenance, sensors } = overview.sensorsSummary;
  const total = Math.max(online + offline + maintenance, 1);
  const onlineDeg = (online / total) * 360;
  const maintenanceDeg = (maintenance / total) * 360;

  donut.innerHTML = `
    <div class="donut-ring" style="background:conic-gradient(#22c7a9 0deg ${onlineDeg}deg, #f7be2c ${onlineDeg}deg ${onlineDeg + maintenanceDeg}deg, #ff6b6b ${onlineDeg + maintenanceDeg}deg 360deg)"></div>
    <div class="donut-center"></div>
  `;

  legend.innerHTML = `
    <div class="sensor-legend__summary">
      <span class="online">Online: ${online}</span>
      <span class="offline">Offline: ${offline}</span>
      <span class="maintenance">Manutencao: ${maintenance}</span>
    </div>
    ${sensors.map((sensor) => `
      <article class="sensor-row">
        <div>
          <i class="sensor-row__dot sensor-row__dot--${sensor.status}"></i>
          <strong>${sensor.name}</strong>
        </div>
        <span>Leitura: ${sensor.readingAgeMinutes} min</span>
      </article>
    `).join('')}
  `;
}

function renderResponseChart(target, items) {
  const max = Math.max(...items.map((item) => item.efficiencyPct), 100);
  target.innerHTML = `
    <div class="response-chart">
      ${items.map((item) => `
        <article class="response-chart__item">
          <div class="response-chart__bar" style="height:${(item.efficiencyPct / max) * 100}%"></div>
          <strong>${item.region}</strong>
        </article>
      `).join('')}
    </div>
  `;
}

function renderActivityLog(target, items) {
  target.innerHTML = items.map((item) => `
    <article class="activity-log__item">
      <span class="activity-log__time">${new Date(item.time).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}</span>
      <strong class="activity-log__message activity-log__message--${item.tone}">${item.message}</strong>
    </article>
  `).join('');
}

function renderBottomKpis(target, overview) {
  target.innerHTML = `
    <article class="bottom-kpi bottom-kpi--blue">
      <span>Tempo Medio de Normalizacao</span>
      <strong>${overview.performance.normalizationMinutes}min</strong>
      <small>↓ 18% vs. mes anterior</small>
    </article>
    <article class="bottom-kpi bottom-kpi--green">
      <span>Taxa de Acuracia Preditiva</span>
      <strong>${overview.performance.predictiveAccuracyPct}%</strong>
      <small>↑ 5% vs. mes anterior</small>
    </article>
    <article class="bottom-kpi bottom-kpi--purple">
      <span>Engajamento Cidadao</span>
      <strong>${overview.performance.citizenEngagement}</strong>
      <small>Usuarios ativos este mes</small>
    </article>
  `;
}

async function initAdminPage() {
  const user = await initializeShell('admin');
  if (!user) {
    const failure = renderErrorState('Entre como administrador na area Operacao para acessar o painel.');
    document.getElementById('admin-summary').innerHTML = failure;
    document.getElementById('hotspots-list').innerHTML = failure;
    document.getElementById('sensor-legend').innerHTML = failure;
    document.getElementById('response-chart').innerHTML = failure;
    document.getElementById('activity-log').innerHTML = failure;
    document.getElementById('bottom-kpis').innerHTML = failure;
    return;
  }

  try {
    const overview = await adminService.getOverview();
    renderSummary(document.getElementById('admin-summary'), overview);
    renderHotspots(document.getElementById('hotspots-list'), overview.recurringHotspots);
    renderSensors(overview);
    renderResponseChart(document.getElementById('response-chart'), overview.responseByRegion);
    renderActivityLog(document.getElementById('activity-log'), overview.activityFeed);
    renderBottomKpis(document.getElementById('bottom-kpis'), overview);

    document.getElementById('export-csv').setAttribute('href', `${CONFIG.APP_ORIGIN}${overview.docs.openApiPath}`);
    document.getElementById('export-pdf').setAttribute('href', `${CONFIG.APP_ORIGIN}${overview.docs.openApiPath}`);
    document.getElementById('open-diagnostics').setAttribute('href', `${CONFIG.APP_ORIGIN}${overview.docs.apiStatusPath}`);
    document.getElementById('open-report').setAttribute('href', `${CONFIG.APP_ORIGIN}${overview.docs.openApiPath}`);
  } catch (error) {
    const failure = renderErrorState(error.message);
    document.getElementById('admin-summary').innerHTML = failure;
    document.getElementById('hotspots-list').innerHTML = failure;
    document.getElementById('sensor-legend').innerHTML = failure;
    document.getElementById('response-chart').innerHTML = failure;
    document.getElementById('activity-log').innerHTML = failure;
    document.getElementById('bottom-kpis').innerHTML = failure;
  }
}

initAdminPage();
