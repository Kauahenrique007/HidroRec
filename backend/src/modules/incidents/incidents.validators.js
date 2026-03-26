function normalizeString(value, field, min = 1) {
  if (typeof value !== 'string' || value.trim().length < min) {
    throw new Error(`Campo ${field} invalido`);
  }
  return value.trim();
}

function optionalString(value) {
  return typeof value === 'string' ? value.trim() : '';
}

function createPublicIncidentValidator(req) {
  return {
    body: {
      neighborhoodName: normalizeString(req.body.neighborhoodName, 'neighborhoodName', 2),
      address: normalizeString(req.body.address, 'address', 3),
      type: normalizeString(req.body.type, 'type', 3),
      waterLevel: normalizeString(req.body.waterLevel, 'waterLevel', 3),
      description: normalizeString(req.body.description, 'description', 8),
      reporterName: optionalString(req.body.reporterName) || 'Colaborador anonimo',
      latitude: req.body.latitude ? Number(req.body.latitude) : null,
      longitude: req.body.longitude ? Number(req.body.longitude) : null
    }
  };
}

function createOperationalIncidentValidator(req) {
  return {
    body: {
      neighborhoodName: normalizeString(req.body.neighborhoodName, 'neighborhoodName', 2),
      address: normalizeString(req.body.address, 'address', 3),
      type: normalizeString(req.body.type, 'type', 3),
      waterLevel: normalizeString(req.body.waterLevel, 'waterLevel', 3),
      description: normalizeString(req.body.description, 'description', 8),
      reporterName: normalizeString(req.body.reporterName, 'reporterName', 3),
      status: optionalString(req.body.status) || 'em_analise',
      source: optionalString(req.body.source) || 'operacional',
      latitude: req.body.latitude ? Number(req.body.latitude) : null,
      longitude: req.body.longitude ? Number(req.body.longitude) : null
    }
  };
}

function updateIncidentStatusValidator(req) {
  return {
    body: {
      status: normalizeString(req.body.status, 'status', 3),
      note: optionalString(req.body.note)
    }
  };
}

module.exports = {
  createOperationalIncidentValidator,
  createPublicIncidentValidator,
  updateIncidentStatusValidator
};
