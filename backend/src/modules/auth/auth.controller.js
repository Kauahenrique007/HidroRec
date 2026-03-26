const { sendSuccess } = require('../../core/response');
const authService = require('./auth.service');

async function login(req, res) {
  const result = await authService.login({
    ...req.validated.body,
    requestMeta: {
      ip: req.ip,
      userAgent: req.headers['user-agent'] || 'unknown'
    }
  });

  sendSuccess(
    res,
    { data: result },
    { statusCode: 200, message: 'Sessao iniciada com sucesso' }
  );
}

async function me(req, res) {
  sendSuccess(res, { data: req.user });
}

module.exports = {
  login,
  me
};
