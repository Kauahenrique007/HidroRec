const path = require('path');

module.exports = {
  nodeEnv: process.env.NODE_ENV || 'development',
  port: Number(process.env.PORT || 3000),
  apiPrefix: '/api',
  apiVersion: 'v1',
  publicDir: path.join(__dirname, '../../..', 'front-end'),
  assets: {
    js: path.join(__dirname, '../../..', 'js'),
    style: path.join(__dirname, '../../..', 'style'),
    img: path.join(__dirname, '../../..', 'img')
  },
  dbPath: path.join(__dirname, '../../data/db.json'),
  authSecret: process.env.AUTH_SECRET || 'hidrorec-dev-secret',
  authTtlHours: Number(process.env.AUTH_TTL_HOURS || 12),
  corsOrigin: process.env.CORS_ORIGIN || '*',
  externalTimeoutMs: Number(process.env.EXTERNAL_TIMEOUT_MS || 4000)
};
