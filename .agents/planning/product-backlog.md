# Product Backlog: EnergySuite Mocks Eradication & Integrations

Este documento rastreia todos os débitos técnicos gerados pelo uso de mocks na UI (Micro-frontends) e nas camadas de infraestrutura do backend. O objetivo de longo prazo é zerar esses débitos, conectando os MFEs a endpoints reais da API e finalizando implementações de serviços simulados.

## 1. Módulo Menza (Portfolio / Oportunidades)
| Status | ID | Descrição | Localização (MFE) |
|---|---|---|---|
| TODO | P-1 | Substituir `loadMockOpportunities()` no Book de Oportunidades por uma chamada real de API. | `opportunities-book.component.ts` |
| TODO | P-2 | Substituir `loadMockData()` no Dashboard do Portfolio pelos dados agregados reais do ETRM. | `dashboard.component.ts` |
| TODO | P-3 | Implementar backend do comando `SimulateOperationCommand` e conectá-lo ao dialog de simulação. | `simulation-dialog.component.ts` |

## 2. Módulo de Operações (BackOffice / CCEE)
| Status | ID | Descrição | Localização (MFE) |
|---|---|---|---|
| TODO | OP-1 | Remover mocks estáticos (`FinancialSettlementItem` e `OperationToBillItem`) do Dashboard Financeiro e consumir a API de liquidações. | `financial-dashboard.ts` |
| TODO | OP-2 | Trocar o mock data na listagem de Tickets. | `tickets-list.ts` |
| TODO | OP-3 | Trocar o mock de Readjustments nos detalhes de Contratos. | `contract-details.ts` |
| TODO | OP-4 | Trocar os dados estáticos de Contrapartes (`company-list`) pela API de Cadastro. | `company-list.ts` |
| TODO | OP-5 | Substituir o "Mock Contract" dos Quick Action Cards pela estrutura correta de criação do backend. | `quick-action-cards.ts` |

## 3. Módulo Pricing (Prospectos & Preços)
| Status | ID | Descrição | Localização (MFE) |
|---|---|---|---|
| TODO | PR-1 | Consumir os dados da Curva Forward via API, parando de gerar mock da Curva Futura no chart. | `forward-curve-chart.ts` |
| TODO | PR-2 | O `prospect.service.ts` inteiro está usando mocks; necessita integração total do fluxo de Prospectos e Dashboards no frontend. | `prospect.service.ts` |

## 4. Módulo Pluvia (Hydrology)
| Status | ID | Descrição | Localização (MFE) |
|---|---|---|---|
| TODO | HY-1 | Consumir dados reais da API Pluvia/Python para preencher o Grid (`generateMockPoints`). | `precipitation-map.component.ts` |
| TODO | HY-2 | Trocar o GUID mockado (`d290f1...`) no Dashboard de Exportações pela execução real. | `exports-dashboard.ts` |
| TODO | HY-3 | Trocar Keycloak JWT roles estáticas (`auth.service.ts`) por integração via angular-oauth2-oidc. | `auth.service.ts` |

## 5. Serviços de Backend e Base
| Status | ID | Descrição | Localização (Backend/Core) |
|---|---|---|---|
| TODO | BK-1 | Substituir "Mock implementation of a Webhook trigger" pela lógica real de dispatch HTTP (ex: HttpClient/Refit). | `WebhookService.cs` |
| TODO | BK-2 | Substituir a simulação no `ExternalTradeSyncService` (que hoje loga "Mock sync skipped") por integração real ETRM <-> CCEE/Externa. | `ExternalTradeSyncService.cs` |
| TODO | BK-3 | Integração de Gestão de Usuários no App-Shell. Onde está o Keycloak IAM real? | `app-shell/user-management.component.ts` |
| TODO | BK-4 | Trocar lógica de persistência "fake" das configurações. | `app-shell/settings-dashboard.component.ts` |

## 6. Débitos da Sprint 10 (Hotfix)
| Status | ID | Descrição | Localização |
|---|---|---|---|
| TODO | S10-1 | Criar teste unitário **dedicado** do `AuditLoggingBehavior` verificando a persistência do `AuditLog` no `IEtrmDbContext` (sucesso e falha). Implementação e registro no pipeline já concluídos (Sprint 10); falta a cobertura automatizada. | `backend/etrm-service/EtrmService.UnitTests/` |
| VERIFICAÇÃO | S10-2 | Confirmação em runtime: publicação manual de `OperationPublishedIntegrationEvent` no Kafka UI (`operation-events`) e HTTP 200 das chamadas do `mf-operations` contra a API ETRM em `8080`. Requer stack em execução. | `nativeInjectorConfig`, `mf-operations` |

## 7. P0 — Integração Kafka/DI Quebrada (varredura 2026-08-28)
> Criticidade: quebra fluxos em runtime e causa perda silenciosa de eventos. Prioridade máxima.

