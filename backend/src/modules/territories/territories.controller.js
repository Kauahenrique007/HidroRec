const AppError = require('../../core/AppError');
const { sendSuccess } = require('../../core/response');
const service = require('./territories.service');

async function list(req, res) {
  const result = await service.listTerritories(req.query);
  sendSuccess(res, { data: result.data, meta: result.meta });
}

async function summary(req, res) {
  const data = await service.getTerritoriesSummary(req.query);
  sendSuccess(res, { data });
}

async function getById(req, res) {
  const territory = await service.getTerritoryById(req.params.id);
  if (!territory) {
    throw new AppError('Territorio nao encontrado', {
      statusCode: 404,
      code: 'TERRITORY_NOT_FOUND'
    });
  }

  sendSuccess(res, { data: territory });
}

module.exports = {
  getById,
  list,
  summary
};
