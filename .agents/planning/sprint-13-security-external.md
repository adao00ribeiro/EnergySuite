# Sprint 13: Segurança de Webhooks, Integrações Externas e Dados de Ciência (P2)

**Status:** ✅ **CONCLUÍDA** (2026-08-28) — dotnet build 0 erros, dotnet test 21/21 (19 + 2 novos), risk-service compila (compileall OK).
**Objetivo:** Endurecer segurança de produção (assinatura HMAC, URLs de webhook configuráveis), implementar as integrações externas reais (CCEE/BBCE external sync, WebhookNotifierService com HTTP/Polly) e eliminar os mocks do `risk-service` (Python) e `mlops` (Airflow). Também cobre o teste unitário dedicado do `AuditLoggingBehavior` (débito S10-1).

**Justificativa de Negócio:** Em produção, webhooks **sem assinatura HMAC** (chave crua no header) e **URLs hardcoded de teste** (`webhook.site`, `b2b-customer.internal`) são vetores de ataque e inviabilizam o B2B real. O `ExternalTradeSyncService` sincroniza trades com a CCEE mas **descarta o payload** e envia valores fixos. O `risk-service` e o `mlops` fabricam dados científicos (precipitação 40x40 aleatória, GEVAZP dummy, sleep simulando SMAP, séries de treino aleatórias), o que invalida qualquer decisão de trading baseada neles.

---

## PARTE A — BACKEND .NET (`backend-architect`)

### A1. 🔴 BK-12: Assinatura HMAC real nos webhooks
- **Contexto:** `OperationPublishedEventConsumer.cs:67` `// Mock implementation` — o header `X-EnergySuite-Signature` recebe a **chave crua**, sem assinatura HMAC real. O destinatário B2B não consegue verificar autenticidade/integridade.
- **Ação (`backend-architect`):** implementar assinatura HMAC-SHA256 do payload (`secret + payload`) gerada via `System.Security.Cryptography`, em header `X-EnergySuite-Signature` no formato `sha256=<hex>`. A `secret` do webhook vem do registro de `WebhookSubscription` (não da URL). Extrair para um helper compartilhado (ex: `WebhookSigningService` em Application/Infrastructure) e usar nos pontos de disparo.
- **Critérios de Aceite:** header contém assinatura HMAC derivada do payload; nenhuma chave crua transitando; build/test verdes.

### A2. 🔴 BK-12(b): URLs de webhook configuráveis (remover hardcoded de teste)
- **Contexto:** `EnaCalculatedEventConsumer.cs:60-64` usa URL fixa `http://b2b-customer.internal/api/webhooks/pluvia`; `WebhookService.cs:33` (Infrastructure) tem `BaseAddress` default `https://webhook.site/energy-suite-events` (página pública de teste).
- **Ação (`backend-architect`):** mover ambas para configuração (`appsettings`/env: ex `Webhooks:PluviaUrl`, `Webhooks:DefaultBaseAddress`). Se não configurado, **não disparar** (log warning), nunca usar default público. Em `EnaCalculatedEventConsumer`, o webhook deve ser consultado de `WebhookSubscriptionRepository` (quando existir subscription) ou config.
- **Critérios de Aceite:** grep por `webhook.site` e `b2b-customer.internal` zerado no código de produção; build/test verdes.

### A3. 🟠 BK-14: `WebhookNotifierService` com HTTP real + retry
- **Contexto:** `WebhookNotifierService.cs:23-25` só loga `[WEBHOOK DISPATCHED]` (`// Simulate HTTP POST...`) via `IWebhookService` — nunca envia HTTP.
- **Ação (`backend-architect`):** implementar POST real com `HttpClientFactory` + policy de retry (Polly: ex: 3 tentativas com backoff) usando o `IWebhookService` real existente (agora com URLs configuráveis). Fire-and-forget seguro com log de falha.
- **Critérios de Aceite:** método dispara POST HTTP real; falha registrada e retentada; build/test verdes.

