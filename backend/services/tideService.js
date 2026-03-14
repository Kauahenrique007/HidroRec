const apacService = require('./apacService');
const { readDB, writeDB } = require('../utils/fileUtils');

async function getCurrentTide() {
  const mareReal = await apacService.getMareFromAPAC();
  if (mareReal.fonte !== 'fallback') {
    const data = await readDB();
    data.mare.push(mareReal);
    if (data.mare.length > 100) data.mare = data.mare.slice(-100);
    await writeDB(data);
  }
  return mareReal;
}

module.exports = { getCurrentTide };