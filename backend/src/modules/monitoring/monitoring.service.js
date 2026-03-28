const { appendAuditLog, readDatabase, updateDatabase } = require('../../infrastructure/repositories/jsonDatabase');
const { buildFallbackSnapshot, fetchApacSnapshot } = require('../integrations/adapters/apac.adapter');
const { fetchWeatherApiSnapshot } = require('../integrations/adapters/weatherapi.adapter');

function mergeSnapshots(...snapshots) {
  const validSnapshots = snapshots.filter(Boolean);
  const fallback = validSnapshots[0];
  const sources = validSnapshots
    .map((snapshot) => snapshot.source)
    .filter(Boolean);

  const climateCandidates = validSnapshots.map((snapshot) => snapshot.climate).filter(Boolean);
  const tideCandidates = validSnapshots.map((snapshot) => snapshot.tide).filter(Boolean);
  const warningCandidates = validSnapshots.find((snapshot) => Array.isArray(snapshot.warnings) && snapshot.warnings.length > 0)?.warnings
    || fallback.warnings;

  const primaryClimate = climateCandidates.find((item) => item.source === 'APAC')
    || climateCandidates.find((item) => item.source === 'WeatherAPI')
    || climateCandidates[0];
  const weatherClimate = climateCandidates.find((item) => item.source === 'WeatherAPI');
  const primaryTide = tideCandidates.find((item) => item.source === 'APAC') || tideCandidates[0];

  return {
    climate: {
      ...primaryClimate,
      forecastTimeline: weatherClimate?.forecastTimeline || primaryClimate.forecastTimeline || [],
      source: sources.join('+'),
      sourceDetails: [...new Set(validSnapshots.flatMap((snapshot) => snapshot.climate?.sourceDetails || [snapshot.source]).filter(Boolean))]
    },
    tide: {
      ...primaryTide,
      source: primaryTide?.source || fallback.tide.source
    },
    warnings: warningCandidates,
    source: sources.join('+') || fallback.source
  };
}

async function fetchMonitoringSnapshot(database) {
  const fallback = buildFallbackSnapshot(database);
  const [apacSnapshot, weatherSnapshot] = await Promise.all([
    fetchApacSnapshot(database),
    fetchWeatherApiSnapshot(database)
  ]);

  return mergeSnapshots(fallback, apacSnapshot, weatherSnapshot);
}

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
    sources: ['APAC', 'WeatherAPI', 'base local', 'operacao'],
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
