import { formatRisk } from '../utils/formatters.js';

function scale(value, min, max) {
  if (max === min) return 50;
  return ((value - min) / (max - min)) * 100;
}

export const mapService = {
  buildTerritoryMapMarkup(territories) {
    if (!territories.length) {
      return '<div class="map-placeholder">Sem territorios para exibir.</div>';
    }

    const latitudes = territories.map((item) => item.latitude);
    const longitudes = territories.map((item) => item.longitude);
    const minLat = Math.min(...latitudes);
    const maxLat = Math.max(...latitudes);
    const minLng = Math.min(...longitudes);
    const maxLng = Math.max(...longitudes);

    return territories.map((territory) => {
      const top = 100 - scale(territory.latitude, minLat, maxLat);
      const left = scale(territory.longitude, minLng, maxLng);

      return `
        <button
          class="territory-marker territory-marker--${territory.risk.level}"
          style="top:${top}%; left:${left}%"
          title="${territory.name} - ${formatRisk(territory.risk.level)}"
          type="button"
        >
          <span class="territory-marker__pulse"></span>
          <span class="territory-marker__core">${territory.risk.score}</span>
        </button>
      `;
    }).join('');
  }
};
