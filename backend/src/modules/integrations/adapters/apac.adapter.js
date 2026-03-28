const axios = require('axios');
const env = require('../../../config/env');

function safeNumber(...values) {
  for (const value of values) {
    const numeric = Number(value);
    if (Number.isFinite(numeric)) {
      return numeric;
    }
  }

  return null;
}

function inferTideInfluence(levelMeters) {
  if (!Number.isFinite(levelMeters)) return 'media';
  if (levelMeters >= 2.2) return 'alta';
  if (levelMeters >= 1.7) return 'media';
  return 'baixa';
}

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
      sourceDetails: ['base local'],
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

function normalizeWarning(item, index = 0) {
  if (!item || typeof item !== 'object') return null;

  return {
    id: item.id || `warning-${index + 1}`,
    title: item.title || item.titulo || item.headline || 'Aviso meteorologico',
    severity: item.severity || item.severidade || item.level || 'atencao',
    source: item.source || item.fonte || 'APAC',
    summary: item.summary || item.resumo || item.description || item.descricao || '',
    validFrom: item.validFrom || item.inicio || item.startsAt || new Date().toISOString(),
    validTo: item.validTo || item.fim || item.endsAt || new Date().toISOString()
  };
}

function normalizeApacSnapshot(payload, database) {
  const climatePayload = payload.climate || payload.tempo || payload.weather || payload;
  const tidePayload = payload.tide || payload.mare || payload.ocean || {};
  const warningsPayload = payload.warnings || payload.alerts || payload.avisos || database.meteorologicalWarnings;

  const observedRainMm = safeNumber(
    climatePayload.observedRainMm,
    climatePayload.chuvaMm,
    climatePayload.precip_mm,
    climatePayload.observed_rain_mm
  );
  const accumulatedRain24h = safeNumber(
    climatePayload.accumulatedRain24h,
    climatePayload.acumulado24h,
    climatePayload.rain24hMm,
    climatePayload.accumulated_rain_24h_mm
  );
  const forecastRainMm = safeNumber(
    climatePayload.forecastRainMm,
    climatePayload.previsaoMm,
    climatePayload.forecast_mm,
    climatePayload.next24hRainMm
  );

  return {
    climate: {
      city: climatePayload.city || climatePayload.cidade || database.systemConfig.city || 'Recife',
      observedRainMm: observedRainMm ?? 0,
      accumulatedRain24h: accumulatedRain24h ?? observedRainMm ?? 0,
      forecastRainMm: forecastRainMm ?? accumulatedRain24h ?? 0,
      temperatureC: safeNumber(climatePayload.temperatureC, climatePayload.temperatura, climatePayload.temp_c) ?? 0,
      humidityPct: safeNumber(climatePayload.humidityPct, climatePayload.umidade, climatePayload.humidity) ?? 0,
      conditionText: climatePayload.conditionText || climatePayload.condicao || 'Leitura APAC',
      source: 'APAC',
      sourceDetails: ['APAC'],
      collectedAt: new Date().toISOString()
    },
    tide: {
      station: tidePayload.station || tidePayload.estacao || 'Porto do Recife',
      levelMeters: safeNumber(tidePayload.levelMeters, tidePayload.nivelMare, tidePayload.level_meters) ?? database.tideReadings.at(-1)?.levelMeters ?? 0,
      influence: tidePayload.influence || tidePayload.influencia || inferTideInfluence(safeNumber(tidePayload.levelMeters, tidePayload.nivelMare)),
      source: 'APAC',
      collectedAt: new Date().toISOString()
    },
    warnings: Array.isArray(warningsPayload)
      ? warningsPayload.map(normalizeWarning).filter(Boolean)
      : database.meteorologicalWarnings,
    source: 'APAC'
  };
}

async function fetchApacSnapshot(database) {
  if (!env.apacApiUrl) {
    return null;
  }

  try {
    const response = await axios.get(env.apacApiUrl, {
      timeout: env.externalTimeoutMs,
      headers: env.apacApiToken ? { Authorization: `Bearer ${env.apacApiToken}` } : undefined
    });

    return normalizeApacSnapshot(response.data || {}, database);
  } catch (error) {
    return null;
  }
}

module.exports = {
  buildFallbackSnapshot,
  fetchApacSnapshot,
  inferTideInfluence,
  normalizeWarning,
  safeNumber
};
