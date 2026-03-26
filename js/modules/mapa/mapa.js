import { mapService } from '../../services/mapService.js';

export function renderTerritoryMapPanel(target, territories) {
  target.innerHTML = `
    <div class="map-board">
      <img src="/img/mapa.png" alt="Mapa de referencia do Recife" class="map-board__image">
      <div class="map-board__overlay">
        ${mapService.buildTerritoryMapMarkup(territories)}
      </div>
    </div>
  `;
}
