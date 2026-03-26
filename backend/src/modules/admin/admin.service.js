const { readDatabase } = require('../../infrastructure/repositories/jsonDatabase');

function sanitizeUsers(users) {
  return users.map((user) => ({
    id: user.id,
    name: user.name,
    email: user.email,
    role: user.role,
    organization: user.organization,
    status: user.status,
    permissions: user.permissions,
    lastLoginAt: user.lastLoginAt,
    updatedAt: user.updatedAt
  }));
}

async function getOverview() {
  const database = await readDatabase();
  const degradedRuns = database.integrationRuns.filter((item) => item.status === 'degraded').length;

  return {
    summary: {
      activeUsers: database.users.filter((item) => item.status === 'active').length,
      incidentVolume: database.incidents.length,
      alertsVolume: database.alerts.length,
      auditEntries: database.auditLogs.length,
      degradedRuns
    },
    users: sanitizeUsers(database.users),
    auditLogs: [...database.auditLogs].slice(-20).reverse(),
    integrationRuns: [...database.integrationRuns].slice(-10).reverse(),
    config: database.systemConfig,
    docs: {
      openApiPath: '/api/v1/docs',
      apiStatusPath: '/api/status'
    }
  };
}

async function listUsers() {
  const database = await readDatabase();
  return sanitizeUsers(database.users);
}

async function listAuditLogs() {
  const database = await readDatabase();
  return [...database.auditLogs].reverse();
}

module.exports = {
  getOverview,
  listAuditLogs,
  listUsers
};
