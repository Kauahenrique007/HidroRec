const axios = require('axios');
const env = require('../../../config/env');
const { normalizeWarning, safeNumber } = require('./apac.adapter');

function sumHourlyRain(hours = [], limit = 24) {
  return hours
    .slice(0, limit)
    .reduce((total, item) => total + (safeNumber(item.precip_mm, item.precipMm) || 0), 0);
}

function normalizeWeatherApiSnapshot(payload, database) {
  const current = payload.current || {};
  const location = payload.location || {};
  const forecastDays = payload.forecast?.forecastday || [];
  const currentDayHours = forecastDays[0]?.hour || [];
  const upcomingHours = currentDayHours.filter((hour) => {
    if (!hour?.time_epoch) return true;
    return hour.time_epoch * 1000 >= Date.now();
  });

  const next24hHours = [...upcomingHours, ...(forecastDays[1]?.hour || [])].slice(0, 24);
  const next24hRain = sumHourlyRain(next24hHours, 24);
  const dailyTotal = safeNumber(forecastDays[0]?.day?.totalprecip_mm, forecastDays[1]?.day?.totalprecip_mm);
  const warningCandidates = payload.alerts?.alert || [];

  return {
    climate: {
      city: location.name || database.systemConfig.city || 'Recife',
      observedRainMm: safeNumber(current.precip_mm, currentDayHours[0]?.precip_mm) ?? 0,
      accumulatedRain24h: safeNumber(dailyTotal, next24hRain, database.climateReadings.at(-1)?.accumulatedRain24h) ?? 0,
      forecastRainMm: safeNumber(next24hRain, dailyTotal, forecastDays[0]?.day?.daily_chance_of_rain) ?? 0,
      temperatureC: safeNumber(current.temp_c, forecastDays[0]?.day?.avgtemp_c) ?? 0,
      humidityPct: safeNumber(current.humidity) ?? 0,
      windKph: safeNumber(current.wind_kph) ?? 0,
      conditionText: current.condition?.text || forecastDays[0]?.day?.condition?.text || 'Previsao WeatherAPI',
      forecastTimeline: next24hHours.slice(0, 8).map((hour) => ({
        time: hour.time,
        temperatureC: safeNumber(hour.temp_c) ?? 0,
        precipMm: safeNumber(hour.precip_mm) ?? 0,
        chanceOfRainPct: safeNumber(hour.chance_of_rain) ?? 0,
        conditionText: hour.condition?.text || ''
      })),
      source: 'WeatherAPI',
      sourceDetails: ['WeatherAPI'],
      collectedAt: new Date().toISOString()
    },
    tide: {
      station: database.tideReadings.at(-1)?.station || 'Porto do Recife',
      levelMeters: database.tideReadings.at(-1)?.levelMeters || 0,
      influence: database.tideReadings.at(-1)?.influence || 'media',
      source: database.tideReadings.at(-1)?.source || 'fallback',
      collectedAt: new Date().toISOString()
    },
    warnings: warningCandidates.length > 0
      ? warningCandidates.map((item, index) =>
          normalizeWarning(
            {
              id: item.headline,
              title: item.headline,
              severity: item.severity || 'atencao',
              source: 'WeatherAPI',
              summary: item.desc,
              validFrom: item.effective,
              validTo: item.expires
            },
            index
          )
        )
      : database.meteorologicalWarnings,
    source: 'WeatherAPI'
  };
}

async function fetchWeatherApiSnapshot(database) {
  if (!env.weatherApiKey) {
    return null;
  }

  try {
    const response = await axios.get(`${env.weatherApiBaseUrl}/forecast.json`, {
      timeout: env.externalTimeoutMs,
      params: {
        key: env.weatherApiKey,
        q: env.weatherApiQuery,
        days: env.weatherApiForecastDays,
        alerts: 'yes',
        aqi: 'no',
        lang: 'pt'
      }
    });

    return normalizeWeatherApiSnapshot(response.data || {}, database);
  } catch (error) {
    return null;
  }
}

module.exports = {
  fetchWeatherApiSnapshot
};
