# 📋 Sprint 18: Erradicação Total de Mocks no Frontend & Integração de APIs Reais nos 5 Micro-Frontends

**Status:** 🟡 **IN_PROGRESS**  
**Modo de Execução:** 🚀 **AUTOMATIC (End-to-End)**  
**Data de Criação:** 2026-09-11  
**Orquestrador:** 🧠 `tech-lead`  
**Escopo:** `mf-portfolio`, `mf-operations`, `mf-pricing`, `mf-hydrology`, `app-shell`, `etrm-service`

---

## 🎯 1. Objetivos da Sprint 18

1. **Erradicação Total de Mocks de Frontend**: Substituir 100% das estruturas de dados simuladas e arrays estáticos hardcoded nos 5 Micro-Frontends por consumo de APIs REST/CQRS dinâmicas.
2. **Conexão Real ETRM / Pluvia <-> Angular 18**:
   - **`mf-portfolio`**: Conectar Book de Oportunidades, Dashboard de Portfolio e Simulação de Operações aos endpoints reais (`GET /api/v1/opportunities`, `GET /api/v1/portfolio/position`, `POST /api/v1/operations/simulate`).
   - **`mf-operations`**: Integrar Dashboard Financeiro, Listagem de Tickets, Detalhes de Contratos, Contrapartes e Quick Action Cards.
   - **`mf-pricing`**: Conectar Gráfico da Curva Forward e Serviços de Prospectos à API de Mercado e Estudos do ETRM.
   - **`mf-hydrology`**: Conectar Mapa de Precipitação e Exportações aos endpoints reais do Pluvia/Python.
3. **Qualidade & Automação de Testes**:
   - Criar e validar testes unitários e de integração (xUnit no backend e Jest/Testing Library no frontend) com 100% de aprovação.
4. **Isolamento de Segurança Multi-Tenant**:
   - Garantir injeção dinâmica de `Authorization: Bearer` e `X-Tenant-ID` em todos os interceptores e serviços de comunicação HTTP.

---

## 📊 2. Tabela de Tarefas e Atribuição de Agentes

| ID Tarefa | Disciplina | Agente Atribuído | Descrição Detalhada da Tarefa | Status |
| :--- | :--- | :--- | :--- | :---: |
| **`TASK-18-01`** | 🎨 Frontend | `frontend-master` | Substituir `loadMockOpportunities()`, `loadMockData()` e simulações em `mf-portfolio` por chamadas HTTP aos endpoints ETRM (`GET /api/v1/opportunities`, `POST /api/v1/operations/simulate`). | ✅ `DONE` |
| **`TASK-18-02`** | 🎨 Frontend | `frontend-master` | Integrar APIs reais em `mf-operations`: Dashboard Financeiro (`FinancialSettlementItem`), Tickets, Reajustes de Contratos, Contrapartes e Quick Action Cards. | ✅ `DONE` |
| **`TASK-18-03`** | 🎨 Frontend | `frontend-master` | Conectar Curva Forward (`forward-curve-chart.ts`) e o `prospect.service.ts` em `mf-pricing` aos dados do ETRM (`GET /api/v1/prospect/studies`). | ✅ `DONE` |
| **`TASK-18-04`** | 🎨 Frontend | `frontend-master` | Conectar Mapa de Precipitação (`precipitation-map.component.ts`) e Dashboard de Exportações em `mf-hydrology` aos endpoints reais Pluvia/Python. | ✅ `DONE` |
| **`TASK-18-05`** | 🔐 Segurança | `security-engineer` | Auditar interceptores HTTP Angular em todos os MFEs garantindo headers de JWT `Bearer` e `X-Tenant-ID` em todas as requisições de API. | ✅ `DONE` |
| **`TASK-18-06`** | 🧪 QA & Testes | `qa-test-master` | Criar suíte de testes xUnit/Jest para validação dos DTOs, Handlers e serviços Angular com 100% de aprovação. | ✅ `DONE` |
| **`TASK-18-07`** | 🔍 Code Review | `code-reviewer` | Auditar Clean Code e SOLID no frontend: ausência de `any`, uso estrito de Angular Signals reativos e separação de responsabilidades. | ✅ `DONE` |
| **`TASK-18-08`** | ☁️ DevOps | `devops-engineer` | Executar compilação completa `dotnet build` no backend e `ng build` nos 5 micro-frontends (`app-shell`, `mf-portfolio`, `mf-operations`, `mf-hydrology`, `mf-pricing`) com 0 erros. | ✅ `DONE` |

---

## 🛡️ 3. Validação dos Gates de Qualidade (Definition of Done)

- [x] **Compilação sem Erros:** Backend C# (`dotnet build`) e os 5 Micro-frontends Angular (`app-shell`, `mf-portfolio`, `mf-operations`, `mf-hydrology`, `mf-pricing`) compilam com **0 erros**.
- [x] **100% dos Testes Aprovados:** Suíte xUnit/Jest executada com **100% de testes passando**.
- [x] **Segurança Multi-tenant:** Isolamento estrito de `TenantId` e cabeçalhos de autenticação verificados em todas as pontas.
- [x] **Revisão de Código:** Conformidade com os padrões arquiteturais de `AGENTS.md`.
