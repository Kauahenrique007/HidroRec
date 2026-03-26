const fs = require('fs').promises;
const crypto = require('crypto');
const env = require('../../config/env');

function hashPassword(password, salt) {
  return crypto.createHash('sha256').update(`${password}:${salt}`).digest('hex');
}

function createSeedUser({ id, name, email, role, organization, password }) {
  const salt = `${id}-hidrorec`;
  return {
    id,
    name,
    email,
    role,
    organization,
    status: 'active',
    salt,
    passwordHash: hashPassword(password, salt),
    permissions: role === 'admin'
      ? ['territories:write', 'incidents:write', 'alerts:write', 'users:read']
      : ['incidents:write', 'alerts:read', 'territories:read'],
    lastLoginAt: null,
    createdAt: '2026-03-20T09:00:00.000Z',
    updatedAt: '2026-03-20T09:00:00.000Z'
  };
}

function mapSeverity(value) {
  const severity = String(value || '').toLowerCase();
  if (severity.includes('alto') || severity.includes('crit')) return 'severo';
  if (severity.includes('medio') || severity.includes('aten')) return 'atencao';
  return 'observacao';
}

function mapRiskLevel(value) {
  const normalized = String(value || '').toLowerCase();
  if (normalized === 'alto') return 'alto';
  if (normalized === 'medio') return 'moderado';
  if (normalized === 'baixo') return 'baixo';
  return 'moderado';
}

function defaultDatabase() {
  return {
    organizations: [
      {
        id: 'org-recife-ops',
        name: 'Centro Operacional Recife',
        type: 'institucional'
      }
    ],
    users: [
      createSeedUser({
        id: 'usr-admin',
        name: 'Operacao HidroRec',
        email: 'operacao@hidrorec.local',
        role: 'admin',
        organization: 'Centro Operacional Recife',
        password: 'HidroRec#2026'
      }),
      createSeedUser({
        id: 'usr-analista',
        name: 'Analista Territorial',
        email: 'analista@hidrorec.local',
        role: 'operator',
        organization: 'Centro Operacional Recife',
        password: 'HidroRec#2026'
      })
    ],
    neighborhoods: [],
    territories: [],
    alerts: [],
    incidents: [],
    climateReadings: [],
    tideReadings: [],
    meteorologicalWarnings: [],
    operationalContexts: [],
    integrationRuns: [],
    auditLogs: [],
    systemConfig: {
      city: 'Recife',
      state: 'PE',
      refreshWindowMinutes: 30
    }
  };
}

