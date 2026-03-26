const crypto = require('crypto');
const AppError = require('../../core/AppError');
const env = require('../../config/env');
const {
  appendAuditLog,
  hashPassword,
  readDatabase,
  updateDatabase
} = require('../../infrastructure/repositories/jsonDatabase');

function toBase64Url(value) {
  return Buffer.from(value).toString('base64url');
}

function sign(data) {
  return crypto.createHmac('sha256', env.authSecret).update(data).digest('base64url');
}

function createToken(user) {
  const header = toBase64Url(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const expiresAt = Date.now() + env.authTtlHours * 60 * 60 * 1000;
  const payload = toBase64Url(
    JSON.stringify({
      sub: user.id,
      role: user.role,
      email: user.email,
      exp: expiresAt
    })
  );
  const signature = sign(`${header}.${payload}`);

  return `${header}.${payload}.${signature}`;
}

function sanitizeUser(user) {
  return {
    id: user.id,
    name: user.name,
    email: user.email,
    role: user.role,
    organization: user.organization,
    permissions: user.permissions,
    status: user.status,
    lastLoginAt: user.lastLoginAt
  };
}

async function login({ email, password, requestMeta }) {
  const database = await readDatabase();
  const user = database.users.find(
    (item) => item.email.toLowerCase() === String(email).trim().toLowerCase()
  );

  if (!user || hashPassword(password, user.salt) !== user.passwordHash) {
    throw new AppError('Credenciais invalidas', {
      statusCode: 401,
      code: 'INVALID_CREDENTIALS'
    });
  }

  const token = createToken(user);

  await updateDatabase((next) => {
    const target = next.users.find((item) => item.id === user.id);
    if (target) {
      target.lastLoginAt = new Date().toISOString();
      target.updatedAt = target.lastLoginAt;
    }
    return next;
  });

  await appendAuditLog({
    actorId: user.id,
    action: 'auth.login',
    resource: 'session',
    metadata: requestMeta
  });

  return {
    token,
    expiresInHours: env.authTtlHours,
    user: sanitizeUser({
      ...user,
      lastLoginAt: new Date().toISOString()
    })
  };
}

async function findUserById(userId) {
  const database = await readDatabase();
  const user = database.users.find((item) => item.id === userId);
  return user ? sanitizeUser(user) : null;
}

async function verifyToken(token) {
  if (!token) {
    throw new AppError('Token ausente', {
      statusCode: 401,
      code: 'AUTH_REQUIRED'
    });
  }

  const [header, payload, signature] = token.split('.');
  if (!header || !payload || !signature || sign(`${header}.${payload}`) !== signature) {
    throw new AppError('Token invalido', {
      statusCode: 401,
      code: 'INVALID_TOKEN'
    });
  }

  const decoded = JSON.parse(Buffer.from(payload, 'base64url').toString('utf8'));
  if (Date.now() > decoded.exp) {
    throw new AppError('Sessao expirada', {
      statusCode: 401,
      code: 'TOKEN_EXPIRED'
    });
  }

  const user = await findUserById(decoded.sub);
  if (!user) {
    throw new AppError('Usuario nao encontrado', {
      statusCode: 401,
      code: 'USER_NOT_FOUND'
    });
  }

  return user;
}

module.exports = {
  findUserById,
  login,
  verifyToken
};
