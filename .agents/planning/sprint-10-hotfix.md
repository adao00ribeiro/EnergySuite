# Sprint 10: Hotfix de Estabilidade, Paridade e Segurança

**Status:** ✅ **CONCLUÍDA** (2026-08-28)

**Objetivo:** Corrigir os bugs críticos de arquitetura/integração identificados na auditoria do Product Owner que ameaçam a estabilidade em runtime, exponham risco de segurança em produção e quebram a paridade do fluxo Menza/BackOffice.

**Justificativa de Negócio:** Os itens abaixo violam as regras da Clean Architecture e das regras de negócio (Sprint 8/9), pois derrubam a publicação de operações, mantêm código quebrado no repositório e comprometem a auditoria de compliance exigida pelo módulo Menza.

## Tasks (Para `backend-architect` e `frontend-master`)

### 1. 🔴 Bug Fix: Registro do Producer Kafka em Falta (`backend-architect`)
- **Contexto de Negócio:** O pipeline CQRS (MediatR + MassTransit) publica eventos de integração para outros serviços (risk-service, pluvia). `KafkaEventPublisher` resolve `ITopicProducer<T>` **por tipo de evento** via `IServiceProvider`. No entanto, apenas 2 producers foram registrados em `NativeInjectorConfig`, fazendo com que a publicação de qualquer outro evento falhe em runtime com `No service for ITopicProducer<...>`.
- **Escopo do Bug:** `PublishOperationCommand` publica `OperationPublishedIntegrationEvent` (evento de operação publicada → consumido por `OperationPublishedEventConsumer` → dispara webhook/auditoria do BackOffice).
- **Critérios de Aceite:**
  - [x] O handler `PublishOperationCommandHandler` publica `OperationPublishedIntegrationEvent` sem lançar exceção de DI.
  - [x] Registro do topic producer adicionado em `NativeInjectorConfig.cs` junto aos existentes (`contract-events`, `pluvia-events`), seguindo a mesma convenção.
  - [x] `dotnet build` do `EtrmService.slnx` passa sem erros.
  - [ ] (Extensível) Publicação manual confirmada no Kafka UI (`localhost:9000`) com o evento no tópico correto. *(requer ambiente em execução)*
- **Arquivos a Modificar:**
  - `backend/etrm-service/EtrmService.API/IoC/NativeInjectorConfig.cs` (adicionar producer)
  - (verificar) `backend/etrm-service/EtrmService.Application/Operations/Commands/PublishOperationCommand.cs`
- **Dependências:** Nenhuma bloqueante.

### 2. 🔴 Bug Fix: Código Órfão Quebrado `EtrmService.Api` (`backend-architect`)
- **Contexto de Negócio:** Existe uma pasta `EtrmService.Api/` (minúsculo) desatualizada, contendo `B2bOperationsController` e `CceeIntegrationController` que referenciam o namespace inexistente `EtrmService.Api.Controllers.Shared`. Ela não possui `.csproj`, não compila e **não faz parte da solution**.
- **Decisão:** Como não é referenciada por nenhum projeto e representa lógica duplicada/desatualizada das funcionalidades já existentes em `EtrmService.API`, deve ser **removida** do repositório.
- **Critérios de Aceite:**
  - [x] Pasta `backend/etrm-service/EtrmService.Api/` (minúsculo) removida do repositório.
  - [x] Nenhum projeto restante referencia o namespace `EtrmService.Api.*`.
  - [x] `dotnet build` do `EtrmService.slnx` passa sem erros.
  - [x] `git status` limpo após `git rm -r backend/etrm-service/EtrmService.Api`.
- **Arquivos a Modificar:**
  - Deleção: `backend/etrm-service/EtrmService.Api/` (recursivo).
- **Dependências:** Nenhuma.

