import { mapService } from '../../services/mapService.js';
import { formatDateTime, formatRisk } from '../../utils/formatters.js';

const RISK_LABELS = [
  ['baixo', 'Baixo'],
  ['moderado', 'Medio'],
  ['alto', 'Alto'],
  ['critico', 'Critico']
];

function normalizeText(value = '') {
  return String(value)
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim();
}

function pointMatches(point, query) {
  const text = normalizeText([
    point.name,
    point.neighborhoodName,
    point.address,
    point.type,
    point.description
  ].filter(Boolean).join(' '));

  return text.includes(normalizeText(query));
}

function distanceBetween(left, right) {
  if (!left || !right) return Number.POSITIVE_INFINITY;
  const lat = Number(left.latitude) - Number(right.latitude);
  const lng = Number(left.longitude) - Number(right.longitude);
  return Math.sqrt((lat * lat) + (lng * lng));
}

function findClosestSafeTerritory(territories, point, blockedIds) {
  return territories
    .filter((territory) => !blockedIds.has(territory.id) && territory.risk?.level !== 'critico')
    .sort((left, right) => distanceBetween(left, point) - distanceBetween(right, point))[0]
    || territories.find((territory) => !blockedIds.has(territory.id))
    || territories[0];
}

function findMapPoint(query, state) {
  if (!query) return null;
  return [...state.territories, ...state.incidents].find((point) => pointMatches(point, query)) || null;
}

function buildBlockedTerritoryIds(state) {
  const blocked = new Set();

  state.incidents
    .filter((incident) => incident.status !== 'resolvido' && normalizeText(incident.type).includes('alag'))
    .forEach((incident) => {
      if (incident.territoryId) {
        blocked.add(incident.territoryId);
        return;
      }

      const closest = state.territories
        .slice()
        .sort((left, right) => distanceBetween(left, incident) - distanceBetween(right, incident))[0];
      if (closest && distanceBetween(closest, incident) < 0.01) {
        blocked.add(closest.id);
      }
    });

  return blocked;
}

function renderPointDetails(target, point, type = 'territory') {
  if (!target || !point) return;

  if (type === 'incident') {
    target.innerHTML = `
      <span class="section-title__eyebrow">Ocorrencia aberta</span>
      <strong>${mapService.escapeHtml(point.address)}</strong>
      <p>${mapService.escapeHtml(point.neighborhoodName)} - ${mapService.escapeHtml(point.waterLevel || point.type)}</p>
      <dl class="detail-list">
        <div><dt>Status</dt><dd>${mapService.escapeHtml(point.status)}</dd></div>
        <div><dt>Atualizado</dt><dd>${formatDateTime(point.updatedAt)}</dd></div>
      </dl>
      <p>${mapService.escapeHtml(point.description || 'Sem descricao complementar.')}</p>
    `;
    return;
  }

  target.innerHTML = `
    <span class="section-title__eyebrow">Territorio monitorado</span>
    <strong>${mapService.escapeHtml(point.name)}</strong>
    <p>${mapService.escapeHtml(point.neighborhoodName)} - ${mapService.escapeHtml(point.address || '')}</p>
    <dl class="detail-list">
      <div><dt>Risco</dt><dd>${formatRisk(point.risk?.level)}</dd></div>
      <div><dt>Score</dt><dd>${point.risk?.score ?? '--'}</dd></div>
      <div><dt>Alertas</dt><dd>${point.activeAlerts ?? 0}</dd></div>
    </dl>
    <p>${mapService.escapeHtml(point.notes || point.risk?.recommendation || 'Monitoramento ativo.')}</p>
  `;
}

function drawRoute(board, routePoints, bounds) {
  const svg = board.querySelector('[data-route-layer]');
  if (!svg) return;

  if (routePoints.length < 2) {
    svg.innerHTML = '';
    return;
  }

  const points = routePoints.map((point) => {
    const position = mapService.positionFor(point, bounds);
    return `${position.left},${position.top}`;
  }).join(' ');

  svg.innerHTML = `
    <polyline class="safe-route-line" points="${points}" vector-effect="non-scaling-stroke"></polyline>
    ${routePoints.map((point) => {
      const position = mapService.positionFor(point, bounds);
      return `<circle class="safe-route-node" cx="${position.left}" cy="${position.top}" r="1.7"></circle>`;
    }).join('')}
  `;
}

function calculateSafeRoute(state, originQuery, destinationQuery) {
  const blockedIds = buildBlockedTerritoryIds(state);
  const warnings = [];
  const currentLocation = state.currentLocation;
  let origin = findMapPoint(originQuery, state) || currentLocation || state.territories[0];
  let destination = findMapPoint(destinationQuery, state) || state.territories[0];

  if (origin?.id && blockedIds.has(origin.id)) {
    warnings.push('Origem com ocorrencia aberta; foi usada a alternativa segura mais proxima.');
    origin = findClosestSafeTerritory(state.territories, origin, blockedIds);
  }

  if (destination?.id && blockedIds.has(destination.id)) {
    warnings.push('Destino com ocorrencia aberta; foi usado o ponto seguro mais proximo.');
    destination = findClosestSafeTerritory(state.territories, destination, blockedIds);
  }

  const safeWaypoints = state.territories
    .filter((territory) => territory.id !== origin?.id && territory.id !== destination?.id)
    .filter((territory) => !blockedIds.has(territory.id) && territory.risk?.level !== 'critico')
    .sort((left, right) => {
      const leftScore = distanceBetween(origin, left) + distanceBetween(left, destination);
      const rightScore = distanceBetween(origin, right) + distanceBetween(right, destination);
      return leftScore - rightScore;
    })
    .slice(0, Math.min(2, Math.max(0, state.territories.length - 2)));

  const routePoints = [origin, ...safeWaypoints, destination].filter(Boolean);

  return {
    routePoints,
    avoided: blockedIds.size,
    warnings
  };
}

