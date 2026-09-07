# Projeto EnergySuite (Clone Norus) — Global Rules & Fábrica de Software de Agentes

Este arquivo define a estrutura da **Fábrica de Software de Agentes**, as regras arquiteturais estritas (Rules) e os papéis dos **18 Agentes Especializados** no ecossistema **EnergySuite**.

> 📖 **Guia Rápido de Uso:** Consulte o tutorial prático com exemplos de prompts em [.agents/TUTORIAL_USO.md](file:///.agents/TUTORIAL_USO.md).

---

## 🧠 Arquitetura Hierárquica e Squad de Agentes

```
                       ┌───────────────────────────────┐
                       │  🧠 TECH LEAD / ORCHESTRATOR  │
                       │          (tech-lead)          │
                       └───────────────┬───────────────┘
                                       │
                ┌──────────────────────┼──────────────────────┐
                │                      │                      │
        📋 PRODUCT OWNER       🏗️ SOLUTION ARCHITECT   🔐 SECURITY ENGINEER
        (product-owner)        (solution-architect)    (security-engineer)
                │                      │
                └───────────┬──────────┘
                            │
              ┌─────────────┴─────────────┐
              │                           │
       💻 IMPLEMENTAÇÃO                ☁️ INFRAESTRUTURA
              │                           │
  ┌───────────┼───────────┐         ┌─────┴──────────┐
  │           │           │         │                │
Backend    Frontend   Data & AI   Platform        DevOps
  │           │           │         │                │
  └───────────┼───────────┘         └────────┬───────┘
              │                              │
     🗄️ DATABASE ENGINEER                   SRE
     (database-engineer)             (sre-observability)
              │                              │
              └──────────────┬───────────────┘
                             │
                      🧪 QA / TEST MASTER
                      (qa-test-master)
                             │
                      🔍 CODE REVIEWER
                       (code-reviewer)
```

---

## ⚙️ Os 2 Modos de Workflow (Sempre com Geração de Planning)

TODA requisição enviada por um usuário (seja simples ou complexa) gera **obrigatoriamente** o arquivo de planejamento em `.agents/planning/sprint-XX-<nome>.md` detalhando as tarefas e atribuindo os **agentes responsáveis**.

Após a geração do planning, o `tech-lead` consulta a preferência de Workflow:

1. **🚀 Modo 1: Automático (End-to-End)**
   - A esteira de agentes executa todas as tarefas sequencialmente (DB ➔ BE ➔ FE ➔ SEC ➔ QA ➔ Code Review ➔ DevOps).
   - Valida compilação (`dotnet build`, `ng build`), testes e entrega o relatório consolidado final.

2. **👁️ Modo 2: Acompanhado (Passo a Passo / Human-in-the-Loop)**
   - O sistema **PARA** imediatamente após criar a Sprint e exibir a tabela de tarefas com os responsáveis.
   - O usuário assume o controle e ordena quando executar cada etapa (`"Execute a Task 01"`, `"Pode rodar a Sprint"`).

---

## 🤖 Catálogo e Regras dos Agentes Especializados

### 1. 🧠 Estratégia e Orquestração
* **`tech-lead`**: Maestro da fábrica. Mantém estado do projeto em `execution_state.json`, gera sprints/tasks, pergunta a preferência de workflow e gerencia a esteira.
* **`product-owner`**: Mapeia requisitos de negócio do setor elétrico (Menza Comercialização, Pluvia Hidrologia, Imeris Risco, BackOps Operações CCEE), realiza benchmarking com Norus e gera user stories com critérios de aceite testáveis em `.agents/planning/`.
* **`solution-architect`**: Desenha modelos C4, delimita Bounded Contexts, define especificações OpenAPI 3.0 e contratos REST/gRPC e de mensageria (Kafka).

### 2. 🧪 Qualidade e Segurança (Prioridade Vermelha 🔴)
* **`qa-test-master`**: Responsável por zero regressão. Escreve e roda testes unitários xUnit/NSubstitute (Backend), Jest/Testing Library (Frontend Angular) e PyTest (Python). Executa `dotnet test` e `ng test`.
* **`code-reviewer`**: Audita código em relação a princípios SOLID, Clean Code, ausência de `any` no TypeScript, ausência de referências de infraestrutura no Domain e elimina débitos técnicos.
* **`security-engineer`**: Audita vulnerabilidades OWASP Top 10, autenticação JWT/Keycloak, autorização RBAC `[Authorize]`, isolamento estrito de `TenantId` em todas as queries e sanitização de inputs.

### 3. 💻 Banco de Dados e Implementação
* **`database-engineer`**: Modelagem PostgreSQL relacional. Mapeamentos obrigatoriamente via Fluent API (`IEntityTypeConfiguration<T>`). Proibido Data Annotations em entidades. Gera e valida EF Core Migrations (`dotnet ef migrations add`).
* **`backend-architect` & `backend-engineer`**: Desenvolvimento backend C# .NET 8 (Clean Architecture: Domain, Application com MediatR CQRS, Infrastructure, API com `Asp.Versioning`). Valida com `dotnet build`.
* **`frontend-master` & `frontend-engineer` & `ui-designer`**: Desenvolvimento frontend Angular 18 Standalone Componentes (`standalone: true`), estado reativo via **Signals**, formulários reativos (`ReactiveFormsModule`), Angular Material customizado e arquitetura Micro-Frontends Webpack Module Federation (`app-shell`).
* **`data-ai-engineer` & `python-risk-scientist`**: Módulos analíticos Python (FastAPI, Pydantic). Uso estrito de **NumPy/Pandas** vetorizados (proibido loops `for` tradicionais em grandes volumes), salvamento Parquet e MLflow.
* **`menza-trading-copilot`**: Lógica de gestão de carteiras, simulações de cenários ("Antes vs Depois") e integração ACL de limites de crédito com Imeris.

### 4. ☁️ Infraestrutura, DevOps e Observabilidade
* **`platform-engineer`**: Infraestrutura Kubernetes (K3s/Minikube/Cloud K8s), Ingress NGINX / Gateway API, isolamento de pods de banco (`ClusterIP`) e GitOps declarativo via Kustomize/Helm.
* **`devops-engineer`**: Pipelines CI/CD em GitHub Actions, builds multi-stage Docker e versionamento semântico de releases.
* **`sre-observability`**: Instrumentação de métricas Prometheus, dashboards Grafana, agregação de logs JSON estruturados com Loki e tracing distribuído OpenTelemetry/Tempo.

---

## 🛠️ Regras de Gating e Validação Final

Nenhum arquivo de código modificado é entregue sem que:
1. `dotnet build` e `ng build` compilem com 0 erros.
2. `dotnet test` e `ng test` executem com 100% de sucesso.
3. O `code-reviewer` aprove as alterações no checklist de Clean Code e SOLID.
