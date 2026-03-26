const AppError = require('../core/AppError');

module.exports = function authorize(roles = []) {
  return function authorizationMiddleware(req, res, next) {
    if (!req.user) {
      next(
        new AppError('Autenticacao obrigatoria', {
          statusCode: 401,
          code: 'AUTH_REQUIRED'
        })
      );
      return;
    }

    if (roles.length > 0 && !roles.includes(req.user.role)) {
      next(
        new AppError('Acesso negado para este perfil', {
          statusCode: 403,
          code: 'FORBIDDEN'
        })
      );
      return;
    }

    next();
  };
};
