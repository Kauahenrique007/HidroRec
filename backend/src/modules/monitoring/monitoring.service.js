const { appendAuditLog, readDatabase, updateDatabase } = require('../../infrastructure/repositories/jsonDatabase');
const { fetchMonitoringSnapshot } = require('../integrations/adapters/apac.adapter');

function uniqueByTimestamp(items, key) {
  const seen = new Set();
  return items.filter((item) => {
    const signature = `${item[key]}:${item.source}`;
    if (seen.has(signature)) {
      return false;
    }
    seen.add(signature);
    return true;
  });
}

async function refreshMonitoring(refresh = false) {
  const database = await readDatabase();
  const latestClimate = database.climateReadings.at(-1);
  const stale = !latestClimate || Date.now() - new Date(latestClimate.collectedAt).getTime() > 30 * 60 * 1000;

  if (!refresh && !stale) {
    return database;
  }

  const snapshot = await fetchMonitoringSnapshot(database);

  return updateDatabase((next) => {
    next.climateReadings.push({
      id: `climate-${Date.now()}`,
      ...snapshot.climate
    });
    next.climateReadings = uniqueByTimestamp(next.climateReadings, 'collectedAt').slice(-48);

    next.tideReadings.push({
      id: `tide-${Date.now()}`,
      ...snapshot.tide
    });
    next.tideReadings = uniqueByTimestamp(next.tideReadings, 'collectedAt').slice(-48);

    if (snapshot.warnings.length > 0) {
      next.meteorologicalWarnings = snapshot.warnings;
    }

    next.integrationRuns.push({
      id: `run-${Date.now()}`,
      source: snapshot.source,
      collectedAt: new Date().toISOString(),
      status: snapshot.source === 'fallback' ? 'degraded' : 'ok'
    });
    next.integrationRuns = next.integrationRuns.slice(-30);
    return next;
  });
}

async function getMonitoringOverview(options = {}) {
  const database = await refreshMonitoring(options.refresh);
  const climate = database.climateReadings.at(-1);
  const tide = database.tideReadings.at(-1);
  const warning = database.meteorologicalWarnings[0] || null;
  const operationalContext = database.operationalContexts.at(-1) || null;

  return {
    climate,
    tide,
    warning,
    operationalContext
  };
}

async function getMonitoringTimeline(limit = 12) {
  const database = await refreshMonitoring(false);
  return {
    climateReadings: database.climateReadings.slice(-limit).reverse(),
    tideReadings: database.tideReadings.slice(-limit).reverse()
  };
}

async function getIntegrationStatus() {
  const database = await readDatabase();
  const lastRun = database.integrationRuns.at(-1) || null;

  return {
    lastRun,
    mode: lastRun?.status === 'degraded' ? 'fallback' : 'live',
    sources: ['APAC', 'base local', 'operacao'],
    refreshWindowMinutes: database.systemConfig.refreshWindowMinutes
  };
}

async function auditPublicSubmission(entry) {
  await appendAuditLog(entry);
}

module.exports = {
  auditPublicSubmission,
  getIntegrationStatus,
  getMonitoringOverview,
  getMonitoringTimeline,
  refreshMonitoring
};