### A4. 🟠 BK-13: `ExternalTradeSyncService` real (CCEE/BBCE)
- **Contexto:** `ExternalTradeSyncService.cs:24-65` faz GET real em `https://api.ccee.org.br/v1/trades/sync?status=pending` mas **não desserializa o corpo** e envia comando com valores fixos `15.5m/250.0m` e `ExternalId = Guid.NewGuid()`; BBCE/N5X citados no log mas nunca chamados.
- **Ação (`backend-architect`):** desserializar a resposta (DTO do payload), mapear para `CreateSwapOperationCommand`/`CreateOperationCommand` com os dados reais (volume, preço, datas, contraparte), aplicar regra de negócio das Sprints 8-9 (criar draft se contraparte existir, rejeitar/log se não). Parametrizar base URL/credenciais no appsettings. Remover valores fixos e `Guid.NewGuid()` de ExternalId (usar id real da API externa).
- **Critérios de Aceite:** payload externo desserializado e usado nos comandos; zero valores fixos de negócio; build/test verdes.

### A5. 🟠 S10-1: Teste unitário dedicado do `AuditLoggingBehavior`
- **Contexto:** Débito registrado na Sprint 10: teste dedicado verificando persistência do `AuditLog` no `IEtrmDbContext` (sucesso e falha) nunca criado.
- **Ação (`backend-architect`):** criar `AuditLoggingBehaviorTests` em `EtrmService.UnitTests` cobrindo: (1) command bem-sucedido persiste `AuditLog`; (2) command com exceção persiste `AuditLog` de falha. Usar mock de `IEtrmDbContext`/in-memory conforme padrão dos testes existentes (`ExecuteAccountOffsetCommandHandlerTests` etc.).
- **Critérios de Aceite:** suite `dotnet test` passa com os novos testes (19/19 + N novos).

### A6. 🟠 BK-16 (avaliação): DbSets de cadastro sem uso
- **Contexto:** DbSets `Persons`, `EconomicGroups`, `PriceIndexValues`, `DocumentAttachments`, `ContractAmendments` declarados em `EtrmDbContext.cs:18-31` com 0 usos.
- **Ação (`backend-architect`):** decidir se pertencem a escopo futuro (cadastro de contrapartes/contratos — manter e documentar) ou remover. **Recomendado:** manter (Já há `Companies`/`EconomicGroups` coerentes com o domínio). Se mantiver, sem ação de código — apenas registrar a decisão na resposta.
- **Critérios de Aceite:** decisão registrada; build/test verdes.

---

## PARTE B — PYTHON (`backend-architect` + `risk-scientist`)

> Convenção: módulos Python usam FastAPI/Pydantic e **NumPy/Pandas** (sem loops). Dados massivos em Parquet.

### B1. 🔴 BK-15(1): `risk-service` — remover mock de precipitação 40x40
- **Contexto:** `risk-service/src/main.py:104-175` `get_precipitation_map` retorna matriz 40x40 aleatória (numpy), docstring "mockada para a Sprint 2". O mf-hydrology consome esse endpoint (`precipitation-map`), exibindo chuva inventada.
- **Ação (`risk-scientist`):** implementar leitura real da fonte de dados (Parquet no MinIO/Data Lake, ingestão via `mlops/dags/ingest_meteorological_data.py` — NOAA/ECMWF/CPTEC). Se não houver dados para o período, retornar estado vazio/erro honesto (HTTP 404/204) — nunca matriz aleatória. Vectorizar com NumPy/Pandas.
- **Critérios de Aceite:** nenhum `np.random` de dados de negócio no endpoint; resposta/documentação coerente; testes do serviço se existirem passando.

