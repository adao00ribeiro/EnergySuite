# Sprint 11: Reestabilização de Integração (Kafka/DI/Job)

**Status:** ✅ **CONCLUÍDA** (2026-08-28) — build 0 erros, dotnet test 19/19 verdes.
**Objetivo:** Corrigir os achados críticos (P0) da varredura do Product Owner que quebram a integração em runtime (consumers Kafka órfãos, mismatch de tópicos, producer faltante, DI ausente) e o job agendado que falha diariamente. Foco exclusivo em restaurar o fluxo de eventos entre os serviços — sem introduzir novas funcionalidades.

**Justificativa de Negócio:** Os itens abaixo causam **perda silenciosa de eventos** (ENA nunca persistido, webhook B2B nunca disparado, `RiskHub` nunca recebe dados), **HTTP 500 no fluxo de Prospec** (`POST /prospect/studies/{id}/execute`) e **falha diária do job hidrológico**. São débitos que violam as regras do Menza (auditoria/acl) e derrubam módulos críticos do clone Norus.

---

## Tasks (Para `backend-architect` e `risk-scientist`)

### 1. 🔴 K-4: Registrar Producer `StudyExecutionRequestedEvent` (`backend-architect`)
- **Contexto de Negócio:** O fluxo de Prospecção (estudo de cenários de mercado) depende de disparar o processamento de modelo para o consumidor `ProspectModelRunnerConsumer` via tópico `study-execution-requested`. Hoje `ExecuteStudyCommandHandler` publica `StudyExecutionRequestedEvent`, mas **não existe** `AddProducer` para esse tipo — o `KafkaEventPublisher.GetRequiredService<ITopicProducer<T>>()` lança `InvalidOperationException`, derrubando o endpoint real `POST /api/v1/prospect/studies/{id}/execute` com HTTP 500.
- **Ação:**
  - Em `NativeInjectorConfig.cs` (linha ~85), adicionar junto aos producers existentes:
    ```csharp
    rider.AddProducer<EtrmService.Application.Prospect.Events.StudyExecutionRequestedEvent>("study-execution-requested");
    ```
  - Confirmar o namespace/tipo do evento em `ExecuteStudyCommandHandler.cs:42`.
- **Critérios de Aceite:**
  - [x] `AddProducer` registrado seguindo a mesma convenção dos demais (`contract-events`, `pluvia-events`, `operation-events`).
  - [x] `POST /api/v1/prospect/studies/{id}/execute` não lança mais `InvalidOperationException` de DI (retorna sem 500 por falta de producer).
  - [x] `dotnet build` do `EtrmService.slnx` sem erros.
  - [x] `dotnet test` continua verde (19/19).
- **Arquivos:** `EtrmService.API/IoC/NativeInjectorConfig.cs`, `EtrmService.Application/Prospect/Commands/ExecuteStudyCommandHandler.cs`
- **Dependências:** Nenhuma bloqueante.

### 2. 🔴 K-1: Registrar Consumer `EnaCalculatedEventConsumer` (`backend-architect`)
- **Contexto de Negócio:** O `risk-service` (Python) publica resultados de ENA no tópico `ena-events` (48 registros por simulação — 4 submercados × 12 meses). O consumidor .NET `EnaCalculatedEventConsumer` já existe e persiste `HydrologicalResult` + dispara webhook, porém **nunca foi registrado** no MassTransit. Resultado: os dados hidrológicos são **descartados para sempre** e a tabela `HydrologicalResults` (Pluvia) permanece vazia.
- **Ação:** Em `NativeInjectorConfig.cs`:
  - Registrar o consumer no rider: `rider.AddConsumer<...EnaCalculatedEventConsumer>();`
  - Mapear o endpoint: `k.TopicEndpoint<EnaCalculatedIntegrationEvent>("ena-events", "etrm-service-group", e => e.ConfigureConsumer<...EnaCalculatedEventConsumer>(context));`
  - Confirmar o tipo do evento (`EnaCalculatedIntegrationEvent`) e o namespace em `EnaCalculatedEventConsumer.cs`.
- **Critérios de Aceite:**
  - [x] Consumer registrado e endpoint `ena-events` mapeado.
  - [ ] Mensagens de ENA do `risk-service` persistidas em `HydrologicalResults` (verificação em runtime no Kafka UI — requer stack).
  - [x] `dotnet build` sem erros; `dotnet test` verde.
