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

function clamp(value, min, max) {
  return Math.min(max, Math.max(min, value));
}

function summarizeReadiness(maxRisk, activeAlerts, pendingIncidents) {
  const pressure = clamp(Math.round((maxRisk * 0.55) + (activeAlerts * 9) + (pendingIncidents * 6)), 0, 100);

  if (pressure >= 85) return { score: pressure, label: 'Resposta total mobilizada' };
  if (pressure >= 65) return { score: pressure, label: 'Equipes em reforco' };
  if (pressure >= 40) return { score: pressure, label: 'Plantao em atencao elevada' };
  return { score: pressure, label: 'Operacao estabilizada' };
}

function buildHighlights({ monitoring, metrics, topTerritories, latestAlerts }) {
  const highlights = [];

  if (monitoring.warning?.summary) {
    highlights.push({
      title: 'Aviso meteorologico vigente',
      body: monitoring.warning.summary
    });
  }

  if (topTerritories[0]) {
    highlights.push({
      title: 'Territorio mais pressionado',
      body: `${topTerritories[0].name} com score ${topTerritories[0].risk.score} em ${topTerritories[0].neighborhoodName}.`
    });
  }

  if (latestAlerts[0]) {
    highlights.push({
      title: 'Alerta mais recente',
      body: `${latestAlerts[0].title} com severidade ${latestAlerts[0].severity}.`
    });
  }

  highlights.push({
    title: 'Pendencias de triagem',
    body: `${metrics.pendingIncidents} ocorrencias aguardando validacao operacional.`
  });

  return highlights.slice(0, 4);
}

function buildRecommendations({ monitoring, topTerritories, latestIncidents, metrics }) {
  const actions = [];
  const leadTerritory = topTerritories[0];

  if (monitoring.tide?.influence === 'alta') {
    actions.push({
      title: 'Reforcar eixos sob influencia de mare',
      owner: 'Sala de monitoramento',
      detail: 'Manter observacao ativa em corredores baixos e cruzar cota de mare com drenagem local.',
      priority: 'alta'
    });
  }

  if (leadTerritory) {
    actions.push({
      title: `Priorizar ${leadTerritory.neighborhoodName}`,
      owner: 'Coordenacao territorial',
      detail: `Direcionar checagem para ${leadTerritory.name}, onde o score de risco atingiu ${leadTerritory.risk.score}.`,
      priority: leadTerritory.risk.level === 'critico' ? 'alta' : 'media'
    });
  }

  if (metrics.pendingIncidents > 0) {
    actions.push({
      title: 'Esvaziar fila de validacao',
      owner: 'Equipe de triagem',
      detail: `Existem ${metrics.pendingIncidents} registros pendentes aguardando confirmacao ou descarte.`,
      priority: metrics.pendingIncidents >= 3 ? 'alta' : 'media'
    });
  }

  if (latestIncidents[0]) {
    actions.push({
      title: 'Responder ocorrencia mais nova',
      owner: 'Supervisor de plantao',
      detail: `${latestIncidents[0].type} reportado em ${latestIncidents[0].address}, ${latestIncidents[0].neighborhoodName}.`,
      priority: 'media'
    });
  }

  return actions.slice(0, 4);
}

function buildRiskBreakdown(territories) {
  const base = {
    baixo: 0,
    moderado: 0,
    alto: 0,
    critico: 0
  };

  territories.forEach((territory) => {
    const key = territory.risk?.level || 'baixo';
    if (key in base) {
      base[key] += 1;
    }
  });

  return Object.entries(base).map(([level, count]) => ({
    level,
    count
  }));
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
  const activeAlerts = database.alerts.filter((item) => item.status === 'active').length;
  const pendingIncidents = database.incidents.filter((item) => item.status === 'pendente').length;
  const readiness = summarizeReadiness(maxRisk, activeAlerts, pendingIncidents);
  const recommendations = buildRecommendations({
    monitoring,
    topTerritories: territoriesResult.data.slice(0, 5),
    latestIncidents: incidentsResult.data,
    metrics: {
      pendingIncidents
    }
  });

  return {
    city: database.systemConfig.city,
    operationalStatus: classifyOperationalStatus(maxRisk),
    updatedAt: new Date().toISOString(),
    scenario: {
      readiness,
      summary: monitoring.operationalContext?.description || 'Monitoramento em andamento com consolidacao de leituras ambientais e territoriais.',
      highlights: buildHighlights({
        monitoring,
        metrics: {
          pendingIncidents
        },
        topTerritories: territoriesResult.data.slice(0, 5),
        latestAlerts: alertsResult.data
      }),
      recommendations
    },
    metrics: {
      activeAlerts,
      monitoringAlerts: database.alerts.filter((item) => item.status === 'monitoring').length,
      pendingIncidents,
      criticalTerritories: territoriesResult.data.filter((item) => item.risk.level === 'critico').length,
      averageAccumulatedRain24h: monitoring.climate?.accumulatedRain24h || 0,
      tideLevelMeters: monitoring.tide?.levelMeters || 0,
      readinessScore: readiness.score
    },
    monitoring,
    riskBreakdown: buildRiskBreakdown(territoriesResult.data),
    topTerritories: territoriesResult.data.slice(0, 5),
    latestAlerts: alertsResult.data,
    latestIncidents: incidentsResult.data
  };
}

module.exports = {
  getOverview
};
