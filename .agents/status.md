# Project Status: Suite for Energy (Clone)

> **Aviso para Agentes de IA:** Leia este documento para entender em que ponto o projeto se encontra antes de propor novas implementações ou refatorações.

## Visão Geral do Sistema
- **Objetivo:** Criar um clone da plataforma Norus (ETRM, Risco e Precificação).
- **Repositório:** GitHub sincronizado (`adao00ribeiro/EnergySuite`).
- **Infraestrutura Local:** Arquivo `docker-compose.yml` finalizado (Kafka, PostgreSQL, Keycloak, Prometheus, Jaeger, Grafana).

---

## 🟢 Backend: ETRM Service (.NET 8)
- **Status:** Estrutura base da API concluída (CQRS + Banco + Testes)
- **Concluído:**
  - `EtrmService.Domain`, `EtrmService.Application`, `EtrmService.Infrastructure`, `EtrmService.API` criados.
  - Projeto `EtrmService.UnitTests` implementado com xUnit, Moq e Shouldly.
  - Entidade `Contract` criada e Repositório implementado.
  - Entity Framework Core configurado com PostgreSQL (Migration `InitialCreate` gerada).
  - Padrão CQRS implementado via `MediatR` (`CreateContractCommand` e `Handler` funcionando).
  - `ContractsController` exposto.
  - Configurar Kafka Producer para disparar evento quando contrato for criado (MassTransit).
  - Adicionar as validações com `FluentValidation` no pipeline do MediatR.
  - Implementar Queries de Leitura (`GetContractByIdQuery`, `GetContractsList`).
- **Próximos Passos (To-Do):**
  - Configurar testes de integração para Endpoints (API).

---

## 🟢 Frontend: Portal Unificado (Angular 22)
- **Status:** Scaffolding inicializado (Standalone Components).
- **Concluído:**
  - `Dockerfile` (Multi-stage com NGINX).
  - Repositório App Shell gerado via `ng new` (SCSS, Routing).
  - Angular Material instalado (`@angular/material`).
  - Configurar interface inicial (Sidenav, Toolbar).
  - Configurar Webpack Module Federation para os Micro-frontends.
  - Integrar o App Shell com a API (Serviço `ContractService` com Signals e HttpClient para `ContractsController`).
  - Implementar telas de listagem (`ContractListComponent`) e criação (`ContractCreateComponent`) com formulários reativos Material.
  - **Dashboard Visual e Risk Service (Imeris):** Componente `ExecutiveDashboardComponent` criado consumindo métricas em tempo real (`RiskSignalrService`) e APIs MLOps.

---

## 🟢 Backend: Risk & Prospec Service (Python) e MLOps (Pluvia)
- **Status:** Concluído e integrado (FastAPI + Kafka + OpenTelemetry + PostgreSQL Async + Airflow MLOps).
- **Concluído:**
  - `Dockerfile` (FastAPI com timeout estendido do pip).
  - FastAPI configurado com OpenTelemetry Tracing (OTLP/Tempo) e instrumentação SQLAlchemy.
  - Consumidor/Produtor Kafka com `aiokafka` (`contract-events` ➔ `risk-events`).
  - Motor de Risco (`RiskEngine`) com cálculo de Exposição Financeira, Mark-to-Market (MtM) e categoria de risco.
  - Métricas Prometheus (`risk_mtm_value`, `risk_contracts_processed_total`).
  - Persistência assíncrona dos cálculos de risco no PostgreSQL.
  - Autenticação OIDC via Keycloak JWT (`auth.py`).
  - Endpoint REST `/api/v1/metrics/contracts/{contract_id}` exposto.
  - **Módulo Pluvia (MLOps):** Pipeline Airflow configurada para treinamento de previsão de preço/PLD em `backend/mlops`.
  - **Testes E2E Completos:** Fluxo End-to-End no Kubernetes validado (Contrato ➔ Kafka ➔ Risk Service ➔ Banco de Risco ➔ Dashboard Angular).

---

## 🎯 Status Geral e Próximos Passos
**O MVP do EnergySuite (Clone da Norus) está com 100% dos fluxos base integrados!** 🚀
As frentes de ETRM, MLOps e Risco comunicam-se perfeitamente via Kafka e REST.

**Próximos passos (A definir):**
- Deploy em nuvem pública (AWS EKS, Azure AKS) via GitHub Actions CI/CD.
- Refinamentos de UX/UI no Angular.
- Implementar novas regras de precificação/risco (ex: Opções, Swap).
