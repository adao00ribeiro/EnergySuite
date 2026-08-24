# Project Status: Suite for Energy (Clone)

> **Aviso para Agentes de IA:** Leia este documento para entender em que ponto o projeto se encontra antes de propor novas implementações ou refatorações.

## Visão Geral do Sistema
- **Objetivo:** Criar um clone da plataforma Norus (ETRM, Risco e Precificação).
- **Repositório:** GitHub sincronizado (`adao00ribeiro/EnergySuite`).
- **Infraestrutura Local:** Arquivo `docker-compose.yml` finalizado (Kafka, PostgreSQL, Keycloak, Prometheus, Jaeger, Grafana).

---

## 🟢 Backend: ETRM Service (.NET 8)
- **Status:** Iniciado (Scaffolding da Clean Architecture)
- **Concluído:**
  - `EtrmService.Domain`, `EtrmService.Application`, `EtrmService.Infrastructure`, `EtrmService.API` criados e referenciados.
  - Entidade `Contract` criada (`EtrmService.Domain/Entities/Contract.cs`).
  - Enumeradores `ContractType` e `EnergySubmarket` criados.
- **Próximos Passos (To-Do):**
  - Instalar dependências (MediatR, Entity Framework Core).
  - Criar as migrations do EF Core no PostgreSQL.
  - Criar os Commands e Queries de `Contract`.

---

## 🟡 Frontend: Portal Unificado (Angular 18)
- **Status:** Apenas Dockerfile criado.
- **Concluído:**
  - `Dockerfile` (Multi-stage com NGINX).
- **Próximos Passos (To-Do):**
  - Rodar `ng new` para inicializar o repositório do App Shell.
  - Configurar Angular Material.
  - Configurar Webpack Module Federation para os Micro-frontends.

---

## 🟡 Backend: Risk & Prospec Service (Python)
- **Status:** Apenas Dockerfile criado.
- **Concluído:**
  - `Dockerfile` (FastAPI).
- **Próximos Passos (To-Do):**
  - Iniciar projeto base do FastAPI.
  - Criar consumidor base do Kafka (`aiokafka`).
