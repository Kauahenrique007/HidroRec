require('dotenv').config();

const app = require('./src/app');
const env = require('./src/config/env');
const logger = require('./src/core/logger');

app.listen(env.port, () => {
  logger.info('server_started', {
    port: env.port,
    nodeEnv: env.nodeEnv
  });
});
