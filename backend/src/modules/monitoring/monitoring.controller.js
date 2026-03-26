const { sendSuccess } = require('../../core/response');
const service = require('./monitoring.service');

async function getOverview(req, res) {
  const data = await service.getMonitoringOverview({
    refresh: String(req.query.refresh || '').toLowerCase() === 'true'
  });
  sendSuccess(res, { data });
}

async function getTimeline(req, res) {
  const limit = Math.min(24, Math.max(3, Number(req.query.limit) || 12));
  const data = await service.getMonitoringTimeline(limit);
  sendSuccess(res, { data });
}

async function getIntegrations(req, res) {
  const data = await service.getIntegrationStatus();
  sendSuccess(res, { data });
}

module.exports = {
  getIntegrations,
  getOverview,
  getTimeline
};
