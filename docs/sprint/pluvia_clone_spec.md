# Especificação de Implementação: Clone do Módulo Pluvia (EnergySuite)

Este documento detalha o planejamento em sprints e tarefas (tasks) para a implementação do módulo "Pluvia" (Previsão Hidrológica e Geração de Cenários), integrado à arquitetura *Cloud-Native*, orientada a eventos e baseada em Micro-frontends do ecossistema **EnergySuite**.

As diretrizes arquiteturais respeitam rigorosamente os padrões definidos para o backend (.NET 8 C# com Clean Architecture/CQRS), frontend (Angular 18 com Standalone Components e Webpack Module Federation) e engenharia de dados/risco (Python/FastAPI, Data Lakehouse, Kafka e EKS Spot).

---

## Visão Geral da Arquitetura do Módulo Pluvia

- **Micro-frontend (Angular 18):** Módulo independente `mf-hydrology` que se integra ao `app-shell`. Responsável por dashboards, mapas de precipitação (ECharts/Leaflet), criação de cenários customizados e gráficos de ENA.
- **Backend Core (.NET C#):** Gerencia entidades de negócio (Cenários, Agendamentos, Permissões), utiliza PostgreSQL (EF Core Fluent API) e expõe queries/commands via GraphQL/MediatR.
- **Engine Hidrológica & MLOps (Python):** Processamento assíncrono para consumir cenários, rodar modelos (SMAP, IA, PREVIVAZ) em instâncias Kubernetes Spot (EKS) e salvar resultados massivos no Data Lakehouse. Orquestração com Apache Airflow.
- **Mensageria:** Apache Kafka centraliza a fila de simulações e eventos do sistema.
- **Observabilidade:** OpenTelemetry implementado de ponta a ponta para rastrear o tempo de execução dos modelos matemáticos.

---

## 📌 Estado Atual (Baseline)
O projeto já conta com uma base inicial implementada:
- ✅ **Frontend Core:** O micro-frontend `mf-hydrology` já está inicializado, configurado com *Module Federation* e roteamento.
- ✅ **Dashboard Base:** A estrutura do `hydrology-dashboard` já existe.
- ✅ **Gráficos ENA:** O componente `reservoir-levels-chart` já está criado com `ngx-echarts` mostrando dados simulados (mock) do "ENA Histórico" e "ENA Projeção (ML)".
- ✅ **Componentes Auxiliares:** Componente de `mlops-status` preparado e DAGs base de MLOps no backend configurados.

---

## 📅 Roadmap de Sprints (Ajustado)

### Sprint 1: Fundação Backend e Engenharia de Dados Base
**Objetivo:** Estabelecer as APIs base no .NET e infraestrutura do Data Lakehouse para ingestão meteorológica (aproveitando o frontend já existente).

* **Task 1.1 (Frontend):** *[Concluída]* Módulo `mf-hydrology` inicializado e integrado ao `app-shell`.
* **Task 1.2 (Frontend):** *[Concluída]* Expandir os layouts base do `mf-hydrology` para incluir abas/navegação para: "Precipitação", "Modelos Hidrológicos", "Cenários Customizados" e "Análises ENA".
* **Task 1.3 (Backend - .NET):** *[Concluída]* Criar os *Domain Models* (Agregados: `PrecipitationScenario`, `ModelExecution`, `HydrologicalResult`) seguindo a Clean Architecture e EF Core Fluent API.
* **Task 1.4 (Backend - .NET):** *[Concluída]* Configurar endpoints básicos via MediatR (Commands/Queries) com `Asp.Versioning` e injeção do OpenTelemetry.
* **Task 1.5 (Engenharia de Dados - Python):** *[Concluída]* Criar DAGs no Apache Airflow para ingestão diária de dados meteorológicos oficiais (ETA, GEFS, ECMWF) e salvar séries temporais brutas no Data Lakehouse (Apache Iceberg/Delta Lake).

### Sprint 2: Visualização de Chuva e Mapas Meteorológicos
**Objetivo:** Permitir que o usuário consulte diferentes modelos e mapas de precipitação no frontend.

* **Task 2.1 (Backend - .NET):** *[Concluída]* Criar queries otimizadas (`AsNoTracking`) no .NET para buscar metadados de previsões de precipitação disponíveis no Lakehouse via integração.
* **Task 2.2 (Data - Python):** *[Concluída]* Desenvolver API (`FastAPI`) para servir dados matriciais (GeoJSON/Grids) baseados nos arquivos armazenados na *bronze layer*.
* **Task 2.3 (Frontend - Angular):** *[Concluída]* Criar componente standalone de visualização de mapas combinando `ECharts` (heatmap/scatter geoespacial) no módulo `mf-hydrology`.
* **Task 2.4 (Frontend - Angular):** *[Concluída]* Implementar gerenciamento de estado local com **Angular Signals** (Filtro por modelo e data) para reatividade otimizada na renderização dos mapas.

### Sprint 3: Motor Hidrológico (Chuva -> Vazão) e MLOps
**Objetivo:** Implementar o core do sistema, convertendo chuva em vazão através de múltiplos modelos.

* **Task 3.1 (MLOps - Python):** *[Concluída]* Containerizar os modelos hidrológicos (SMAP, IA, PREVIVAZ) e integrá-los com **MLflow** para versionamento dos modelos de ML.
* **Task 3.2 (Infra/Kubernetes):** *[Concluída]* Configurar *Workers* no EKS (Spot Instances) que consomem a fila do Kafka para processamento paralelo de centenas de cenários.
* **Task 3.3 (Backend - .NET):** *[Concluída]* Implementar o command `RunHydrologicalSimulationCommand`. O controller (herdado de `ApiControllerBase`) deve disparar um evento `SimulationRequestedIntegrationEvent` no **Apache Kafka**.
* **Task 3.4 (Data - Python):** *[Concluída]* Criar os consumers Kafka (usando `confluent-kafka-python` ou Faust) que orquestram os cálculos no EKS e salvam o resultado (Vazão Afluente) no Lakehouse.

### Sprint 4: Criação Massiva e Customização de Cenários
**Objetivo:** Habilitar cenários customizados e combinação de mapas pelo usuário.

* **Task 4.1 (Frontend):** *[Concluída]* Criar interface "Meus Mapas" com formulários reativos (`ReactiveFormsModule`) para permitir o *upload* de arquivos CSV de precipitação customizada.
* **Task 4.2 (Frontend):** *[Concluída]* Desenvolver componente de UI para combinar até 3 mapas meteorológicos, com inputs numéricos para atribuição de pesos percentuais (Ex: 50% ECMWF, 30% GEFS).
* **Task 4.3 (Backend - .NET):** *[Concluída]* Criar endpoints de upload e validação de CSV, persistindo o arquivo temporariamente em *blob storage* (S3/MinIO) e registrando a entidade `CustomScenario`.
* **Task 4.4 (Data - Python):** *[Concluída]* Ajustar o worker de processamento para aceitar rasters/CSVs customizados e misturar (blend) matrizes de chuva antes da simulação hidrológica.

### Sprint 5: Cálculo e Integração de ENA (Energia Natural Afluente)
**Objetivo:** Transformar dados de vazão em ENA e plugar os dados reais no gráfico já existente.

* **Task 5.1 (Data - Python):** *[Concluída]* Implementar rotina de cálculo que converte as matrizes de Vazão em MWmed e %MLT, dividindo por submercado e Bacia.
* **Task 5.2 (Backend - .NET):** *[Concluída]* Expor APIs de agregação que retornam dados de ENA consolidados (histórico vs. previsto vs. anterior).
* **Task 5.3 (Frontend):** *[Concluída]* Refatorar o dashboard `reservoir-levels-chart` existente para consumir a API .NET, substituindo o mock de dados por séries reais comparando previsões de ENA e cenários.
* **Task 5.4 (Frontend):** *[Concluída]* Criar funcionalidade visual para mostrar evolução da previsão (hoje, ontem, -2 dias, etc.).

### Sprint 6: Agendamento, Automação e Arquivos de Saída (GEVAZP)
**Objetivo:** Transformar o Pluvia numa ferramenta de workflow setorial integrável a outros sistemas de despacho.

* **Task 6.1 (Backend - .NET):** *[Concluída]* Desenvolver funcionalidade de Agendamento (Cron Jobs) utilizando bibliotecas nativas como `Quartz.NET` ou delegando agendamentos recorrentes ao Airflow.
* **Task 6.2 (Backend - .NET):** *[Concluída]* Implementar rastreamento (telemetria) de execuções para o usuário (dashboards de status "em andamento", "falhou"), conectando com o componente `mlops-status`.
* **Task 6.3 (Data - Python):** *[Concluída]* Criar geradores dos arquivos setoriais oficiais: `VNA`, `ENA`, `PREVS`, `DADVAZ` e `STR`.
* **Task 6.4 (Frontend):** *[Concluída]* Disponibilizar área de "Downloads e Exportações", permitindo baixar CSVs ou arquivos nativos prontos para o DECOMP/GEVAZP/DESSEM.

### Sprint 7: APIs Externas (B2B), Webhooks e Refinamentos
**Objetivo:** Tornar o sistema integrável e enterprise-ready.

* **Task 7.1 (Backend - .NET):** *[Concluída]* Criar uma camada de APIs públicas B2B (API Gateway) restritas via API Keys ou JWT (Keycloak).
* **Task 7.2 (Backend - .NET):** *[Concluída]* Implementar sistema de Webhooks. Ao fim do cálculo de um `PREVS` no Kafka, o microsserviço dispara callbacks HTTP.
* **Task 7.3 (DevSecOps):** *[Concluída]* Implementar *Rate Limiting* (por IP/Tenant) rigoroso nos endpoints abertos da API B2B.
* **Task 7.4 (Frontend & Backend):** *[Concluída]* Validação cruzada de permissões. O Angular e o .NET devem conferir acesso via Claims (ex: "CanViewENA").

---

## 🛠️ Padrões Operacionais Durante a Implementação

1. **Reaproveitamento Frontend**: Utilize a base do projeto `mf-hydrology` para criar os novos submódulos do Pluvia, mantendo coesão visual com o `hydrology-dashboard`.
2. **Mensageria**: Nenhum serviço web (C#) deve ficar aguardando de forma síncrona a geração dos milhares de cenários do EKS.
3. **Observabilidade (OpenTelemetry)**: Como teremos cálculos complexos e geração massiva, traces distribuídos são essenciais. O correlation ID do click no botão "Simular" do Angular deve ser repassado para toda a cadeia.
4. **Custo de Nuvem (FinOps)**: Os endpoints devem carregar apenas os agregados necessários do Lakehouse.
