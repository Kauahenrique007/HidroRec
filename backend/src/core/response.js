function sendSuccess(res, payload = {}, options = {}) {
  const statusCode = options.statusCode || 200;
  const meta = {
    requestId: res.locals.requestId,
    timestamp: new Date().toISOString(),
    ...(payload.meta || {})
  };

  res.status(statusCode).json({
    success: true,
    message: options.message || payload.message || null,
    data: payload.data ?? null,
    meta
  });
}

function sendError(res, error) {
  res.status(error.statusCode || 500).json({
    success: false,
    error: {
      code: error.code || 'INTERNAL_ERROR',
      message: error.expose ? error.message : 'Erro interno no servidor',
      details: error.expose ? error.details || null : null
    },
    meta: {
      requestId: res.locals.requestId,
      timestamp: new Date().toISOString()
    }
  });
}

module.exports = { sendError, sendSuccess };
