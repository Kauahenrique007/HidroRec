module.exports = {
  openapi: '3.0.3',
  info: {
    title: 'HidroRec API',
    version: '1.0.0',
    description: 'API operacional versionada para monitoramento hidroclimatico e risco urbano.'
  },
  paths: {
    '/api/v1/auth/login': {
      post: {
        summary: 'Autentica usuario operacional'
      }
    },
    '/api/v1/dashboard/overview': {
      get: {
        summary: 'Retorna resumo executivo do painel'
      }
    },
    '/api/v1/territories': {
      get: {
        summary: 'Lista territorios com paginacao, filtros e score de risco'
      }
    },
    '/api/v1/incidents/public-report': {
      post: {
        summary: 'Registra reporte colaborativo'
      }
    }
  }
};
