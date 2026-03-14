const apacService = require('./apacService');
const { readDB, writeDB } = require('../utils/fileUtils');

async function getCurrentClimate() {
  const climaReal = await apacService.getClimaFromAPAC();
  if (climaReal.fonte !== 'fallback') {
    const data = await readDB();
    data.clima.push(climaReal);
    if (data.clima.length > 100) data.clima = data.clima.slice(-100);
    await writeDB(data);
  }
  return climaReal;
}

module.exports = { getCurrentClimate };