| Status | ID | Descrição | Localização |
|---|---|---|---|
| TODO | K-1 | Registrar consumer `EnaCalculatedEventConsumer` no endpoint MassTransit `ena-events` (hoje eventos de ENA do risk-service são descartados; `HydrologicalResult` nunca é persistido). | `EtrmService.API/IoC/NativeInjectorConfig.cs` |
| TODO | K-2 | Registrar consumer `OperationPublishedEventConsumer` no endpoint `operation-events` (webhook B2B pós-publicação nunca dispara). | `EtrmService.API/IoC/NativeInjectorConfig.cs` |
| TODO | K-3 | Alinhar tópico de risco: risk-service publica em `risk-events`/`ena-events`, mas o .NET escuta `risk-calculated`/nada → `RiskHub` e consumer de ENA nunca recebem. Renomear TopicEndpoint ou ajustar `risk-service/src/kafka_consumer.py`. | `NativeInjectorConfig.cs`, `risk-service/src/kafka_consumer.py` |
| TODO | K-4 | Adicionar `AddProducer<StudyExecutionRequestedEvent>("study-execution-requested")` — `POST /prospect/studies/{id}/execute` lança 500 (`InvalidOperationException` no `KafkaEventPublisher`). | `NativeInjectorConfig.cs:83-85`, `KafkaEventPublisher.cs:20` |
| TODO | K-5 | Registrar DI ausente: `IImerisCreditClient`, `IWebhookNotifierService`, `ITradingCopilotService`, `IOpportunityEngineService`. | `NativeInjectorConfig.cs` |

## 8. P1 — Job Falho + Higiene de Mocks (Backend)
| Status | ID | Descrição | Localização |
|---|---|---|---|
| TODO | BK-5 | `HydrologicalSimulationJob` envia `ScenarioId = Guid.Empty` (placeholder) → `RunHydrologicalSimulationCommandHandler` lança "Scenario not found" e o job de 04:00 falha toda manhã. Carregar o cenário diário padrão. | `EtrmService.API/Jobs/HydrologicalSimulationJob.cs:30` |
| TODO | BK-6 | Decidir módulo CCEE: os 4 handlers (`ProcessCliqCceeCsv`, `GenerateAdjustmentXml`, `GenerateCcealXml`, `GetCceeComparisons`) estão órfãos — sem controller/API. Expor endpoints HTTP ou remover. | `EtrmService.Application/CceeIntegration/` |
| TODO | BK-7 | Entidades sem DbSet/mapeamento Fluent: `Simulation`, `Opportunity`, `Strategy` (nunca persistidas); criar `ForecastMetadataMap.cs` e `CustomScenarioMap.cs` (convenção hoje). | `EtrmService.Domain/Entities/` |
| TODO | BK-8 | Substituir mocks de tela: `GetPortfolioPositionQuery` ("Sprint 2"), `GetStrategiesQuery`/`CreateStrategyCommand` (kanban), `OpportunityEngineService`, `SimulateOperationCommand`, `GetStudyResultsQueryHandler` (PLD aleatório), `GetEnaResultsQueryHandler` fallback 12 meses, `GetModelExecutionsQueryHandler`, `GenerateDecksCommandHandler` (`vazoes.dat` fake), fallback de exports/forecasts no `PluviaController`. | `EtrmService.Application/` + `EtrmService.API/Controllers/PluviaController.cs` |
| TODO | BK-9 | `ApproveOperationCommandHandler` não persiste nada e usa `mockCounterpartyId = Guid.NewGuid()`; `SimulateOperationCommand` fabrica estado "Before"; `CreateStrategyCommand` não persiste. Vincular a handlers com regra/persistência real (+ endpoints). | `EtrmService.Application/Operations/Commands/`, `Strategies/` |
| TODO | BK-10 | `ProspectController` usa tenant fixo `00000000-...-0001` hardcoded em vez de `ICurrentUserService.TenantId` (multi-tenancy violado). | `EtrmService.API/Controllers/ProspectController.cs:15,29,50,64,78` |
| TODO | BK-11 | Reconectar `UserManagementController` ao Keycloak (GetUsers mock / UpdateRoles só loga) e persistir `SettingsController` (tema/idioma/token `eyJ...` fake). | `EtrmService.API/Controllers/UserManagementController.cs`, `SettingsController.cs` |

