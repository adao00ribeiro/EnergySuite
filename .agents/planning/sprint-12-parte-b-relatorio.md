# Sprint 12 - Parte B (Frontend) - Erradicação de Mocks - Relatório

Escopo: EnergySuite frontend (Angular 18 MFE, standalone, Signals). Regras aplicadas: sem NgModule, sem CSS inline que quebre o Design System, `ng build` limpo, remoção total de mocks, URLs de backend centralizadas em `environments`.

## 1. Builds (todos OK)

| MFE | Comando | Resultado |
|---|---|---|
| mf-pricing | `ng build` | PASS (sem erros) |
| mf-hydrology | `ng build` | PASS (sem erros) |
| app-shell | `ng build` | PASS (aversos: budgets SCSS pré-existentes, não bloqueantes) |
| mf-portfolio | `ng build` | PASS (sem erros) |
| mf-operations | `ng build` | PASS (averso: budget SCSS pré-existente, não bloqueante) |

## 2. Origem de dados (novos endpoints reais conectados)

### mf-pricing
- `GET /api/v1/prospect/studies` - lista de estudos; `POST /api/v1/prospect/studies` - criação; `POST /api/v1/prospect/studies/{id}/execute` - execução; `GET /api/v1/prospect/studies/{id}/results` - resultados PLD.
- SignalR `/hubs/prospect` (porta 8080 via `prospectHubUrl`).
- Observação: `GET /api/v1/pricing/forward-curve` NÃO existe no backend - ForwardCurveChart usa `GET /api/v1/prospect/studies/{id}/results` do estudo mais recente concluído (a curva forward é derivada dos resultados reais); se quiser um endpoint dedicado, ver seção 4.

### mf-hydrology
- `GET /api/v1/pluvia/ena` (submarket/offsetDays) - ENA, nível de reservatório e reservatórios.
- `GET /api/v1/pluvia/executions` - MLOps status (era pointed to 8000, corrigido para 8080).
- `GET /api/v1/pluvia/exports/{executionId}`, `POST /api/v1/pluvia/custom-maps/upload`, `POST /api/v1/pluvia/custom-maps/blend` - URLs centralizadas em `environment.apiUrl`.
- `GET {riskApiUrl}/pluvia/precipitation-map` - mapa de precipitação (mantido no FastAPI 8000, real).
- AuthService: `fallbackClaims` removido; sem token/roles → `[]` (acesso negado honesto).

### app-shell
- `getPriceForecasts` → `GET /api/v1/prospect/studies` + `GET .../{id}/results` (dados reais; vazio quando não há estudo concluído).
- `getRiskSummary` → `GET {riskApiUrl}/metrics/portfolio` (FastAPI real) → totalExposure/activeContractsCount derivados; var95/var99/stressTestLoss = 0 (sem endpoint - ver seção 4).
- `RiskService` URL centralizada via `environment.riskApiUrl`.
- Alerts: array fixo removido; SignalR `/hubs/alerts` (8080) com `ReceiveAlert` real.
- Users: `GET /api/v1/users`; edição de roles via dialog com `PUT /api/v1/users/{id}/roles`.
- Settings: `GET /api/v1/settings`, `PUT /api/v1/settings`, `POST /api/v1/settings/m2m-tokens`.

### mf-portfolio
- `GET /api/v1/portfolio/opportunities` - opportunities book.
- `GET /api/v1/portfolio/position` - dashboard (compras/vendas/líquido/mensal), asset-allocation e balanço de energia (dados reais por mês).
- `POST /api/v1/portfolio/simulate` - retorno real `SimulationResultDto` (mapeado corretamente; removido fallback fake).
- `POST /api/v1/portfolio/approve` - aprovação real (lista erro honesto do backend, ex: Imeris).
- Fallback `previousVolumeMwm:30.5`, `setTimeout`, e limite `volumeMwm > 20` removidos.
- Estratégias (kanban): dados fake removidos; colunas vazias com estado honesto.

### mf-operations
- `GET /api/v1/operations` (via OperationsService) para a Central de Aprovação (filtro `PendingApproval`) e `PATCH /api/v1/operations/{id}/state` com `{ newState: 'Approved' | 'Inactive' }` para aprovar/rejeitar.
- `GET /api/v1/portfolios` para PortfolioList.
- **Bug corrigido**: apiUrl duplicada (`environment.apiUrl/api/v1/operations` → `environment.apiUrl/operations`).

