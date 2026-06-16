import { formatRisk } from '../utils/formatters.js';

function escapeHtml(value = '') {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');
}

function scale(value, min, max) {
  if (!Number.isFinite(value)) return 50;
  if (max === min) return 50;
  return ((value - min) / (max - min)) * 100;
}

function getBounds(items) {
  const coordinates = items.filter((item) => Number.isFinite(item.latitude) && Number.isFinite(item.longitude));
  if (!coordinates.length) {
    return {
      minLat: -8.14,
      maxLat: -8.03,
      minLng: -34.93,
      maxLng: -34.86
    };
  }

  const latitudes = coordinates.map((item) => item.latitude);
  const longitudes = coordinates.map((item) => item.longitude);
  const latPadding = Math.max((Math.max(...latitudes) - Math.min(...latitudes)) * 0.12, 0.004);
  const lngPadding = Math.max((Math.max(...longitudes) - Math.min(...longitudes)) * 0.12, 0.004);

  return {
    minLat: Math.min(...latitudes) - latPadding,
    maxLat: Math.max(...latitudes) + latPadding,
    minLng: Math.min(...longitudes) - lngPadding,
    maxLng: Math.max(...longitudes) + lngPadding
  };
}

function positionFor(item, bounds) {
  return {
    top: 100 - scale(Number(item.latitude), bounds.minLat, bounds.maxLat),
    left: scale(Number(item.longitude), bounds.minLng, bounds.maxLng)
  };
}

function normalizeRisk(level) {
  return ['baixo', 'moderado', 'alto', 'critico'].includes(level) ? level : 'baixo';
}

export const mapService = {
  escapeHtml,
  getBounds,
  positionFor,

  buildTerritoryMapMarkup(territories = [], bounds = getBounds(territories)) {
    if (!territories.length) {
      return '<div class="map-placeholder">Sem territorios para exibir.</div>';
    }

    return territories.map((territory) => {
      const position = positionFor(territory, bounds);
      const riskLevel = normalizeRisk(territory.risk?.level);
      const score = territory.risk?.score ?? '--';
      const title = `${territory.name} - ${formatRisk(riskLevel)} - score ${score}`;

      return `
        <button
          class="territory-marker territory-marker--${riskLevel}"
          style="top:${position.top}%; left:${position.left}%"
          title="${escapeHtml(title)}"
          aria-label="${escapeHtml(title)}"
          data-map-point="territory"
          data-territory-id="${escapeHtml(territory.id)}"
          type="button"
        >
          <span class="territory-marker__pulse"></span>
          <span class="territory-marker__core">${escapeHtml(score)}</span>
        </button>
      `;
    }).join('');
  },

  buildIncidentMapMarkup(incidents = [], bounds = getBounds(incidents)) {
    const openIncidents = incidents.filter((incident) => incident.status !== 'resolvido' && Number.isFinite(incident.latitude) && Number.isFinite(incident.longitude));

    return openIncidents.map((incident) => {
      const position = positionFor(incident, bounds);
      const severity = incident.severity === 'severo' ? 'severo' : incident.severity === 'atencao' ? 'atencao' : 'observacao';
      const title = `${incident.type} em ${incident.address}, ${incident.neighborhoodName}`;

      return `
        <button
          class="incident-marker incident-marker--${severity}"
          style="top:${position.top}%; left:${position.left}%"
          title="${escapeHtml(title)}"
          aria-label="${escapeHtml(title)}"
          data-map-point="incident"
          data-incident-id="${escapeHtml(incident.id)}"
          type="button"
        ></button>
      `;
    }).join('');
  }
};