### B2. 🟠 BK-15(2): `risk-service` — SMAP e ENA reais (remover `sleep`/`uniform`)
- **Contexto:** `kafka_consumer.py:58-63` `// Simulate heavy SMAP calculation` (`asyncio.sleep(2)`) e ENA gerada com `random.uniform` para 4 submercados × 12 meses.
- **Ação (`risk-scientist`):** substituir por cálculo real (SMAP/hidrológico executado sobre os dados de ENA/GSF do Data Lake; se o modelo científico não estiver no escopo mínimo, ao menos calcular ENA a partir dos dados reais de precipitação por submercado/bacia — vectorizado — em vez de `uniform`). Persistir em `HydrologicalResults` via tópico `ena-events` (consumido pelo .NET — Sprint 11).
- **Critérios de Aceite:** nenhum `random.uniform` de ENA; nenhum `sleep` simulando SMAP; script consome/calcula dados reais.

### B3. 🟠 BK-15(3): `risk-service` — GEVAZP real
- **Contexto:** `gevazp_generator.py:23-31` `# Create dummy content representing ONS/CCEE standard txt files` — gera 5 arquivos `.rv0` de conteúdo fixo.
- **Ação (`risk-scientist`):** gerar os arquivos GEVAZP a partir de dados reais (ENA/hidrologia persistidos), no formato ONS/CCEE correto, e enviá-los ao MinIO (mesmo bucket `datalake/exports` que o frontend consome). Se o formato/dados não estiverem disponíveis no sprint, deixar a publicação vazia/erro honesto.
- **Critérios de Aceite:** nenhum arquivo `.rv0` de conteúdo fixo/dummy gerado sem origem de dados.

### B4. 🟠 BK-15(4): `mlops` — treino com dados reais
- **Contexto:** `mlops/dags/train_hydrological_model.py:32-39` e `train_price_model.py:55-76,168-174` usam `# Mock parameters/metrics` e séries aleatórias (`extract_simulate_data`) em vez de ler o Data Lake; `log_model` comentado.
- **Ação (`risk-scientist`):** DAGs de treino devem ler Parquet/Metadados reais do Data Lake/MLflow (produzidos por `ingest_meteorological_data.py` e cálculos do risk-service), remover `random`/`simulate`, habilitar `log_model`/artefatos no MLflow (regra do AGENTS.md): rastreabilidade de ML.
- **Critérios de Aceite:** nenhum dado de treino aleatório; artefatos registrados no MLflow; DAGs executáveis sem mock.

---

## DoD da Sprint
- [x] `dotnet build` do `EtrmService.slnx` sem erros; `dotnet test` verde (21/21 = 19 baseline + 2 novos do AuditLoggingBehavior).
- [x] Grep em código de produção por `webhook.site`, `b2b-customer.internal`, `[WEBHOOK DISPATCHED]`, `15.5m/250.0m` e `api.ccee.org.br` hardcoded → 0 nos arquivos alterados; assinatura HMAC (`sha256=<hex>`) present nos 3 pontos de webhook.
- [x] Python: `python -m compileall` no `risk-service` / `mlops/dags` OK; `random.uniform`/`np.random` de dados de negócio → 0; `asyncio.sleep` de simulação → 0 (restante = retry de conexão Kafka, legítimo).
- [x] Decisões B registradas (parquet gold/silver; estado vazio honesto 404; `train_state=NO_DATA` no MLflow).

## Fora de Escopo / Sprint 14+
- Modelo SMAP científico completo (ENA usa proxy de projeção por persistência da série real — vetorizado).
- Parser de GRIB2/escrita gold/silver em `ingest_meteorological_data.py` (exigiria `cfgrib`/`xarray`).
- Integração BBCE/N5X além do mapeamento atual (depende de credenciais/contratos).
- `TradingCopilotService` / `ImerisCreditClient` (simulações ainda presentes), `ProcessCliqCceeCsv` parsing mock (CCEE).
- `HydrologicalSimulationJob`/Quartz multi-tenancy em `Guid.Empty`; endpoints `forward-curve`, VaR/summary, session logs, `requestedBy/requestedAt`.