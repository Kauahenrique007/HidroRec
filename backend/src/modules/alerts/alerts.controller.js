const { sendSuccess } = require('../../core/response');
const AppError = require('../../core/AppError');
const service = require('./alerts.service');

async function list(req, res) {
  const result = await service.listAlerts(req.query);
  sendSuccess(res, { data: result.data, meta: result.meta });
}

async function getById(req, res) {
  const alert = await service.getAlertById(req.params.id);
  if (!alert) {
    throw new AppError('Alerta nao encontrado', {
      statusCode: 404,
      code: 'ALERT_NOT_FOUND'
    });
  }

  sendSuccess(res, { data: alert });
}

module.exports = {
  getById,
  list
};
