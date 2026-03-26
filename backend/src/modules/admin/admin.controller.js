const { sendSuccess } = require('../../core/response');
const service = require('./admin.service');

async function overview(req, res) {
  const data = await service.getOverview();
  sendSuccess(res, { data });
}

async function listUsers(req, res) {
  const data = await service.listUsers();
  sendSuccess(res, { data });
}

async function listAuditLogs(req, res) {
  const data = await service.listAuditLogs();
  sendSuccess(res, { data });
}

module.exports = {
  listAuditLogs,
  listUsers,
  overview
};