function normalizeDatabase(raw = {}) {
  const base = defaultDatabase();
  const neighborhoods = raw.neighborhoods || raw.bairros || [];
  const neighborhoodMap = new Map(
    neighborhoods.map((item) => [
      item.id,
      {
        id: item.id,
        name: item.name || item.nome,
        latitude: item.latitude,
        longitude: item.longitude
      }
    ])
  );

  return {
    ...base,
    ...raw,
    neighborhoods: neighborhoods.map((item) => ({
      id: item.id,
      name: item.name || item.nome,
      latitude: item.latitude,
      longitude: item.longitude
    })),
    users: raw.users || base.users,
    territories: (raw.territories || raw.areasRisco || []).map((item) => {
      const neighborhood = neighborhoodMap.get(item.neighborhoodId || item.bairroId);
      return {
        id: item.id,
        name: item.name || item.rua,
        neighborhoodId: item.neighborhoodId || item.bairroId,
        neighborhoodName: item.neighborhoodName || neighborhood?.name || item.bairro || 'Nao informado',
        address: item.address || item.rua,
        latitude: item.latitude,
        longitude: item.longitude,
        vulnerabilityLevel: item.vulnerabilityLevel || mapRiskLevel(item.nivel),
        drainageStatus: item.drainageStatus || 'monitorado',
        historicalIncidents: item.historicalIncidents || 3,
        exposureIndex: item.exposureIndex || 60,
        drainageCapacityIndex: item.drainageCapacityIndex || 45,
        notes: item.notes || item.descricao || '',
        createdAt: item.createdAt || '2026-03-20T09:00:00.000Z',
        updatedAt: item.updatedAt || '2026-03-26T10:00:00.000Z'
      };
    }),
    alerts: (raw.alerts || raw.alertas || []).map((item) => ({
      id: item.id,
      title: item.title || item.titulo,
      territoryId: item.territoryId || item.areaId || null,
      neighborhoodId: item.neighborhoodId || item.bairroId || null,
      severity: item.severity || mapSeverity(item.nivel),
      status: item.status || 'active',
      source: item.source || item.origem || 'sistema',
      message: item.message || item.mensagem,
      createdAt: item.createdAt || item.dataHora || new Date().toISOString(),
      updatedAt: item.updatedAt || item.dataHora || new Date().toISOString()
    })),
    incidents: (raw.incidents || raw.ocorrencias || []).map((item) => ({
      id: item.id,
      territoryId: item.territoryId || null,
      neighborhoodId: item.neighborhoodId || item.bairroId || null,
      neighborhoodName: item.neighborhoodName || neighborhoodMap.get(item.bairroId)?.name || item.bairro || 'Nao informado',
      address: item.address || item.rua || 'Nao informado',
      type: item.type || item.tipo,
      waterLevel: item.waterLevel || item.nivelAgua || 'medio',
      severity: item.severity || mapSeverity(item.nivelAgua || item.nivel),
      description: item.description || item.descricao,
      status: item.status || 'pendente',
      source: item.source || item.origem || 'colaborativa',
      reporterName: item.reporterName || 'Operador nao identificado',
      reporterChannel: item.reporterChannel || (item.origem === 'institucional' ? 'institucional' : 'colaborativo'),
      latitude: item.latitude || null,
      longitude: item.longitude || null,
      createdAt: item.createdAt || item.dataHora || new Date().toISOString(),
      updatedAt: item.updatedAt || item.dataHora || new Date().toISOString()
    })),
    climateReadings: (raw.climateReadings || raw.clima || []).map((item) => ({
      id: item.id,
      city: item.city || item.local || 'Recife',
      observedRainMm: item.observedRainMm ?? item.chuvaMm ?? 0,
      accumulatedRain24h: item.accumulatedRain24h ?? Math.max(Number(item.chuvaMm || 0) * 2.5, 0),
      forecastRainMm: item.forecastRainMm ?? Math.max(Number(item.chuvaMm || 0) * 1.3, 0),
      temperatureC: item.temperatureC ?? item.temperatura ?? null,
      humidityPct: item.humidityPct ?? item.umidade ?? null,
      source: item.source || item.fonte || 'fallback',
      collectedAt: item.collectedAt || item.dataHora || new Date().toISOString()
    })),
    tideReadings: (raw.tideReadings || raw.mare || []).map((item) => ({
      id: item.id,
      station: item.station || item.local || 'Porto do Recife',
      levelMeters: item.levelMeters ?? item.nivel ?? 0,
      influence: item.influence || item.influencia || 'media',
      source: item.source || item.fonte || 'fallback',
      collectedAt: item.collectedAt || item.dataHora || new Date().toISOString()
    })),
    meteorologicalWarnings: raw.meteorologicalWarnings || [
      {
        id: 'warn-001',
        title: 'Aviso hidrometeorologico',
        severity: 'atencao',
        source: 'APAC',
        summary: 'Instabilidade moderada com acumulados relevantes para a madrugada.',
        validFrom: '2026-03-26T06:00:00.000Z',
        validTo: '2026-03-26T18:00:00.000Z'
      }
    ],
    operationalContexts: raw.operationalContexts || [
      {
        id: 'ctx-001',
        status: 'mobilizacao',
        description: 'Equipes monitorando canais e corredores de drenagem.',
        updatedAt: '2026-03-26T10:00:00.000Z'
      }
    ],
    integrationRuns: raw.integrationRuns || [],
    auditLogs: raw.auditLogs || [],
    systemConfig: {
      ...base.systemConfig,
      ...(raw.systemConfig || {})
    }
  };
}

async function readDatabase() {
  const content = await fs.readFile(env.dbPath, 'utf8');
  return normalizeDatabase(JSON.parse(content));
}

async function writeDatabase(data) {
  await fs.writeFile(env.dbPath, JSON.stringify(data, null, 2), 'utf8');
  return data;
}

async function updateDatabase(mutator) {
  const current = await readDatabase();
  const next = await Promise.resolve(mutator(JSON.parse(JSON.stringify(current))));
  return writeDatabase(next);
}

async function appendAuditLog(entry) {
  return updateDatabase((database) => {
    database.auditLogs.push({
      id: crypto.randomUUID(),
      timestamp: new Date().toISOString(),
      ...entry
    });
    database.auditLogs = database.auditLogs.slice(-250);
    return database;
  });
}

module.exports = {
  appendAuditLog,
  hashPassword,
  readDatabase,
  updateDatabase,
  writeDatabase
};
