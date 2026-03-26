const AppError = require('../core/AppError');
const logger = require('../core/logger');
const { sendError } = require('../core/response');

module.exports = function errorHandler(error, req, res, next) {
  const appError = error instanceof AppError
    ? error
    : new AppError(error.message || 'Erro interno no servidor', {
        statusCode: error.statusCode || 500,
        code: error.code || 'INTERNAL_ERROR',
        expose: false
      });

  logger.error('request_failed', {
    requestId: res.locals.requestId,
    method: req.method,
    path: req.originalUrl,
    statusCode: appError.statusCode,
    code: appError.code,
    stack: error.stack
  });

  sendError(res, appError);
};
