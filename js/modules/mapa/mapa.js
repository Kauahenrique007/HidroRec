import { mapService } from '../../services/mapService.js';

export function renderTerritoryMapPanel(target, territories) {
  target.innerHTML = `
    <div class="map-board">
      <div class="map-board__grid"></div>
      <div class="map-board__river"></div>
      <div class="map-board__overlay">
        ${mapService.buildTerritoryMapMarkup(territories)}
      </div>
      <div class="map-legend">
        <strong>Legenda</strong>
        <span><i class="legend-dot legend-dot--baixo"></i>Normal</span>
        <span><i class="legend-dot legend-dot--moderado"></i>Atencao</span>
        <span><i class="legend-dot legend-dot--alto"></i>Alagamento</span>
      </div>
    </div>
  `;
}
