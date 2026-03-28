const { paginate, parsePagination } = require('../../core/pagination');
const { readDatabase } = require('../../infrastructure/repositories/jsonDatabase');
const riskService = require('../risk/risk.service');

async function enrichTerritories(database) {
  const risks = await riskService.getTerritoryRiskSnapshots();
  const riskMap = new Map(risks.map((item) => [item.territoryId, item]));
  const alertsByNeighborhood = new Map();

  database.alerts.forEach((alert) => {
    const current = alertsByNeighborhood.get(alert.neighborhoodId) || 0;
    alertsByNeighborhood.set(alert.neighborhoodId, current + 1);
  });

  return database.territories.map((territory) => ({
    ...territory,
    activeAlerts: alertsByNeighborhood.get(territory.neighborhoodId) || 0,
    risk: riskMap.get(territory.id)
  }));
}

function sortTerritories(items, sortBy, order) {
  const direction = order === 'asc' ? 1 : -1;
  return [...items].sort((left, right) => {
    const leftValue = sortBy === 'score' ? left.risk.score : left[sortBy] || left.updatedAt;
    const rightValue = sortBy === 'score' ? right.risk.score : right[sortBy] || right.updatedAt;

    if (leftValue > rightValue) return direction;
    if (leftValue < rightValue) return -direction;
    return 0;
  });
}

function filterTerritories(territories, query = {}) {
  let filtered = [...territories];

  if (query.search) {
    const search = query.search.toLowerCase();
    filtered = filtered.filter(
      (item) =>
        item.name.toLowerCase().includes(search) ||
        item.neighborhoodName.toLowerCase().includes(search)
    );
  }

  if (query.riskLevel) {
    filtered = filtered.filter((item) => item.risk.level === query.riskLevel);
  }

  if (query.hasAlerts === 'true') {
    filtered = filtered.filter((item) => item.activeAlerts > 0);
  }

  if (query.minScore) {
    const minScore = Number(query.minScore);
    if (Number.isFinite(minScore)) {
      filtered = filtered.filter((item) => item.risk.score >= minScore);
    }
  }

  if (query.neighborhoodName) {
    const neighborhood = String(query.neighborhoodName).toLowerCase();
    filtered = filtered.filter((item) => item.neighborhoodName.toLowerCase().includes(neighborhood));
  }

  return filtered;
}

async function listTerritories(query = {}) {
  const database = await readDatabase();
  const territories = await enrichTerritories(database);
  const pagination = parsePagination(query);
  const filtered = filterTerritories(territories, query);
  const sorted = sortTerritories(filtered, pagination.sortBy, pagination.order);
  return paginate(sorted, pagination);
}

async function getTerritoriesSummary(query = {}) {
  const database = await readDatabase();
  const territories = filterTerritories(await enrichTerritories(database), query);
  const sortedByScore = sortTerritories(territories, 'score', 'desc');
  const riskLevels = {
    baixo: 0,
    moderado: 0,
    alto: 0,
    critico: 0
  };

  territories.forEach((territory) => {
    if (territory.risk.level in riskLevels) {
      riskLevels[territory.risk.level] += 1;
    }
  });

  return {
    total: territories.length,
    withAlerts: territories.filter((item) => item.activeAlerts > 0).length,
    averageScore: territories.length > 0
      ? Number((territories.reduce((total, item) => total + item.risk.score, 0) / territories.length).toFixed(1))
      : 0,
    highestScore: sortedByScore[0]?.risk.score || 0,
    topTerritory: sortedByScore[0]
      ? {
          id: sortedByScore[0].id,
          name: sortedByScore[0].name,
          neighborhoodName: sortedByScore[0].neighborhoodName,
          score: sortedByScore[0].risk.score
        }
      : null,
    riskLevels
  };
}

async function getTerritoryById(id) {
  const database = await readDatabase();
  const territory = database.territories.find((item) => String(item.id) === String(id));
  if (!territory) {
    return null;
  }

  const risk = await riskService.buildRiskSnapshot(database, territory);
  const incidents = database.incidents.filter(
    (item) =>
      item.territoryId === territory.id ||
      item.neighborhoodId === territory.neighborhoodId
  );

  return {
    ...territory,
    risk,
    incidents: incidents.slice(-5).reverse(),
    alerts: database.alerts.filter((item) => item.neighborhoodId === territory.neighborhoodId)
  };
}

module.exports = {
  getTerritoryById,
  getTerritoriesSummary,
  listTerritories
};