## 3. Infraestrutura
- `mf-pricing`, `mf-hydrology`, `mf-portfolio`: `src/environments/environment.ts` criado/padronizado (`apiUrl` 8080; hydrology e app-shell com `riskApiUrl` 8000) - B7.
- `app.config.ts`: `provideHttpClient()` adicionado em `mf-pricing` e `mf-portfolio` (faltava).
- Hub SignalR de Prospect/hydrology unificado na porta 8080; hubs de alerta e risco no 8080.

## 4. Pendências PARA BACKEND (Parte A / próximos sprints)
1. **`GET /api/v1/pricing/forward-curve`** - não existe; Frontend usa results de prospect/studies como proxy. Criar endpoint dedicado para curva forward.
2. **VaR/estresse** - não há endpoint de VaR (95/99) nem stress test; `getRiskSummary` retorna 0 nesses campos. Criar `GET /api/v1/risk/summary` (ou similar) no EtrmService/FastAPI.
3. **`POST /api/v1/strategies`** - não existe controller (Strategy está no Domain). Kanban de estratégias fica somente local até existir CRUD.
4. **`GET /api/v1/settings/m2m-tokens`** - não existe (só POST para gerar); lista de chaves inicia vazia.
5. **Revogação de chave M2M** - sem endpoint; remoção é local.
6. **Logs de sessão de usuário** - sem endpoint; botão mostra "não disponível".
7. **Solicitante/data de solicitação (Central de Aprovação)** - não expostos pela API de operations; coluna removida até backend prover `requestedBy/requestedAt`.
8. **Approve no fluxo de oportunidade** - `POST /api/v1/portfolio/approve` exige `operationId` real; no fluxo de oportunidades sem operação, retorna "Operação não encontrada." (resposta honesta do backend).

## 5. Arquivos alterados
### mf-pricing
- `src/environments/environment.ts` (criado)
- `src/app/features/prospect/services/prospect.service.ts`
- `src/app/features/prospect/prospect-detail/prospect-detail.ts` + `.html`
- `src/app/features/prospect/prospect-dashboard/prospect-dashboard.ts` + `.html`
- `src/app/features/pricing/components/risk-metrics/risk-metrics.ts` + `.html`
- `src/app/features/pricing/components/forward-curve-chart/forward-curve-chart.ts` + `.html` + `.css`
- `src/app/features/pricing/pricing-dashboard/pricing-dashboard.ts`
- `src/app/app.config.ts`

### mf-hydrology
- `src/environments/environment.ts` (criado)
- `src/app/core/services/auth.service.ts`
- `src/app/features/hydrology/components/ena-analytics/ena-analytics.ts`
- `src/app/features/hydrology/components/mlops-status/mlops-status.ts`
- `src/app/features/hydrology/components/precipitation-map/precipitation-map.component.ts`
- `src/app/features/hydrology/components/reservoir-levels-chart/reservoir-levels-chart.ts` + `.html` + `.css`
- `src/app/features/hydrology/components/exports-dashboard/exports-dashboard.ts`
- `src/app/features/hydrology/components/custom-scenarios/custom-scenarios.ts`

### app-shell
- `src/environments/environment.ts` + `environment.development.ts`
- `src/app/core/services/mlops.service.ts`
- `src/app/core/services/risk.service.ts`
- `src/app/features/alerts/alerts-dashboard.component.ts`
- `src/app/features/users/user-management.component.ts` + `edit-roles-dialog.component.ts`/`.html`/`.scss` (novos)
- `src/app/features/settings/settings-dashboard.component.ts`

### mf-portfolio
- `src/environments/environment.ts` (criado)
- `src/app/app.config.ts`
- `src/app/core/services/portfolio.service.ts`
- `src/app/features/strategies/strategies.component.ts` + `.html` + `.scss`
- `src/app/features/portfolio/components/asset-allocation/asset-allocation.ts` + `.html` + `.scss`
- `src/app/features/portfolio/components/energy-balance-chart/energy-balance-chart.ts` + `.html` + `.scss`
- `src/app/features/opportunities/components/simulation-dialog/simulation-dialog.component.ts`

### mf-operations
- `src/app/features/operations/services/operations.service.ts` (bug apiUrl duplicada corrigido)
- `src/app/features/operations/approval-center/approval-center.ts` + `.html` + `.scss`
- `src/app/features/portfolios/portfolio-list/portfolio-list.ts` + `.html` + `.scss`