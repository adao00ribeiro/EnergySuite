# Plano de Sprints: Energy Prospect

Com base na sua análise técnica profunda e no SPEC fornecido (espelhado no Prospec da Norus), estruturei a implementação da plataforma **Energy Prospect** em **6 Sprints** altamente coesas. A arquitetura utilizará a base existente do EnergySuite: o backend será construído dentro do **`etrm-service`** (como um novo módulo interno) e o frontend será acoplado ao micro-frontend **`mf-pricing`** (já que Prospec lida fortemente com cálculo de preços/PLD). Tudo isso amparado por mensageria (Kafka/MassTransit), armazenamento (MinIO) e um parque de Workers.

---

## Roadmap de Sprints

### Sprint 1: Fundação, Estudos e Gestão de Arquivos
**Objetivo:** Estabelecer a infraestrutura básica do serviço, permitindo o CRUD de estudos e upload dos decks originais no Object Storage.

* **Tarefas:**
  - [x] **Infra:** Configurar estrutura de pastas `Prospect` dentro do `etrm-service` (API, Domain, Infrastructure, Application).
  - [x] **Infra:** Configurar rotas e submódulo `prospect` dentro do MFE existente `mf-pricing`.
  - [x] **Backend:** Modelar a entidade `Study`, `StudyTag`, e `StudyFile` (Estado Inicial: CREATED, UPLOADING).
  - [x] **Backend:** Implementar integração com MinIO/S3 para upload e download de arquivos e extração de ZIPs.
  - [x] **Frontend:** Criar a "Central de Estudos" (Tabela com filtros, ordenação e tags coloridas).
  - [x] **Frontend:** Criar o formulário de "Novo Estudo" e *Drag-and-Drop* para upload de arquivos base.

### Sprint 2: Motor de Geração de Decks e Planilha Facilitadora
**Objetivo:** Permitir ao analista enviar uma planilha Excel que será validada, processada e convertida na árvore de Decks mensais/semanais.

* **Tarefas:**
  - [ ] **Backend:** Implementar Parser de Excel (via ClosedXML ou similar) para importar premissas.
  - [ ] **Backend:** Desenvolver o Gerador de Decks, clonando o Deck base para os N meses do horizonte especificado.
  - [ ] **Backend:** Integração e geração automática de cenários e arquivos (`vazoes.dat`, simulando GEVAZP).
  - [ ] **Frontend:** Painel de configuração de Premissas (Modificação de parâmetros) por interface gráfica.
  - [ ] **Frontend:** Visualização da árvore gerada de Decks Futuros antes da execução.

### Sprint 3: Orquestração de Execução e Workers Distribuídos
**Objetivo:** Criar o core de execução. O usuário manda executar o estudo, e o orquestrador gerencia a fila e envia comandos para os *Workers*.

* **Tarefas:**
  - [ ] **Backend:** Criar a Máquina de Estados de Execução (QUEUED -> RUNNING -> PROCESSING -> COMPLETED/FAILED).
  - [ ] **Backend (Mensageria):** Configurar fila `model-execution-jobs` (RabbitMQ/Kafka).
  - [ ] **Backend (Workers):** Criar `ModelRunnerWorker` que faz Pull da fila, baixa arquivos do S3, "roda" o modelo e reenvia resultados.
  - [ ] **Frontend:** Monitoramento em Tempo Real via WebSocket (SignalR).
  - [ ] **Frontend:** Interface de Logs dinâmicos, transmitindo saídas do *stdout/stderr* do worker na UI.

### Sprint 4: Encadeamento e Tratamento de Inviabilidades
**Objetivo:** Conectar a saída de um modelo na entrada de outro (NW -> DC) e lidar inteligentemente com erros no processo.

* **Tarefas:**
  - [ ] **Backend:** Criar o *Workflow Engine* para encadeamento automático (Ex: Terminou NEWAVE -> dispara job DECOMP Mês 1).
  - [ ] **Backend:** Simulador de Inviabilidade (se uma flag existir, o worker falha de propósito).
  - [ ] **Backend:** Algoritmo de "Ajuste Automático" (Tenta corrigir e refaz o job) vs. Estado de "Aguardando Ajuste Manual".
  - [ ] **Frontend:** Fluxo de resolução de inviabilidade na interface (Upload de patch manual).

### Sprint 5: Processador de Resultados e Dashboards
**Objetivo:** Quando os Workers finalizam com sucesso, os dados binários e relatórios devem ser processados em banco para renderização rápida.

* **Tarefas:**
  - [ ] **Backend:** Criar `ResultProcessor` (Extrai PLD, ENA, CMO dos resultados gerados e insere no PostgreSQL).
  - [ ] **API:** Endpoints `/api/v1/results/{id}/chart-data` flexíveis por eixo X, Y e Tipo.
  - [ ] **Frontend:** Integração do Construtor de Gráficos (Echarts ou Chart.js) visualizando Linhas/Barras e Tabelas Consolidadas.
  - [ ] **Frontend:** Botão de Exportação e Download de relatórios CSV, XLSX e PNG.
  - [ ] **Geral:** Disparo de e-mails/notificações quando o Estudo termina.

### Sprint 6: API Cliente e Reaproveitamento
**Objetivo:** Expor todas essas capacidades via API pública (M2M) e permitir o reuso de dados de estudos anteriores.

* **Tarefas:**
  - [ ] **Backend:** Feature de "Reaproveitar Resultados" (Clona a referência MinIO de reservatórios e premissas hidrológicas para um Novo Estudo).
  - [ ] **API Gateway:** Documentar (Swagger) e blindar com JWT Client Credentials as rotas B2B `/api/v1/studies`, `/api/v1/decks/generate`, `/api/v1/executions`.
  - [ ] **Backend:** Implementar disparos de Webhooks (`study.started`, `study.completed`, `deck.completed`) para APIs externas de clientes.
