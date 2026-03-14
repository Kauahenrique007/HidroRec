const { readDB } = require('../utils/fileUtils');

async function getActiveAlerts() {
  const data = await readDB();
  return data.alertas.filter(a => a.status === 'ativo');
}

module.exports = { getActiveAlerts };