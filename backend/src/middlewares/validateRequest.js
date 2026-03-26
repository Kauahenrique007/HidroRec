const AppError = require('../core/AppError');

module.exports = function validateRequest(validator) {
  return function validationMiddleware(req, res, next) {
    try {
      req.validated = validator(req);
      next();
    } catch (error) {
      next(
        new AppError(error.message || 'Payload invalido', {
          statusCode: 400,
          code: 'VALIDATION_ERROR',
          details: error.details || null
        })
      );
    }
  };
};
