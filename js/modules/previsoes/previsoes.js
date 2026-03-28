import { initializeShell } from '../../app.js';
import { incidentsService } from '../../services/incidentsService.js';
import { monitoringService } from '../../services/monitoringService.js';
import { formatNumber } from '../../utils/formatters.js';
import { renderErrorState } from '../../utils/helpers.js';

function buildChartSvg(points, { stroke = '#4c9fff', fill = 'rgba(76,159,255,0.22)', max = 100 } = {}) {
  if (!points.length) {
    return '<div class="empty-state"><strong>Sem dados</strong></div>';
  }

  const width = 680;
  const height = 220;
  const step = width / Math.max(points.length - 1, 1);
  const coordinates = points.map((point, index) => {
    const x = index * step;
    const y = height - ((point.value / max) * (height - 30)) - 10;
    return `${x},${y}`;
  }).join(' ');
  const areaPoints = `0,${height} ${coordinates} ${width},${height}`;

  return `
    <svg viewBox="0 0 ${width} ${height}" class="chart-svg" preserveAspectRatio="none" aria-hidden="true">
      <polygon points="${areaPoints}" fill="${fill}"></polygon>
      <polyline points="${coordinates}" fill="none" stroke="${stroke}" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"></polyline>
      ${points.map((point, index) => {
        const x = index * step;
        const y = height - ((point.value / max) * (height - 30)) - 10;
        return `<circle cx="${x}" cy="${y}" r="3" fill="${stroke}"></circle>`;
      }).join('')}
    </svg>
    <div class="chart-label-row">
      ${points.map((point) => `<span>${point.label}</span>`).join('')}
    </div>
  `;
}

function buildBars(points, colorClass = '') {
  const max = Math.max(...points.map((point) => point.value), 1);
  return `
    <div class="bar-chart">
      ${points.map((point) => `
        <div class="bar-chart__item">
          <div class="bar-chart__bar ${colorClass}" style="height:${(point.value / max) * 100}%"></div>
          <span>${point.label}</span>
        </div>
      `).join('')}
    </div>
  `;
}

function renderForecast(data, incidents) {
  const timeline = data.timeline.climateReadings.slice().reverse();
  const rainSeries = timeline.map((item) => ({
    label: new Date(item.collectedAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }),
    value: Number(item.forecastRainMm || item.observedRainMm || 0)
  }));
  const tideSeries = data.timeline.tideReadings.slice().reverse().map((item) => ({
    label: new Date(item.collectedAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }),
    value: Number(item.levelMeters || 0)
  }));

  const maxRain = Math.max(...rainSeries.map((item) => item.value), 1);
  const maxTide = Math.max(...tideSeries.map((item) => item.value), 1);
  const floodingProbability = Math.min(
    100,
    Math.round((data.overview.climate.forecastRainMm * 1.2) + (data.overview.tide.levelMeters * 12))
  );

  const probabilitySeries = rainSeries.map((item, index) => ({
    label: item.label,
    value: Math.min(100, Math.round((item.value / maxRain) * 70 + ((tideSeries[index]?.value || 0) / maxTide) * 30))
  }));

  const weekDays = ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sab', 'Dom'];
  const weeklySeries = weekDays.map((label, index) => ({
    label,
    value: incidents.filter((incident) => new Date(incident.createdAt).getDay() === ((index + 1) % 7)).length + 1
  }));

  document.getElementById('forecast-probability').textContent = `${formatNumber(floodingProbability)}%`;
  document.getElementById('forecast-rain-total').textContent = `${formatNumber(data.overview.climate.forecastRainMm, 1)} mm`;
  document.getElementById('forecast-tide-peak').textContent = `${formatNumber(Math.max(...tideSeries.map((item) => item.value), data.overview.tide.levelMeters), 1)} m`;
  document.getElementById('probability-chart').innerHTML = buildChartSvg(probabilitySeries, { max: 100 });
  document.getElementById('rain-bars-chart').innerHTML = buildBars(rainSeries, 'bar-chart__bar--blue');
  document.getElementById('tide-line-chart').innerHTML = buildChartSvg(tideSeries, {
    stroke: '#22c7a9',
    fill: 'rgba(34,199,169,0.15)',
    max: Math.max(maxTide, 3)
  });
  document.getElementById('weekly-history-chart').innerHTML = buildBars(weeklySeries, 'bar-chart__bar--green');
}

async function initForecastPage() {
  await initializeShell('previsoes');

  try {
    const [overview, timeline, incidentsResponse] = await Promise.all([
      monitoringService.getOverview(),
      monitoringService.getTimeline(8),
      incidentsService.list({ page: 1, pageSize: 30 })
    ]);

    renderForecast({ overview, timeline }, incidentsResponse.data);
  } catch (error) {
    const failure = renderErrorState(error.message);
    document.getElementById('probability-chart').innerHTML = failure;
    document.getElementById('rain-bars-chart').innerHTML = failure;
    document.getElementById('tide-line-chart').innerHTML = failure;
    document.getElementById('weekly-history-chart').innerHTML = failure;
  }
}

initForecastPage();
