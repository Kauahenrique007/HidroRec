const { sendSuccess } = require('../../core/response');
const AppError = require('../../core/AppError');
const service = require('./incidents.service');

async function list(req, res) {
  const result = await service.listIncidents(req.query);
  sendSuccess(res, { data: result.data, meta: result.meta });
}

async function summary(req, res) {
  const data = await service.getIncidentsSummary(req.query);
  sendSuccess(res, { data });
}

async function createPublic(req, res) {
  const incident = await service.createIncident(req.validated.body, null);
  sendSuccess(
    res,
    { data: incident },
    { statusCode: 201, message: 'Reporte colaborativo registrado' }
  );
}

async function getById(req, res) {
  const incident = await service.getIncidentById(req.params.id);
  if (!incident) {
    throw new AppError('Ocorrencia nao encontrada', {
      statusCode: 404,
      code: 'INCIDENT_NOT_FOUND'
    });
  }

  sendSuccess(res, { data: incident });
}

async function createOperational(req, res) {
  const incident = await service.createIncident(req.validated.body, req.user);
  sendSuccess(
    res,
    { data: incident },
    { statusCode: 201, message: 'Ocorrencia operacional registrada' }
  );
}

async function updateStatus(req, res) {
  const incident = await service.updateIncidentStatus(req.params.id, req.validated.body, req.user);
  sendSuccess(res, { data: incident }, { message: 'Status da ocorrencia atualizado' });
}

module.exports = {
  createOperational,
  createPublic,
  getById,
  summary,
  list,
  updateStatus
};
