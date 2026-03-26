const { sendSuccess } = require('../../core/response');
const riskService = require('./risk.service');

async function listRiskSnapshots(req, res) {
  const data = await riskService.getTerritoryRiskSnapshots();
  sendSuccess(res, { data });
}

module.exports = {
  listRiskSnapshots
};
