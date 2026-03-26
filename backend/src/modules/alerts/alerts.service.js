const { paginate, parsePagination } = require('../../core/pagination');
const { readDatabase } = require('../../infrastructure/repositories/jsonDatabase');

async function listAlerts(query = {}) {
  const database = await readDatabase();
  const pagination = parsePagination(query);

  let alerts = database.alerts.map((alert) => {
    const territory = database.territories.find((item) => item.id === alert.territoryId);
    const neighborhood = database.neighborhoods.find((item) => item.id === alert.neighborhoodId);

    return {
      ...alert,
      territoryName: territory?.name || null,
      neighborhoodName: neighborhood?.name || territory?.neighborhoodName || 'Nao informado'
    };
  });

  if (query.status) {
    alerts = alerts.filter((item) => item.status === query.status);
  }

  if (query.severity) {
    alerts = alerts.filter((item) => item.severity === query.severity);
  }

  if (query.search) {
    const search = query.search.toLowerCase();
    alerts = alerts.filter(
      (item) =>
        item.title.toLowerCase().includes(search) ||
        item.message.toLowerCase().includes(search) ||
        item.neighborhoodName.toLowerCase().includes(search)
    );
  }

  alerts.sort((left, right) => new Date(right.updatedAt) - new Date(left.updatedAt));
  return paginate(alerts, pagination);
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
  listAlerts
};