## 9. P2 — Segurança e Integrações Externas (Backend)
| Status | ID | Descrição | Localização |
|---|---|---|---|
| TODO | BK-12 | Assinatura HMAC real no header `X-EnergySuite-Signature` (hoje chave crua — `// Mock implementation`) e mover webhook ENA da URL fixa `b2b-customer.internal` para config. | `OperationPublishedEventConsumer.cs:67`, `EnaCalculatedEventConsumer.cs:60-64` |
| TODO | BK-13 | `ExternalTradeSyncService` simula sync: faz GET real na CCEE mas não desserializa o corpo e envia valores fixos `15.5m/250.0m`; BBCE/N5X citados mas nunca chamados. | `EtrmService.Infrastructure/BackgroundServices/ExternalTradeSyncService.cs:24-65` |
| TODO | BK-14 | `WebhookNotifierService` só loga `[WEBHOOK DISPATCHED]` (nunca POSTa); `WebhookService` tem URL default `webhook.site` pública. Implementar dispatch HTTP (Polly/retry). | `Application/Services/WebhookNotifierService.cs`, `Infrastructure/Services/WebhookService.cs` |
| TODO | BK-15 | `risk-service`: remover mock 40x40 de precipitação, GEVAZP dummy (`gevazp_generator.py`), `asyncio.sleep(2)` e fallback anônimo de auth. `mlops`: treinar com dados reais do Data Lake em vez de séries aleatórias. | `risk-service/src/`, `mlops/dags/train_*.py` |

## 10. P1/P2 — Frontend: Mocks Restantes (após Sprints 8-10)
| Status | ID | Descrição | Localização (MFE) |
|---|---|---|---|
| TODO | F-1 | **mf-pricing**: `executeStudy()` simula fluxo inteiro com `setTimeout` (`// Fake HTTP request`); `loadResults()` retorna PLD hardcoded sem chamar `GET /prospect/studies/{id}/results`; prospect-dashboard usa backend `loadStudies()` em vez do mock. | `mf-pricing/src/app/features/prospect/prospect-detail/prospect-detail.ts:83-138`, `prospect-dashboard*/prospect-dashboard.ts:43-48` |
| TODO | F-2 | **mf-pricing**: `risk-metrics` defaults hardcoded (VaR/MtM/Vol) nunca sobrescritos; forward-curve usa fallback hardcoded; `onNewSimulation` só mostra snackbar (não persiste). | `mf-pricing/src/app/features/pricing/components/risk-metrics/risk-metrics.ts:20-42`, `forward-curve-chart.ts:19-24` |
| TODO | F-3 | **mf-hydrology**: ENA do `ena-analytics` hardcoded (chamada HTTP ausente); `auth.service.ts` com `fallbackClaims` estáticos e sem Keycloak; URLs hardcoded `localhost:8000` sem `environment.ts`/proxy consistente; `reservoir-levels-chart` gera histórico fake. | `mf-hydrology/src/app/features/hydrology/components/ena-analytics/ena-analytics.ts:69-91`, `core/services/auth.service.ts:8-37`, `mlops-status*`, `precipitation-map*` |
| TODO | F-4 | **app-shell**: `MlopsService` retorna `of(...)` (forecast sinusoidal fake + exposure hardcoded) → `executive-dashboard` herda dados inventados; alerts estáticos; `user-management` com array mock (Keycloak real ausente); `settings-dashboard` `saveSettings()` não persiste; `generateApiKey` cria token fake local. | `app-shell/src/app/core/services/mlops.service.ts:31-70`, `features/alerts/*`, `features/users/user-management.component.ts:30-39`, `features/settings/settings-dashboard.component.ts:43-60` |
| TODO | F-5 | **mf-portfolio**: telas Estratégias (kanban) e Portfolio (asset-allocation + energy-balance) 100% hardcoded; `simulation-dialog` fallback mock e limite de crédito Imeris hardcoded (`volume > 20`) sem consultar ACL; `approve()` usa `setTimeout` local. | `mf-portfolio/src/app/features/strategies/strategies.component.ts:23-50`, `portfolio/components/*`, `opportunities/components/simulation-dialog/*:42-83` |
| TODO | F-6 | **mf-operations**: `approval-center` com 1 item hardcoded e `approve()/reject()` só `console.log`; `portfolio-list` tabela 100% hardcoded sem service. | `mf-operations/src/app/features/operations/approval-center/approval-center.ts:32-34`, `features/portfolios/portfolio-list/portfolio-list.ts:29-33` |
| TODO | F-7 | Endpoints frontend `/api/v1/...` sem proxy configurado no mf-hydrology/mf-pricing (caminhos relativos vs `localhost:8000/8080`). Validar roteamento/proxy no `app-shell` e definir `environment` único. | `frontend/mf-hydrology`, `frontend/mf-pricing` |

## 11. Dados de Uso Não Utilizados (Backend — baixa prioridade)
| Status | ID | Descrição | Localização |
|---|---|---|---|
| TODO | BK-16 | DbSets declarados sem uso: `Persons`, `EconomicGroups`, `PriceIndexValues`, `DocumentAttachments`, `ContractAmendments` (via DbSet). Verificar se pertencem a escopo futuro (cadastro) ou remover. | `EtrmService.Infrastructure/Persistence/EtrmDbContext.cs:18-31` |
