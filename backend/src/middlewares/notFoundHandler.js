const AppError = require('../core/AppError');

module.exports = function notFoundHandler(req, res, next) {
  next(new AppError('Rota nao encontrada', {
    statusCode: 404,
    code: 'ROUTE_NOT_FOUND'
  }));
};
