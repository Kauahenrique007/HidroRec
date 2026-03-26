const {
  OPERATIONAL_SCORES,
  SCORE_THRESHOLDS,
  TIDE_SCORES,
  VULNERABILITY_SCORES,
  WARNING_SEVERITY_SCORES,
  WEIGHTS
} = require('../../constants/risk');
const { readDatabase } = require('../../infrastructure/repositories/jsonDatabase');
const monitoringService = require('../monitoring/monitoring.service');

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value));
}

function computeRainScore(value, maxInput, maxScore) {
  return clamp(Math.round((Number(value || 0) / maxInput) * maxScore), 0, maxScore);
}

function computeIncidentHistoryScore(territory, incidents) {
  const relevant = incidents.filter(
    (item) =>
      item.territoryId === territory.id ||
      item.neighborhoodId === territory.neighborhoodId
  ).length;
  return clamp(relevant * 4, 0, WEIGHTS.incidentHistory);
}

function classifyScore(score) {
  return SCORE_THRESHOLDS.find((item) => score >= item.min);
}

async function buildRiskSnapshot(database, territory) {
  const monitoring = await monitoringService.getMonitoringOverview({ refresh: false });
  const climate = monitoring.climate;
  const tide = monitoring.tide;
  const warning = monitoring.warning;
  const operationalContext = monitoring.operationalContext;

  const forecastRain = computeRainScore(climate?.forecastRainMm, 60, WEIGHTS.forecastRain);
  const accumulatedRain = computeRainScore(climate?.accumulatedRain24h, 120, WEIGHTS.accumulatedRain);
  const incidentHistory = computeIncidentHistoryScore(territory, database.incidents);
  const vulnerability = VULNERABILITY_SCORES[territory.vulnerabilityLevel] || 0;
  const warningSeverity = WARNING_SEVERITY_SCORES[warning?.severity || 'none'] || 0;
  const operational = OPERATIONAL_SCORES[operationalContext?.status || 'rotina'] || 0;
  const tideInfluence = TIDE_SCORES[tide?.influence || 'media'] || 0;

  const score = clamp(
    forecastRain + accumulatedRain + incidentHistory + vulnerability + warningSeverity + operational + tideInfluence,
    0,
    100
  );

  const level = classifyScore(score);
  const explanation = [
    {
      key: 'forecastRain',
      label: 'Chuva prevista',
      value: `${climate?.forecastRainMm ?? 0} mm`,
      contribution: forecastRain
    },
    {
      key: 'accumulatedRain',
      label: 'Acumulado 24h',
      value: `${climate?.accumulatedRain24h ?? 0} mm`,
      contribution: accumulatedRain
    },
    {
      key: 'incidentHistory',
      label: 'Recorrencia historica',
      value: `${territory.historicalIncidents} eventos catalogados`,
      contribution: incidentHistory
    },
    {
      key: 'vulnerability',
      label: 'Vulnerabilidade territorial',
      value: territory.vulnerabilityLevel,
      contribution: vulnerability
    },
    {
      key: 'warningSeverity',
      label: 'Severidade do aviso',
      value: warning?.severity || 'none',
      contribution: warningSeverity
    },
    {
      key: 'operationalContext',
      label: 'Contexto operacional',
      value: operationalContext?.status || 'rotina',
      contribution: operational
    },
    {
      key: 'tideInfluence',
      label: 'Influencia de mare',
      value: tide?.influence || 'media',
      contribution: tideInfluence
    }
  ];

  return {
    territoryId: territory.id,
    score,
    level: level.level,
    label: level.label,
    recommendation:
      score >= 75
        ? 'Acionar contingencia local e monitoramento em campo.'
        : score >= 50
          ? 'Intensificar observacao, drenagem e comunicacao de alerta.'
          : score >= 25
            ? 'Manter vigilancia e preparar resposta operacional.'
            : 'Acompanhar rotina com verificacao preventiva.',
    explanation
  };
}

async function getTerritoryRiskSnapshots() {
  const database = await readDatabase();
  return Promise.all(database.territories.map((territory) => buildRiskSnapshot(database, territory)));
}

module.exports = {
  buildRiskSnapshot,
  getTerritoryRiskSnapshots
};
