function requireString(value, field, min = 1) {
  if (typeof value !== 'string' || value.trim().length < min) {
    throw new Error(`Campo ${field} invalido`);
  }
  return value.trim();
}

function loginValidator(req) {
  return {
    body: {
      email: requireString(req.body.email, 'email', 5).toLowerCase(),
      password: requireString(req.body.password, 'password', 6)
    }
  };
}

module.exports = {
  loginValidator
};
