# Sprint 12: Erradicação de Mocks (Backend + Frontend)

**Status:** ✅ **CONCLUÍDA** (2026-08-28) — build 0 erros, dotnet test 19/19, `ng build` dos 5 MFEs sem erros.
**Objetivo:** Zerar os mocks P1 identificados na varredura do Product Owner — dados fabricados em queries/handlers do backend e telas dos Micro-frontends — substituindo por integração real com a API e persistência em banco. Nenhuma funcionalidade nova; apenas a realinha.

**Justificativa de Negócio:** Os módulos Menza (Portfolio/Oportunidades), Pluvia (Hidrologia), Pricing (Prospectos) e BackOffice exibem dados que **não existem em banco** (PLD aleatório, gaps sintéticos, kanban de estratégias fake, ENA mockado). Isso engana os traders/analistas com números fictícios de P&L e risco, violando a confiabilidade do clone Norus e a auditoria exigida pelo Menza (`AuditLoggingBehavior` grava comandos que, hoje, calculam dados inventados).

---

## PARTE A — BACKEND (`backend-architect`)

### A1. 🟠 BK-10: Multi-tenancy real no `ProspectController`
- **Contexto:** Todos os endpoints `api/v1/prospect/*` usam tenant fixo `00000000-0000-0000-0000-000000000001` (`// Mock Tenant`) em vez de `ICurrentUserService.TenantId` (`ProspectController.cs:15,29,50,64,78`). Com multi-tenancy Global Query Filters, um usuário de outro tenant lê/escreve dados errados.
- **Ação:** Injetar `ICurrentUserService` no controller e substituir `Guid.Parse("00000000-...-0001")` por `_currentUser.TenantId` em `CreateStudy`, `GetStudies`, `ExecuteStudy`, `GetStudyResults`, `CloneStudy`. Remover o comentário `// Simulando a blindagem B2B via JWT`.
- **Critérios de Aceite:** nenhum tenant hardcoded restante no controller; build/test verdes.

### A2. 🟠 BK-8(1): `GetPortfolioPositionQuery` real
- **Contexto:** `GetPortfolioPositionQueryHandler.cs` fabrica portfolio "Mock Sprint 2", heatmap/gaps/PLD **aleatórios** (`Random`) e `Task.Delay(100)` simulando banco. O componente `position-grid`/`heatmap-chart` do mf-portfolio herda esses números inventados.
- **Ação:** Reescrever o handler agregando dados reais **da tabela `Operations`** (já persistidas via `CreateOperationCommandHandler`): somar `TotalPurchasedMwMed`/`TotalSoldMwMed` por `Submarket`/`EnergySource`/mês para o `Year` requisitado, calcular `NetPositionMwMed`, e popular `Heatmap`/`DetailedGaps`/`MonthlyPositions` a partir dessas operações. `PortfolioName` vem de `Portfolios` (via `GetPortfoliosQuery`). Remover `Task.Delay`/`Random`.
- **Critérios de Aceite:** dto construído via EF (`_context.Operations` com Includes), sem `Random`/`Task.Delay`, build/test verdes.

### A3. 🟠 BK-8(2) + BK-7: Estratégias persistidas no banco
- **Contexto:** `GetStrategiesQuery` mocka 4 estratégias para o Kanban (`// Mocking Data for Kanban`) e `CreateStrategyCommandHandler` **não grava nada** (`// Simula gravação no DB`, `Task.Delay(100)`). A entidade `Strategy` (Domain) **não tem DbSet nem mapeamento Fluent** (BK-7).
- **Ação (`backend-architect`):**
  - Adicionar `DbSet<Strategy>` em `EtrmDbContext` + `StrategyMap.cs` (Fluent API, sem Data Annotations) + migration.
  - Reescrever `CreateStrategyCommandHandler` persistindo a entidade (repositório ou `IEtrmDbContext`) e `GetStrategiesQueryHandler` lendo do banco (grupo por status Draft/Approved/Inactive, campos do modelo real).
  - Decidir os campos reais da `Strategy` já existente — usar o modelo atual da entidade; se estiver desalinhado com a UI do kanban, ajustar dentro do escopo.
- **Critérios de Aceite:** tabela `strategies` criada e usada; query/command sem mocks; build/test verdes.

