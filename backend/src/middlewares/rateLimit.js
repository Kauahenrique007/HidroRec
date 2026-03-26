const AppError = require('../core/AppError');

function createRateLimiter({ windowMs, limit }) {
  const hits = new Map();

  return function rateLimiter(req, res, next) {
    const key = req.ip || req.headers['x-forwarded-for'] || 'anonymous';
    const now = Date.now();
    const record = hits.get(key) || { count: 0, resetAt: now + windowMs };

    if (now > record.resetAt) {
      record.count = 0;
      record.resetAt = now + windowMs;
    }

    record.count += 1;
    hits.set(key, record);

    if (record.count > limit) {
      next(
        new AppError('Limite temporario de requisicoes excedido', {
          statusCode: 429,
          code: 'RATE_LIMIT_EXCEEDED'
        })
      );
      return;
    }

    next();
  };
}

module.exports = { createRateLimiter };
