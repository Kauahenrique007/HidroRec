import { formatStatusClass } from './utils.js';

const BOUNDS = {
  minLat: -8.14,
  maxLat: -8.01,
  minLon: -34.97,
  maxLon: -34.84
};

function normalize(lat, lon) {
  const x = ((lon - BOUNDS.minLon) / (BOUNDS.maxLon - BOUNDS.minLon)) * 100;
  const y = (1 - (lat - BOUNDS.minLat) / (BOUNDS.maxLat - BOUNDS.minLat)) * 100;
  return { x, y };
}

export function renderMap(container, points) {
  container.querySelectorAll('.map-marker').forEach((node) => node.remove());

  points.forEach((point) => {
    const marker = document.createElement('div');
    const { x, y } = normalize(Number(point.latitude), Number(point.longitude));
    const variant = formatStatusClass(point.severidade);

    marker.className = 'map-marker';
    marker.style.left = `${Math.max(8, Math.min(92, x))}%`;
    marker.style.top = `${Math.max(14, Math.min(88, y))}%`;
    marker.innerHTML = `
      <div class="map-marker__pin map-marker__pin--${variant}"></div>
      <div class="map-marker__label">
        <strong>${point.titulo}</strong><br>
        <small>${point.bairro} | ${point.scoreRisco}</small>
      </div>
    `;

    container.appendChild(marker);
  });
}