### A4. 🟠 BK-8(3): `OpportunityEngineService` real
- **Contexto:** `OpportunityEngineService.cs` retorna 3 oportunidades hardcoded com `Task.Delay(50)` (`// Simulate processing`). `GetRankedOpportunitiesQueryHandler` é órfão (sem endpoint) — o mf-portfolio usa `portfolio.service` com `GET /api/v1/portfolio/opportunities`... **verificar**: se não houver controller/endpoint para esse query, criar `PortfolioController` expondo `GET /api/v1/portfolio/opportunities` chamando o query (alinhado ao service do frontend).
- **Ação:** Implementar o motor real: ler operações/contratos do portfolio, computar gaps de posição (déficit/excedente por submercado/mês) usando a lógica agregada (semelhante A2), cruzar com estratégias ativas e produzir `OpportunityDto` pontuados. Persistir resultados em `Opportunity` (entidade, ver BK-7) ou retornar DTO calculado. Remover comentários `// Simulate`.
- **Critérios de Aceite:** motor sem dados hardcoded; endpoint `GET /api/v1/portfolio/opportunities` acessível (se ausente); build/test verdes.

### A5. 🟠 BK-9: `SimulateOperationCommand` + `ApproveOperationCommand` reais
- **Contexto:**
  - `SimulateOperationCommand.cs:39-47` mocka o estado "Before" (`30.5m`, `450000`, `-12000`).
  - `ApproveOperationCommand.cs:19,36-39,52` usa `mockCounterpartyId = Guid.NewGuid()`, não persiste e não chama a ACL do Imeris (agora resolvível, ver Sprint 11 K-5).
- **Ação (`backend-architect`):** `SimulateOperationCommandHandler` calcular "Before" consultando posição real (via lógica de A2/`GetPortfolioPositionQuery`) e "After" aplicando a operação simulada (volume/preço/ticket informados); retornar `financialDelta`. `ApproveOperationCommandHandler`: persistir a operação como `Approved` (ChangeOperationState), usar `IImerisCreditClient.ValidateAsync` para a validação de crédito e `IWebhookNotifierService.Notify` no fluxo; remover GUIDs mock.
- **Critérios de Aceite:** nenhum `Guid.NewGuid()` fake/valor hardcoded; handlers persistem/validam de fato; build/test verdes.

### A6. 🟠 BK-8(4): Resultados de Estudo (PLD) reais
- **Contexto:** `GetStudyResultsQueryHandler.cs:35-43` gera PLD fictício (`// Gerando PLD fictício`) com `Random`. O fluxo completo (Sprint 11 K-4) já publica `StudyExecutionRequestedEvent`; o `ProspectModelRunnerConsumer` deve persistir resultados de verdade.
- **Ação (`backend-architect`):**
  - Persistir resultados no `ProspectModelRunnerConsumer` após "executar" (gravar PLD em entidade/extensão de `ProspectStudy` — ver modelo existente) em vez de apenas logar.
  - `GetStudyResultsQueryHandler` passa a **ler do banco**; se não houver resultados, retorna lista vazia (status "em execução") em vez de fabricar números.
  - (Refinar `ProspectModelRunnerConsumer` para salvar PLD por submercado/mês persistidos, removendo `Task.Delay`/inviabilidade fabricada se o escopo permitir — registrar no backlog o que não couber.)
- **Critérios de Aceite:** handler sem `Random`; resultados lidos do banco; build/test verdes.

### A7. 🟠 BK-8(5): Fallbacks de Pluvia e ModelExecutions
- **Contexto:** `PluviaController.cs:78-87` fabrica forecasts GEFS/ECMWF/ETA e `:120-132` mocka exports MinIO; `GetModelExecutionsQueryHandler.cs` retorna accuracy fixa `"MSE: 0.042"` + 3 execuções fictícias; `GetEnaResultsQueryHandler.cs:47-61` gera 12 meses de ENA aleatórios quando DB vazio.
- **Ação (`backend-architect`):** substituir fallbacks mockados por: consulta real às tabelas (ex: `HydrologicalResults`, execuções reais de modelo), e no caso de DB vazio retornar lista vazia/204 em vez de dados inventados. Accuracy calculada de metric reais se existirem; caso contrário remover o campo fictício.
- **Critérios de Aceite:** nenhum array de forecast/exportação fabricado; build/test verdes.

### A8. 🟠 BK-11: `UserManagementController` (Keycloak) e `SettingsController` (persistência)
- **Contexto:** `UserManagementController.cs` mocka 1 usuário e `UpdateRoles` só loga; `SettingsController.cs` retorna settings mock e `GenerateM2MToken` devolve token falso `"eyJ..." + Guid` (BK-4 do backlog).
- **Ação (`backend-architect`):**
  - Integrar `UserManagementController` ao Keycloak admin API (Realm Management REST) para listar usuários e atualizar roles (usar `HttpClientFactory` + config do `appsettings` para a URL do Keycloak admin). Se a integração exigir credenciais de serviço, usar service account no `appsettings` (nunca hardcoded).
  - `SettingsController`: criar casca EF (entidade `AppSetting`/tenant) mínima para persistir theme/language/timezone e gerar M2M tokens apenas via API real (JWT assinado com a key configurada — regra de segurança, não token fake). Se a entidade exigir migration, adicionar.
  - Registrar o que não couber no backlog (ex: UI completa).
