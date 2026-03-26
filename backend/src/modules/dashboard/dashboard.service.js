const { readDatabase } = require('../../infrastructure/repositories/jsonDatabase');
const alertsService = require('../alerts/alerts.service');
const incidentsService = require('../incidents/incidents.service');
const monitoringService = require('../monitoring/monitoring.service');
const territoriesService = require('../territories/territories.service');

function classifyOperationalStatus(score) {
  if (score >= 75) return 'Contingencia';
  if (score >= 50) return 'Resposta ampliada';
  if (score >= 25) return 'Atencao operacional';
  return 'Rotina monitorada';
}

async function getOverview() {
  const [database, monitoring, territoriesResult, alertsResult, incidentsResult] = await Promise.all([
    readDatabase(),
    monitoringService.getMonitoringOverview({ refresh: true }),
    territoriesService.listTerritories({ page: 1, pageSize: 50, sortBy: 'score', order: 'desc' }),
    alertsService.listAlerts({ page: 1, pageSize: 10 }),
    incidentsService.listIncidents({ page: 1, pageSize: 10 })
  ]);

  const territoryRisks = territoriesResult.data.map((item) => item.risk.score);
  const maxRisk = territoryRisks.length > 0 ? Math.max(...territoryRisks) : 0;

  return {
    city: database.systemConfig.city,
    operationalStatus: classifyOperationalStatus(maxRisk),
    updatedAt: new Date().toISOString(),
    metrics: {
      activeAlerts: database.alerts.filter((item) => item.status === 'active').length,
      pendingIncidents: database.incidents.filter((item) => item.status === 'pendente').length,
      criticalTerritories: territoriesResult.data.filter((item) => item.risk.level === 'critico').length,
      averageAccumulatedRain24h: monitoring.climate?.accumulatedRain24h || 0,
      tideLevelMeters: monitoring.tide?.levelMeters || 0
    },
    monitoring,
    topTerritories: territoriesResult.data.slice(0, 5),
    latestAlerts: alertsResult.data,
    latestIncidents: incidentsResult.data
  };
}

module.exports = {
  getOverview
};