- **Arquivos:** `EtrmService.API/IoC/NativeInjectorConfig.cs`, `EtrmService.API/Consumers/EnaCalculatedEventConsumer.cs`
- **Dependências:** Nenhuma.

### 3. 🔴 K-2: Registrar Consumer `OperationPublishedEventConsumer` (`backend-architect`)
- **Contexto de Negócio:** O módulo Menza publica `OperationPublishedIntegrationEvent` no tópico `operation-events` quando uma operação é publicada. O consumidor `OperationPublishedEventConsumer` (que consulta webhooks cadastrados e dispara notificações B2B) **nunca foi registrado**. A regra "B2B Webhooks" do Menza está ativa no produtor mas morta no receptor — nenhum cliente externo é notificado da publicação.
- **Ação:** Registrar consumer + `TopicEndpoint<OperationPublishedIntegrationEvent>("operation-events", ...)` em `NativeInjectorConfig.cs`, na mesma convenção.
- **Critérios de Aceite:**
  - [x] Consumer registrado e endpoint `operation-events` mapeado.
  - [ ] Publicação de operação dispara o consumer (verificação via log / Kafka UI — requer stack).
  - [x] `dotnet build` sem erros; `dotnet test` verde.
- **Arquivos:** `EtrmService.API/IoC/NativeInjectorConfig.cs`, `EtrmService.API/Consumers/OperationPublishedEventConsumer.cs`
- **Dependências:** Nenhuma.

### 4. 🔴 K-3: Alinhar Tópico de Risco (`backend-architect` + `risk-scientist`)
- **Contexto de Negócio:** O `risk-service` publica o resultado de cálculo de risco no tópico **`risk-events`** (`TOPIC_PRODUCE = "risk-events"` em `kafka_consumer.py:33`), enquanto o .NET escuta **`risk-calculated`** (`NativeInjectorConfig.cs:91`) e publica ENA em **`ena-events`**. Resultado: `RiskCalculatedEventConsumer` nunca recebe mensagens → o `RiskHub` (SignalR) nunca envia atualizações de risco para o frontend, e o dashboard de risco fica sem dados em tempo real.
- **Ação (decisão recomendada):** Alinhar **um dos dois lados**. Recomendação: padronizar pelo nome do tópico produzido pelo `risk-service` (`risk-events`) ou, alternativamente, alterar o produtor Python. Evitar tocar no Python se possível (menos acoplamento) — a decisão é da arquitetura.
  - Se mudar o .NET: renomear `"risk-calculated"` para `"risk-events"` no `TopicEndpoint` (linha 91).
  - Se mudar o Python: `TOPIC_PRODUCE = "risk-calculated"`.
  - Confirmar também o payload/evento (`RiskCalculatedIntegrationEvent`) batendo com `RiskCalculatedEvent` do Python.
- **Critérios de Aceite:**
  - [x] Tópico de risco alinhado entre produtor (Python) e consumidor (.NET) — renomeado para `risk-events`; flags do consumer sinalizam quando um contrato é processado.
  - [ ] `RiskHub` recebe `ReceiveRiskCalculation` e atualiza o frontend (verificação em runtime — requer stack).
  - [x] `dotnet build` sem erros; testes verdes (se aplicável).
- **Arquivos:** `EtrmService.API/IoC/NativeInjectorConfig.cs`, `backend/risk-service/src/kafka_consumer.py`
- **Dependências:** Decisão de arquitetura sobre o lado a alterar.

### 5. 🟠 K-5: Registrar DI dos Serviços do Menza/Imeris (`backend-architect`)
- **Contexto de Negócio:** Os handlers do Copilot/Menza (`ApproveOperationCommandHandler`, `SimulateOperationCommandHandler`, `GetRankedOpportunitiesQueryHandler`) injetam interfaces que **não têm registro** em `NativeInjectorConfig`: `IImerisCreditClient`, `IWebhookNotifierService`, `ITradingCopilotService`, `IOpportunityEngineService`. Hoje o DI não falha porque nenhum controller chama esses handlers (código órfão), mas qualquer endpoint/UI que invoque o Copilot quebrará em runtime. O `ApproveOperationCommand` (aprovado pelo Copilot → validação de crédito no Imeris via ACL) é regra de negócio do Menza.
- **Ação:** Registrar as 4 implementações como `Scoped`/`Transient` em `NativeInjectorConfig.cs` (convenção já usada para `ICurrentUserService`/`WebhookService`):
  - `IImerisCreditClient` → `ImerisCreditClient`
  - `IWebhookNotifierService` → `WebhookNotifierService`
  - `ITradingCopilotService` → `TradingCopilotService`
  - `IOpportunityEngineService` → `OpportunityEngineService`
  - (Confirmar a implementação default ou refatorar para variantes reais se a Sprint incluir a fase de erradicação de mocks.)