### 3. 🔴 Bug Fix: `mf-operations` apontando para porta do MLflow (`frontend-master`)
- **Contexto de Negócio:** O módulo BackOffice (Operações financeiras, contratos, CCEE) depende da API ETRM para dados transacionais. O `environment.ts` usa `apiUrl: 'http://localhost:5000'`, que é a porta do **MLflow** — o ETRM expõe sua API em **8080** (conforme `app-shell`/docker-compose). Resultado: todas as chamadas do BackOffice falham.
- **Critérios de Aceite:**
  - [x] `apiUrl` do `mf-operations` aponta para `http://localhost:8080/api/v1` (ou o padrão do `app-shell`).
  - [ ] As chamadas HTTP do `mf-operations` retornam 200 contra a API ETRM real. *(requer backend + MFE em execução)*
  - [x] Arquivo `environment.production.ts` (se existir) também corrigido/consistente com o padrão de runtime. *(não existe variante de produção no MFE)*
- **Arquivos a Modificar:**
  - `frontend/mf-operations/src/environments/environment.ts`
  - (verificar) `frontend/mf-operations/src/environments/environment.production.ts`
- **Dependências:** Serviço ETRM em execução na porta correta.

### 4. 🟠 Débito de Compliance: Persistência da Auditoria no Pipeline (`backend-architect`)
- **Contexto de Negócio:** A regra do módulo **Menza (`TradingCopilot`)** exige *"Auditoria Transparente: Todo Command/Query deve ser interceptado pelo pipeline MediatR (`AuditLoggingBehavior`)"*. Hoje o behavior apenas loga no `ILogger` e não persiste o `AuditLog`, o que **viola a regra de negócio e a conformidade regulatória** (rastreabilidade de operações de trading).
- **Escopo Mínimo (Sprint 10):** Persistir o `AuditLog` de forma genérica dentro do `AuditLoggingBehavior` (gravar entidade via `IEtrmDbContext`, usando `IUserService` para `UserId`/`TenantId`), sem depender de handlers individuais.
- **Critérios de Aceite:**
  - [x] `AuditLoggingBehavior` persiste uma entrada `AuditLog` para cada Command/Query executado com sucesso/erro.
  - [x] `TenantId`/`UserId` populados corretamente (multi-tenancy preservado via Global Query Filters).
  - [x] Migração EF Core adicionada (se necessário) e aplicada. *(não necessária — tabela `audit_logs` já existente)*
  - [x] `dotnet build` do `EtrmService.slnx` sem erros.
  - [x] `dotnet test` passa (19/19).
  - [ ] Teste unitário **dedicado** do `AuditLoggingBehavior` verificando a gravação no `DbContext`. *(não criado nesta sprint — débito registrado para refinamento; suite existente segue verde 19/19)*
- **Arquivos a Modificar:**
  - `backend/etrm-service/EtrmService.Application/Behaviors/AuditLoggingBehavior.cs`
  - `backend/etrm-service/EtrmService.Infrastructure/Persistence/` (contexto/migração)
  - `backend/etrm-service/EtrmService.UnitTests/` (teste do behavior)
- **Dependências:** Item 1 (estabilidade do pipeline) recomendado como pré-requisito.

## Definição de Pronto (DoD) da Sprint
- [x] Todos os critérios de aceite de código/build acima atendidos. *(itens que exigem runtime — Kafka UI manual e HTTP 200, e o teste unitário dedicado do behavior — registrados como débito/verificação em execução)*
- [x] `dotnet build` do `EtrmService.slnx` sem erros (0 erros, 19 warnings pré-existentes); `ng build` do `mf-operations` sem erros.
- [x] Testes unitários existentes passando (`dotnet test` — 19/19).
- [x] Nenhum novo mock introduzido; nenhuma regressão nos fluxos Menza/BackOffice.

## Fora de Escopo (próxima Sprint recomendada)
- Implementar `prospect.service.ts` real (integração total Prospec).
- Conectar `WebhookService` com retry/Polly e URL configurável.
- Corrigir fallback anônimo de auth do `risk-service` e `[Authorize]` do `B2bOperationsController`.
- Atualizar documentação para Angular 22 / Native Federation.
