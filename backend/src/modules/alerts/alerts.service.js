const { paginate, parsePagination } = require('../../core/pagination');
const { readDatabase } = require('../../infrastructure/repositories/jsonDatabase');

function mapAlerts(database) {
  return database.alerts.map((alert) => {
    const territory = database.territories.find((item) => item.id === alert.territoryId);
    const neighborhood = database.neighborhoods.find((item) => item.id === alert.neighborhoodId);

    return {
      ...alert,
      territoryName: territory?.name || null,
      neighborhoodName: neighborhood?.name || territory?.neighborhoodName || 'Nao informado'
    };
  });
}

function filterAlerts(alerts, query = {}) {
  let filtered = [...alerts];

  if (query.status) {
    filtered = filtered.filter((item) => item.status === query.status);
  }

  if (query.severity) {
    filtered = filtered.filter((item) => item.severity === query.severity);
  }

  if (query.source) {
    filtered = filtered.filter((item) => item.source === query.source);
  }

  if (query.neighborhoodName) {
    const neighborhoodName = String(query.neighborhoodName).toLowerCase();
    filtered = filtered.filter((item) => item.neighborhoodName.toLowerCase().includes(neighborhoodName));
  }

  if (query.search) {
    const search = query.search.toLowerCase();
    filtered = filtered.filter(
      (item) =>
        item.title.toLowerCase().includes(search) ||
        item.message.toLowerCase().includes(search) ||
        item.neighborhoodName.toLowerCase().includes(search)
    );
  }

  filtered.sort((left, right) => new Date(right.updatedAt) - new Date(left.updatedAt));
  return filtered;
}

async function listAlerts(query = {}) {
  const database = await readDatabase();
  const pagination = parsePagination(query);
  const alerts = filterAlerts(mapAlerts(database), query);
  return paginate(alerts, pagination);
}

async function getAlertsSummary(query = {}) {
  const database = await readDatabase();
  const alerts = filterAlerts(mapAlerts(database), query);

  return {
    total: alerts.length,
    active: alerts.filter((item) => item.status === 'active').length,
    monitoring: alerts.filter((item) => item.status === 'monitoring').length,
    severe: alerts.filter((item) => item.severity === 'severo').length,
    attention: alerts.filter((item) => item.severity === 'atencao').length,
    neighborhoods: [...new Set(alerts.map((item) => item.neighborhoodName))].length,
    latestUpdatedAt: alerts[0]?.updatedAt || null
  };
}

async function getAlertById(id) {
  const database = await readDatabase();
  const alert = database.alerts.find((item) => String(item.id) === String(id));
  if (!alert) {
    return null;
  }

  const territory = database.territories.find((item) => item.id === alert.territoryId);
  const neighborhood = database.neighborhoods.find((item) => item.id === alert.neighborhoodId);
  const relatedIncidents = database.incidents
    .filter(
      (item) =>
        item.territoryId === alert.territoryId ||
        item.neighborhoodId === alert.neighborhoodId
    )
    .slice(-5)
    .reverse();

  return {
    ...alert,
    territoryName: territory?.name || null,
    neighborhoodName: neighborhood?.name || territory?.neighborhoodName || 'Nao informado',
    relatedIncidents
  };
}

module.exports = {
  getAlertById,
  getAlertsSummary,
  listAlerts
};
