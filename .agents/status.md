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
- **Próximos Passos (To-Do):**
  - Integrar o App Shell com a API (chamadas HTTP ao ETRM Service).

---

## 🟡 Backend: Risk & Prospec Service (Python)
- **Status:** Apenas Dockerfile criado.
- **Concluído:**
  - `Dockerfile` (FastAPI).
  - Iniciar projeto base do FastAPI.
  - Criar consumidor base do Kafka (`aiokafka`).
- **Próximos Passos (To-Do):**
  - Implementar lógica de cálculo de risco ao receber o evento de contrato e expor métricas.