- **Critérios de Aceite:** sem usuário/token fabricado; endpoints falham/retornam erro honesto se Keycloak indisponível (não sucesso falso); build/test verdes.

---

## PARTE B — FRONTEND (`frontend-master`)

### B1. 🔴 F-1: `mf-pricing` — Prospect Detail e Dashboard reais
- **Contexto:** `prospect-detail.ts:82-126` simula `executeStudy()` inteiro com `setTimeout` encadeados e `// Fake HTTP request`; `loadResults():131` retorna PLD hardcoded (`// Simulando retorno da API`); `prospect-dashboard.ts:43-48` usa `studies[]` mock mesmo tendo `ProspectService.loadStudies()` real.
- **Ação (`frontend-master`):**
  - `executeStudy()` → `ProspectService.executeStudy(studyId)` (POST `studies/{id}/execute`), atualizando logs/status via **SignalR** real (`ProspectHub`, já conectado) em vez de `setTimeout`.
  - `loadResults()` → chamar `GET studies/{id}/results` e alimentar o chart; estado vazio = "aguardando processamento".
  - Dashboard → substituir array mock por `ProspectService.loadStudies()`; remover fallback fake em `onNewStudy()` (erro honesto).
  - Decks/premissas → consumir da API do estudo (DTO existente) se disponível; senão estado vazio.
- **Critérios de Aceite:** nenhum `setTimeout` de simulação; charts/tabelas vindos da API; `ng build` do MFE sem erros.

### B2. 🟠 F-2: `mf-pricing` — Risk Metrics, Forward Curve, Nova Simulação
- **Contexto:** `risk-metrics.ts:20-42` tem defaults hardcoded (VaR/MtM/Vol) não sobrescritos; `forward-curve-chart.ts:19-24` usa fallback hardcoded; `pricing-dashboard.ts:38` `onNewSimulation()` só snackbar (não persiste).
- **Ação (`frontend-master`):** alimentar `risk-metrics` via API real (metadados de preço/risco se existirem, senão remover default); forward-curve sem fallback inventado (estado de erro/empty legível); `onNewSimulation` POST para a API e refresh da lista. Coordenou com backend para garantir endpoints que o frontend chama existam (`/api/v1/pricing/...`).
- **Critérios de Aceite:** nenhum default numérico inventado visível; `ng build` sem erros.

### B3. 🟠 F-3: `mf-hydrology` — ENA, Auth e URLs
- **Contexto:** `ena-analytics.ts:69-91` ENA hardcoded (sem HTTP); `auth.service.ts:8` `fallbackClaims` estáticos e `isAuthenticated()` só checa sessionStorage (sem Keycloak); `mlops-status.ts:39` e `precipitation-map*.ts:84` apontam `http://localhost:8000` hardcoded sem `environment`.
- **Ação (`frontend-master`):** ENA via API real (`GET /api/v1/pluvia/ena`); criar `environment.ts` no mf-hydrology com `apiUrl` e proxy no `app-shell` para `/api/v1/pluvia/*` (consistência como mf-operations); auth → emitir token/claims do app-shell (Keycloak real) via prop/rota e remover `fallbackClaims` (sem roles = não autenticado).
- **Critérios de Aceite:** sem `localhost:8000` hardcoded; sem `fallbackClaims`; `ng build` sem erros.

### B4. 🟠 F-4: `app-shell` — Mlops, Alerts, Usuários, Settings
- **Contexto:** `mlops.service.ts:31-70` retorna `of(...)` (forecast sinusoidal + exposure hardcoded); `alerts-dashboard:31-35` array fixo; `user-management:30-39` array mock + `alert()` modal; `settings-dashboard:43-60` `saveSettings()` não persiste e `generateApiKey()` cria token fake local.
- **Ação (`frontend-master`):**
  - `MlopsService.getPriceForecasts/getRiskSummary` → HTTP real (`/api/v1/pricing/forward-curve` e `/api/v1/risk/...` conforme os endpoints que o backend expõe; risco vem do `risk-service` via API do app-shell). Sem `of(...)`.
  - `alerts-dashboard` → API de alerts (ou SignalR `/api/hubs/alerts` real já existente); sem array fixo.
  - `user-management` → `GET /api/v1/users` (controller real do backend A8); `editRoles` abre modal funcional persistindo via API.
  - `settings-dashboard` → `GET/POST /api/v1/settings` (backend A8) e `generateApiKey` chama backend real (remove token fake local).
- **Critérios de Aceite:** nenhum `of(...)` de dados inventado nesses services; `ng build` do app-shell sem erros.

