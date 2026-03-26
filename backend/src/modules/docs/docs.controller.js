const { sendSuccess } = require('../../core/response');
const openapi = require('./openapi');

async function getOpenApi(req, res) {
  sendSuccess(res, { data: openapi });
}

module.exports = {
  getOpenApi
};
