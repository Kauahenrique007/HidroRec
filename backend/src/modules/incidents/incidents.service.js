const crypto = require('crypto');
const AppError = require('../../core/AppError');
const { paginate, parsePagination } = require('../../core/pagination');
const {
  appendAuditLog,
  readDatabase,
  updateDatabase
} = require('../../infrastructure/repositories/jsonDatabase');

function severityFromWaterLevel(value) {
  const level = String(value || '').toLowerCase();
  if (level.includes('alto') || level.includes('cintura') || level.includes('peito')) return 'severo';
  if (level.includes('medio') || level.includes('joelho')) return 'atencao';
  return 'observacao';
}

async function listIncidents(query = {}) {
  const database = await readDatabase();
  const pagination = parsePagination(query);
  let incidents = [...database.incidents];

  if (query.status) {
    incidents = incidents.filter((item) => item.status === query.status);
  }

  if (query.source) {
    incidents = incidents.filter((item) => item.source === query.source);
  }

  if (query.search) {
    const search = query.search.toLowerCase();
    incidents = incidents.filter(
      (item) =>
        item.neighborhoodName.toLowerCase().includes(search) ||
        item.address.toLowerCase().includes(search) ||
        item.description.toLowerCase().includes(search)
    );
  }

  incidents.sort((left, right) => new Date(right.createdAt) - new Date(left.createdAt));
  return paginate(incidents, pagination);
}

async function getIncidentById(id) {
  const database = await readDatabase();
  const incident = database.incidents.find((item) => String(item.id) === String(id));
  if (!incident) {
    return null;
  }

  const territory = database.territories.find((item) => item.id === incident.territoryId);
  const neighborhood = database.neighborhoods.find((item) => item.id === incident.neighborhoodId);

  return {
    ...incident,
    territoryName: territory?.name || null,
    neighborhoodName: neighborhood?.name || incident.neighborhoodName
  };
}

async function createIncident(payload, actor) {
  let incident = {
    id: crypto.randomUUID(),
    territoryId: null,
    neighborhoodId: null,
    neighborhoodName: payload.neighborhoodName,
    address: payload.address,
    type: payload.type,
    waterLevel: payload.waterLevel,
    severity: severityFromWaterLevel(payload.waterLevel),
    description: payload.description,
    status: payload.status || 'pendente',
    source: payload.source || 'colaborativa',
    reporterName: payload.reporterName,
    reporterChannel: actor?.role ? 'operacional' : 'colaborativo',
    latitude: Number.isFinite(payload.latitude) ? payload.latitude : null,
    longitude: Number.isFinite(payload.longitude) ? payload.longitude : null,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  };

  await updateDatabase((database) => {
    const neighborhood = database.neighborhoods.find(
      (item) => item.name.toLowerCase() === incident.neighborhoodName.toLowerCase()
    );
    const territory = database.territories.find(
      (item) =>
        item.neighborhoodId === neighborhood?.id ||
        item.neighborhoodName.toLowerCase() === incident.neighborhoodName.toLowerCase()
    );

    incident = {
      ...incident,
      neighborhoodId: neighborhood?.id || null,
      territoryId: territory?.id || null
    };

    database.incidents.push(incident);
    return database;
  });

  await appendAuditLog({
    actorId: actor?.id || 'public',
    action: 'incident.create',
    resource: 'incident',
    resourceId: incident.id,
    metadata: {
      source: incident.source,
      neighborhoodName: incident.neighborhoodName
    }
  });

  return incident;
}

async function updateIncidentStatus(id, payload, actor) {
  let updatedIncident = null;

  await updateDatabase((database) => {
    const incident = database.incidents.find((item) => String(item.id) === String(id));
    if (!incident) {
      throw new AppError('Ocorrencia nao encontrada', {
        statusCode: 404,
        code: 'INCIDENT_NOT_FOUND'
      });
    }

    incident.status = payload.status;
    incident.updatedAt = new Date().toISOString();
    incident.operationalNote = payload.note || '';
    incident.reviewedBy = actor.name;
    updatedIncident = incident;
    return database;
  });

  await appendAuditLog({
    actorId: actor.id,
    action: 'incident.update_status',
    resource: 'incident',
    resourceId: updatedIncident.id,
    metadata: {
      status: updatedIncident.status
    }
  });

  return updatedIncident;
}

module.exports = {
  createIncident,
  getIncidentById,
  listIncidents,
  updateIncidentStatus
};
