// mapa.js
let map;
let markersLayer;

function initMap() {
  map = L.map('map').setView([-8.0476, -34.8770], 12); // Centro do Recife

  L.tileLayer(CONFIG.MAP_TILE_LAYER, {
    attribution: CONFIG.MAP_ATTRIBUTION
  }).addTo(map);

  markersLayer = L.layerGroup().addTo(map);
  
  loadMapData();
}

async function loadMapData() {
  try {
    const [ocorrencias, areasRisco] = await Promise.all([
      api.get('/ocorrencias'),
      api.get('/areas-risco')
    ]);
    
    renderMarkers(ocorrencias, 'ocorrencia');
    renderMarkers(areasRisco, 'area-risco');
  } catch (error) {
    console.error('Erro ao carregar dados do mapa:', error);
  }
}

function renderMarkers(items, type) {
  items.forEach(item => {
    const lat = item.latitude;
    const lng = item.longitude;
    if (!lat || !lng) return;

    const nivel = item.nivel || item.nivelAgua || 'medio';
    const color = nivel === 'alto' ? 'red' : nivel === 'medio' ? 'orange' : 'green';
    
    const marker = L.circleMarker([lat, lng], {
      radius: 8,
      fillColor: color,
      color: '#fff',
      weight: 2,
      opacity: 1,
      fillOpacity: 0.8
    }).bindPopup(`
      <b>${item.rua || item.descricao || 'Ponto'}</b><br>
      ${item.bairro ? 'Bairro: ' + item.bairro : ''}<br>
      Nível: <span style="color:${color}">${nivel}</span><br>
      <a href="/pages/detalhes.html?id=${item.id}&type=${type}">Ver detalhes</a>
    `);
    
    markersLayer.addLayer(marker);
  });
}

document.addEventListener('DOMContentLoaded', initMap);