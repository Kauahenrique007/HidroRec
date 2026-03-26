const RISK_LEVELS = {
  LOW: 'baixo',
  MODERATE: 'moderado',
  HIGH: 'alto',
  CRITICAL: 'critico'
};

const SCORE_THRESHOLDS = [
  { min: 75, level: RISK_LEVELS.CRITICAL, label: 'Critico' },
  { min: 50, level: RISK_LEVELS.HIGH, label: 'Alto' },
  { min: 25, level: RISK_LEVELS.MODERATE, label: 'Moderado' },
  { min: 0, level: RISK_LEVELS.LOW, label: 'Baixo' }
];

const WEIGHTS = {
  forecastRain: 20,
  accumulatedRain: 20,
  incidentHistory: 15,
  vulnerability: 15,
  warningSeverity: 15,
  operationalContext: 10,
  tideInfluence: 5
};

const VULNERABILITY_SCORES = {
  baixa: 4,
  media: 9,
  alta: 15
};

const WARNING_SEVERITY_SCORES = {
  none: 0,
  observacao: 5,
  atencao: 10,
  severo: 15
};

const OPERATIONAL_SCORES = {
  rotina: 2,
  mobilizacao: 6,
  contingencia: 10
};

const TIDE_SCORES = {
  baixa: 1,
  media: 3,
  alta: 5
};

module.exports = {
  OPERATIONAL_SCORES,
  RISK_LEVELS,
  SCORE_THRESHOLDS,
  TIDE_SCORES,
  VULNERABILITY_SCORES,
  WARNING_SEVERITY_SCORES,
  WEIGHTS
};