function bindMapInteractions(target, state) {
  const detailsTarget = target.querySelector('[data-map-details]');
  const routeStatus = target.querySelector('[data-route-status]');
  const board = target.querySelector('.map-board');
  const originInput = target.querySelector('[data-route-origin]');
  const destinationInput = target.querySelector('[data-route-destination]');

  target.querySelectorAll('[data-map-point="territory"]').forEach((button) => {
    button.addEventListener('click', () => {
      const territory = state.territories.find((item) => item.id === button.dataset.territoryId);
      renderPointDetails(detailsTarget, territory, 'territory');
    });
  });

  target.querySelectorAll('[data-map-point="incident"]').forEach((button) => {
    button.addEventListener('click', () => {
      const incident = state.incidents.find((item) => item.id === button.dataset.incidentId);
      renderPointDetails(detailsTarget, incident, 'incident');
    });
  });

  target.querySelector('[data-action="route-current"]')?.addEventListener('click', () => {
    if (!('geolocation' in navigator)) {
      routeStatus.textContent = 'Geolocalizacao indisponivel neste navegador.';
      routeStatus.className = 'route-status route-status--error';
      return;
    }

    routeStatus.textContent = 'Buscando localizacao atual...';
    routeStatus.className = 'route-status';

    navigator.geolocation.getCurrentPosition(
      (position) => {
        state.currentLocation = {
          id: 'current-location',
          name: 'Localizacao atual',
          address: 'Localizacao atual',
          neighborhoodName: 'Recife',
          latitude: position.coords.latitude,
          longitude: position.coords.longitude
        };
        originInput.value = 'Localizacao atual';
        routeStatus.textContent = 'Localizacao anexada a origem da rota.';
        routeStatus.className = 'route-status route-status--success';
      },
      () => {
        routeStatus.textContent = 'Nao foi possivel obter sua localizacao.';
        routeStatus.className = 'route-status route-status--error';
      },
      { enableHighAccuracy: true, timeout: 7000, maximumAge: 300000 }
    );
  });

  target.querySelector('[data-action="route-safe"]')?.addEventListener('click', () => {
    if (!state.territories.length) {
      routeStatus.textContent = 'Nao ha pontos suficientes para calcular rota.';
      routeStatus.className = 'route-status route-status--error';
      return;
    }

    const result = calculateSafeRoute(state, originInput.value, destinationInput.value);
    drawRoute(board, result.routePoints, state.bounds);

    const routeNames = result.routePoints.map((point) => point.name || point.address || 'Origem').join(' > ');
    const warningText = result.warnings.length ? ` ${result.warnings.join(' ')}` : '';
    routeStatus.textContent = `Rota segura: ${routeNames}. ${result.avoided} ponto(s) alagado(s) evitado(s).${warningText}`;
    routeStatus.className = 'route-status route-status--success';
  });

  if (state.territories[0]) {
    renderPointDetails(detailsTarget, state.territories[0], 'territory');
  }
}

export function renderTerritoryMapPanel(target, territories = [], options = {}) {
  const incidents = options.incidents || [];
  const bounds = mapService.getBounds([...territories, ...incidents]);
  const state = {
    territories,
    incidents,
    alerts: options.alerts || [],
    bounds,
    currentLocation: null
  };

  target.innerHTML = `
    <div class="map-shell">
      <div class="map-toolbar" aria-label="Busca e rota segura">
        <div class="field">
          <label for="route-origin-${target.id || 'map'}">Origem</label>
          <input id="route-origin-${target.id || 'map'}" data-route-origin type="search" placeholder="Local atual, bairro ou endereco">
        </div>
        <div class="field">
          <label for="route-destination-${target.id || 'map'}">Destino</label>
          <input id="route-destination-${target.id || 'map'}" data-route-destination type="search" placeholder="Territorio, bairro ou rua">
        </div>
        <div class="map-toolbar__actions">
          <button class="button button--ghost" type="button" data-action="route-current">Local atual</button>
          <button class="button" type="button" data-action="route-safe">Rota segura</button>
        </div>
      </div>

      <div class="map-board" role="region" aria-label="Mapa sintetico de areas de risco do Recife">
        <div class="map-board__grid"></div>
        <div class="map-board__river"></div>
        <svg class="map-route-layer" data-route-layer viewBox="0 0 100 100" preserveAspectRatio="none" aria-hidden="true"></svg>
        <div class="map-board__overlay">
          ${mapService.buildTerritoryMapMarkup(territories, bounds)}
          ${mapService.buildIncidentMapMarkup(incidents, bounds)}
        </div>
        <div class="map-legend">
          <strong>Legenda de risco</strong>
          ${RISK_LABELS.map(([level, label]) => `<span><i class="legend-dot legend-dot--${level}"></i>${label}</span>`).join('')}
          <span><i class="legend-ring"></i>Ocorrencia aberta</span>
        </div>
      </div>

      <div class="map-bottom">
        <div class="route-status" data-route-status role="status" aria-live="polite">Informe origem e destino para calcular uma rota que evite alagamentos abertos.</div>
        <aside class="map-details" data-map-details aria-live="polite"></aside>
      </div>
    </div>
  `;

  bindMapInteractions(target, state);
}
