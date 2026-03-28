const { readDatabase } = require('../../infrastructure/repositories/jsonDatabase');

function sanitizeUsers(users) {
  return users.map((user) => ({
    id: user.id,
    name: user.name,
    email: user.email,
    role: user.role,
    organization: user.organization,
    status: user.status,
    permissions: user.permissions,
    lastLoginAt: user.lastLoginAt,
    updatedAt: user.updatedAt
  }));
}

function mapNeighborhoodRegion(name = '') {
  const normalized = String(name).toLowerCase();
  if (normalized.includes('boa viagem') || normalized.includes('pina') || normalized.includes('imbiribeira')) return 'Sul';
  if (normalized.includes('centro') || normalized.includes('santo antonio') || normalized.includes('aurora')) return 'Centro';
  if (normalized.includes('torre') || normalized.includes('espinheiro') || normalized.includes('casa forte')) return 'Norte';
  return 'Oeste';
}

async function getOverview() {
  const database = await readDatabase();
  const degradedRuns = database.integrationRuns.filter((item) => item.status === 'degraded').length;
  const incidentsByTerritory = new Map();
  const sensorBase = [
    { name: 'Caxanga', status: 'online', readingAgeMinutes: 2 },
    { name: 'Espinheiro', status: 'online', readingAgeMinutes: 1 },
    { name: 'Pina', status: 'online', readingAgeMinutes: 3 },
    { name: 'Centro', status: 'maintenance', readingAgeMinutes: 45 },
    { name: 'Casa Forte', status: 'online', readingAgeMinutes: 2 },
    { name: 'Boa Viagem', status: 'online', readingAgeMinutes: 1 }
  ];

  database.incidents.forEach((incident) => {
    const key = incident.territoryId || incident.neighborhoodName || incident.address;
    incidentsByTerritory.set(key, (incidentsByTerritory.get(key) || 0) + 1);
  });

  const recurringHotspots = database.territories.map((territory) => {
    const occurrences = incidentsByTerritory.get(territory.id) || territory.historicalIncidents || 0;
    return {
      territoryName: territory.name,
      occurrences,
      responseMinutes: Math.max(20, 48 - occurrences)
    };
  }).sort((left, right) => right.occurrences - left.occurrences).slice(0, 5);

  const responseByRegionMap = new Map();
  database.territories.forEach((territory) => {
    const region = mapNeighborhoodRegion(territory.neighborhoodName);
    const current = responseByRegionMap.get(region) || {
      region,
      timeMinutes: 0,
      efficiencyPct: 0,
      count: 0
    };

    current.timeMinutes += Math.max(12, 36 - Math.round((territory.drainageCapacityIndex || 40) / 4));
    current.efficiencyPct += Math.min(100, Math.max(45, territory.drainageCapacityIndex + 30));
    current.count += 1;
    responseByRegionMap.set(region, current);
  });

  const responseByRegion = [...responseByRegionMap.values()].map((item) => ({
    region: item.region,
    timeMinutes: Math.round(item.timeMinutes / item.count),
    efficiencyPct: Math.round(item.efficiencyPct / item.count)
  }));

  const activityFeed = [
    ...database.alerts.slice(-3).map((alert) => ({
      time: new Date(alert.updatedAt).toISOString(),
      message: `${alert.title}: ${alert.neighborhoodId || alert.territoryId || 'territorio monitorado'}`,
      tone: alert.severity === 'severo' ? 'danger' : 'warning'
    })),
    ...database.incidents.slice(-3).map((incident) => ({
      time: new Date(incident.updatedAt).toISOString(),
      message: `Reporte de usuario recebido: ${incident.neighborhoodName}`,
      tone: incident.source === 'colaborativa' ? 'info' : 'success'
    })),
    {
      time: new Date().toISOString(),
      message: 'Previsao atualizada: Probabilidade 45%',
      tone: 'accent'
    }
  ].sort((left, right) => new Date(right.time) - new Date(left.time)).slice(0, 8);

  const sensorsSummary = {
    online: sensorBase.filter((item) => item.status === 'online').length,
    offline: sensorBase.filter((item) => item.status === 'offline').length,
    maintenance: sensorBase.filter((item) => item.status === 'maintenance').length,
    sensors: sensorBase
  };

  return {
    summary: {
      activeUsers: database.users.filter((item) => item.status === 'active').length,
      incidentVolume: database.incidents.length,
      alertsVolume: database.alerts.length,
      auditEntries: database.auditLogs.length,
      degradedRuns,
      reportsToday: database.incidents.filter((item) => item.createdAt?.startsWith(new Date().toISOString().slice(0, 10))).length,
      efficiencyPct: 89,
      trendPct: 15,
      averageResponseMinutes: 23
    },
    users: sanitizeUsers(database.users),
    auditLogs: [...database.auditLogs].slice(-20).reverse(),
    integrationRuns: [...database.integrationRuns].slice(-10).reverse(),
    recurringHotspots,
    responseByRegion,
    sensorsSummary,
    activityFeed,
    config: database.systemConfig,
    docs: {
      openApiPath: '/api/v1/docs',
      apiStatusPath: '/api/status'
    },
    performance: {
      normalizationMinutes: 32,
      predictiveAccuracyPct: 87,
      citizenEngagement: '1.2k'
    }
  };
}

async function listUsers() {
  const database = await readDatabase();
  return sanitizeUsers(database.users);
}

async function listAuditLogs() {
  const database = await readDatabase();
  return [...database.auditLogs].reverse();
}

module.exports = {
  getOverview,
  listAuditLogs,
  listUsers
};
