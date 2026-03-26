const { sendSuccess } = require('../../core/response');
const service = require('./dashboard.service');

async function overview(req, res) {
  const data = await service.getOverview();
  sendSuccess(res, { data });
}

module.exports = {
  overview
};