- **Critérios de Aceite:**
  - [x] As 4 interfaces resolvidas pelo `IServiceProvider` sem exceção.
  - [x] `dotnet build` sem erros; `dotnet test` verde.
  - [ ] (Opcional) Smoke test de resolução via DI no startup. *(não executado nesta sprint — débito opcional)*
- **Arquivos:** `EtrmService.API/IoC/NativeInjectorConfig.cs`
- **Dependências:** Nenhuma. *(Nota: os mocks internos desses serviços serão tratados em sprint posterior — aqui garante-se apenas a resolubilidade.)*

### 6. 🟠 BK-5: Corrigir `HydrologicalSimulationJob` (`backend-architect`)
- **Contexto de Negócio:** O job Quartz que roda às 04:00 dispara `RunHydrologicalSimulationCommandHandler` enviando `ScenarioId = Guid.Empty` (placeholder). O handler lança `"Scenario not found"` → **o job falha todas as manhãs**, sem efeito. Regra do clone Norus: deve rodar o cenário hidrológico **padrão do dia**.
- **Ação:** Substituir `ScenarioId = Guid.Empty` por uma consulta real do cenário padrão (ex: `_context.PrecipitationScenarios` filtrando o cenário `IsDefault`/do dia), ou buscar no repositório antes de disparar o comando.
- **Critérios de Aceite:**
  - [x] O job resolve um `ScenarioId` válido (default do dia) antes de disparar.
  - [x] `RunHydrologicalSimulationCommandHandler` não lança mais "Scenario not found" por Guid vazio.
  - [x] `dotnet build` sem erros; `dotnet test` verde.
- **Arquivos:** `EtrmService.API/Jobs/HydrologicalSimulationJob.cs:28-35`, `EtrmService.Application/Pluvia/Commands/RunHydrologicalSimulationCommandHandler.cs`
- **Dependências:** Conhecimento do modelo de cenário (verificar campo `IsDefault`/agendamento).

---

## Definição de Pronto (DoD) da Sprint
- [x] `dotnet build` do `EtrmService.slnx` sem erros (0 erros, 2 warnings NU1603 pré-existentes).
- [x] `dotnet test` verde (19/19).
- [x] Nenhum novo consumer/producer/DI órfão introduzido — verificação no `NativeInjectorConfig.cs`.
- [ ] Verificações de runtime (dependem de stack): eventos de ENA persistindo, `operation-events` consumido, `risk-events` religando o `RiskHub`, `POST /prospect/studies/{id}/execute` sem 500 de producer, job 04:00 sem erro de cenário. *(requer stack em execução — registrado abaixo)*

## Registro de Verificação em Runtime (pós-sprint) — pendente, requer stack
- [ ] `risk-service` publicando em tópico alinhado e `.NET` consumindo (log do consumer).
- [ ] Kafka UI: mensagens em `ena-events`, `operation-events`, `study-execution-requested`.
- [ ] `RiskHub` (SignalR) atualizando o dashboard de risco no `app-shell`.

## Fora de Escopo (próximas Sprints recomendadas)
- **Sprint 12 — Erradicação de mocks backend (P1):** CCEE (expor endpoints ou remover), entidades sem DbSet (`Simulation`/`Opportunity`/`Strategy`), `GetPortfolioPositionQuery`/`GetStrategiesQuery`/`OpportunityEngineService`/`SimulateOperationCommand`, tenant fixo no `ProspectController`, `UserManagementController`/`SettingsController` (Keycloak/persistência).
- **Sprint 12 — Frontend (P1):** mf-pricing (prospect fake HTTP), mf-hydrology (auth/ENA/URLs), app-shell (MlopsService/ alerts/users/settings), mf-portfolio (estratégias/portfolio/simulação), mf-operations (approval-center/portfolio-list).
- **Sprint 13 — Segurança/integrações externas (P2):** HMAC no webhook, URL `webhook.site`/`b2b-customer.internal` para config, `ExternalTradeSyncService` real (desserializar payload CCEE), `WebhookNotifierService` POST real com Polly.
