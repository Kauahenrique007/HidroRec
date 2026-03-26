const axios = require('axios');
const env = require('../../../config/env');

function buildFallbackSnapshot(database) {
  const climate = database.climateReadings.at(-1) || {
    city: 'Recife',
    observedRainMm: 18,
    accumulatedRain24h: 46,
    forecastRainMm: 32,
    temperatureC: 26,
    humidityPct: 84,
    source: 'fallback',
    collectedAt: new Date().toISOString()
  };

  const tide = database.tideReadings.at(-1) || {
    station: 'Porto do Recife',
    levelMeters: 2.1,
    influence: 'alta',
    source: 'fallback',
    collectedAt: new Date().toISOString()
  };

  return {
    climate: {
      ...climate,
      source: 'fallback',
      collectedAt: new Date().toISOString()
    },
    tide: {
      ...tide,
      source: 'fallback',
      collectedAt: new Date().toISOString()
    },
    warnings: database.meteorologicalWarnings,
    source: 'fallback'
  };
}

function normalizeRemotePayload(payload) {
  return {
    climate: {
      city: payload.city || 'Recife',
      observedRainMm: Number(payload.observedRainMm || payload.chuvaMm || 0),
      accumulatedRain24h: Number(payload.accumulatedRain24h || payload.acumulado24h || 0),
      forecastRainMm: Number(payload.forecastRainMm || payload.previsaoMm || 0),
      temperatureC: Number(payload.temperatureC || payload.temperatura || 0),
      humidityPct: Number(payload.humidityPct || payload.umidade || 0),
      source: payload.source || 'APAC',
      collectedAt: new Date().toISOString()
    },
    tide: {
      station: payload.station || 'Porto do Recife',
      levelMeters: Number(payload.levelMeters || payload.nivelMare || 0),
      influence: payload.influence || 'media',
      source: payload.source || 'APAC',
      collectedAt: new Date().toISOString()
    },
    warnings: Array.isArray(payload.warnings) ? payload.warnings : [],
    source: payload.source || 'APAC'
  };
}

async function fetchMonitoringSnapshot(database) {
  const baseUrl = process.env.APAC_API_URL;
  if (!baseUrl) {
    return buildFallbackSnapshot(database);
  }

  try {
    const response = await axios.get(baseUrl, { timeout: env.externalTimeoutMs });
    return normalizeRemotePayload(response.data || {});
  } catch (error) {
    return buildFallbackSnapshot(database);
  }
}

module.exports = {
  fetchMonitoringSnapshot
};