### B5. 🟠 F-5: `mf-portfolio` — Estratégias, Portfolio e Simulação
- **Contexto:** `strategies.component.ts:23-34` kanban hardcoded; `asset-allocation.ts:12-16` e `energy-balance-chart.ts:17-19` arrays estáticos; `simulation-dialog:42-83` fallback mock e `approve()` com limite local `volume > 20`.
- **Ação (`frontend-master`):** Estratégias → chamar `GET/POST /api/v1/strategies` (backend A3); `asset-allocation`/`energy-balance` → dados da posição real (`GET /api/v1/portfolio/position` → `GetPortfolioPositionQuery`); `simulation-dialog` → conectar ao `POST /api/v1/portfolio/simulate` real (remove `setTimeout` e limite local); `approve()` → API real (que internamente valida via Imeris ACL — A5).
- **Critérios de Aceite:** sem arrays hardcoded nos componentes citados; `ng build` sem erros.

### B6. 🟠 F-6: `mf-operations` — Approval Center e Portfolio List
- **Contexto:** `approval-center.ts:32-34` 1 item hardcoded, `approve()/reject()` só `console.log`; `portfolio-list.ts:29-33` tabela 100% hardcoded sem service.
- **Ação (`frontend-master`):** Approval Center → consumir operações pendentes da `OperationsService` (ex: filtrar `ChangeOperationStateCommand`/estado) e implementar `approve/reject` chamando `PATCH /api/v1/operations/{id}/state`; Portfolio List → usar `PortfolioService` (existe `GET /api/v1/portfolios`) substituindo o array.
- **Critérios de Aceite:** nenhum item hardcoded; ações chamam API; `ng build` sem erros.

### B7. 🟠 F-7: Consistência de proxy/environment
- **Contexto:** `mf-hydrology`/`mf-pricing` usam caminhos relativos ou portas hardcoded sem proxy único (F-7).
- **Ação (`frontend-master`):** padronizar todos os MFEs no mesmo padrão do `app-shell`/`mf-operations` (`environment.apiUrl = http://localhost:8080/api/v1`) e garantir proxy/roteamento no shell para os caminhos `/api/v1/*` e hubs SignalR.
- **Critérios de Aceite:** `apiUrl` consistente nos 5 MFEs; `ng build` dos 5 sem erros.

---

## Definição de Pronto (DoD)
- [x] `dotnet build` do `EtrmService.slnx` sem erros; `dotnet test` (19/19) verde.
- [x] `ng build` dos MFEs alterados sem erros (mf-pricing, mf-hydrology, app-shell, mf-portfolio, mf-operations).
- [x] **Zero** comentários `// Mock`, `// Simula`, `// Fake`, `Task.Delay` de simulação e `Random` de dados de negócio nos arquivos alterados (grep por `Mock|Simulat|Fake|placeholder` nos diffs).
- [x] Nenhuma exceção na resolução DI (Sprint 11 garantiu registros).
- [x] Migrations EF geradas e aplicáveis (`20260829010537_AddStrategiesAppSettingsAndStudyResults`).

## Pendências registradas (para Sprint 13)
- CCEE (`ProcessCliqCceeCsv` parsing mock, `GenerateAdjustmentXml`, endpoints) — Sprint 13.
- `ImerisCreditClient` (Task.Delay/heurística 20 MWm) — ACL real Sprint 13.
- `TradingCopilotService` (Task.Delay "Simulate AI") — Sprint 13 mlops.
- `OperationPublishedEventConsumer` HMAC mock — Sprint 13 segurança.
- `ExternalTradeSyncService` (sync CCEE/BBCE) — Sprint 13.
- `GenerateDecksCommandHandler` (`vazoes.dat` mock) — Sprint 13.
- `Operation` sem `Submarket`/`EnergySource` → heatmap/detalhe de submercado vazio (A2/A4).
- `Opportunity` não persistida (A4 retorna DTO calculado).
- `GET /api/v1/portfolio/dashboard` não implementado (dependência do mf-portfolio).
- PLD real (GEVAZP/NEWAVE) não existe → consumer persiste `[]` (honesto).
- `GET /api/v1/pricing/forward-curve`, endpoints de VaR/summary, session logs, `requestedBy/requestedAt` — registrados pelo frontend como lacunas de API.

## Fora de Escopo (Sprint 13 ou posterior)
- Segurança de webhooks (HMAC, URLs configuráveis) — Sprint 13.
- `ExternalTradeSyncService` real (CCEE/BBCE) — Sprint 13.
- `risk-service`/`mlops` (mocks Python) — Sprint 13.
- DbSets órfãos de cadastro (`Persons`, `EconomicGroups`, etc.) — avaliar em refinamento.