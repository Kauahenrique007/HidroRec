const crypto = require('crypto');

module.exports = function requestContext(req, res, next) {
  const requestId = crypto.randomUUID();
  res.locals.requestId = requestId;
  res.setHeader('X-Request-Id', requestId);
  next();
};
