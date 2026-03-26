const authService = require('../modules/auth/auth.service');

module.exports = async function authenticate(req, res, next) {
  try {
    const authorization = req.headers.authorization || '';
    const [, token] = authorization.split(' ');
    req.user = await authService.verifyToken(token);
    next();
  } catch (error) {
    next(error);
  }
};
