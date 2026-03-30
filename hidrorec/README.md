# HidroRec

HidroRec é um MVP funcional de monitoramento urbano inteligente para Recife, com frontend em HTML/CSS/JavaScript puro e backend em ASP.NET Core Web API com SQL Server, Entity Framework Core, JWT, FluentValidation, AutoMapper, Serilog, Swagger e Docker.

## Estrutura

```text
hidrorec/
├── frontend/
│   ├── index.html
│   ├── alertas.html
│   ├── previsoes.html
│   ├── admin.html
│   ├── reporte.html
│   ├── css/
│   ├── js/
│   ├── assets/
│   └── components/
└── backend/
    ├── Controllers/
    ├── Application/
    ├── Domain/
    ├── Infrastructure/
    ├── Middlewares/
    ├── Configurations/
    ├── Migrations/
    ├── Program.cs
    ├── appsettings.json
    ├── Dockerfile
    └── docker-compose.yml
```

## O que já está pronto

- Dashboard institucional com status da cidade, indicadores e mapa estilizado.
- Tela de alertas com filtro por criticidade.
- Tela de previsões com indicadores e janelas críticas.
- Tela de reporte colaborativo com geolocalização, seleção visual do nível da água, preview de imagem e envio real para a API.
- Painel administrativo inicial com login, métricas, fila de reportes, atualização de status, auditoria e logs.
- API REST com autenticação JWT e endpoints para auth, dashboard, reportes, alertas, previsões, admin e usuários.
- Persistência com EF Core, seed inicial e migration `InitialCreate`.

## Credenciais seed

- Admin: `admin@hidrorec.local`
- Senha: `HidroRec#2026`

## Rodando com Docker

Na pasta [`backend`](C:/Users/ACPGROUP/Desktop/HTML%20PURO/hidrorec/backend):

```bash
docker compose up --build
```

Depois abra:

- App: [http://localhost:8080](http://localhost:8080)
- Swagger: [http://localhost:8080/swagger](http://localhost:8080/swagger)

## Rodando localmente sem Docker

1. Suba um SQL Server local e ajuste a connection string em [appsettings.Development.json](C:/Users/ACPGROUP/Desktop/HTML%20PURO/hidrorec/backend/appsettings.Development.json) se necessário.
2. Rode a migration:

```bash
dotnet ef database update --project backend/backend.csproj --startup-project backend/backend.csproj
```

3. Inicie a API:

```bash
dotnet run --project backend/backend.csproj
```

4. Abra:

- App: [http://localhost:8080](http://localhost:8080)

## Endpoints principais

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/dashboard/resumo`
- `GET /api/dashboard/indicadores`
- `GET /api/dashboard/pontos-recentes`
- `GET /api/dashboard/mapa`
- `GET /api/dashboard/pontos-atencao`
- `POST /api/reportes`
- `GET /api/reportes`
- `GET /api/reportes/{id}`
- `PUT /api/reportes/{id}`
- `PATCH /api/reportes/{id}/status`
- `DELETE /api/reportes/{id}`
- `GET /api/alertas`
- `GET /api/alertas/ativos`
- `GET /api/previsoes`
- `GET /api/admin/reportes`
- `GET /api/admin/metricas`
- `GET /api/admin/auditoria`
- `GET /api/admin/logs`
- `GET /api/usuarios`

## Observações

- O frontend é servido pela própria API para simplificar o deploy do MVP.
- O backend já está preparado para integrar serviços externos futuros em `Infrastructure/ExternalServices`.
- As imagens dos reportes são salvas em `/uploads`.
