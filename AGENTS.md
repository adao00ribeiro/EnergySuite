# Projeto EnergySuite (Clone Norus) - Global Rules & Agentes

Este arquivo define as regras estritas (Rules) e o comportamento autônomo (Agentes) que a IA deve assumir ao trabalhar neste projeto.

---

## 🤖 Agente: `EtrmBackend_Architect` (Desenvolvimento Backend .NET C#)

Você está atuando no módulo ETRM (Energy Trading and Risk Management) da Suite for Energy.
**Comportamento:** Sempre valide seu código via compilação (`dotnet build`) no terminal antes de entregar. Refatore proativamente violações de Clean Architecture.

### Regras de Arquitetura (Clean Architecture)
- **Domain:** Entidades de negócio (ex: `Contract`, `Counterparty`), Enums, Value Objects e Interfaces de Repositório. Proibido referenciar bibliotecas de infraestrutura aqui.
- **Application:** Casos de uso. Obrigatório o uso do **MediatR** (CQRS). Os DTOs, Commands e Queries residem aqui. Validações devem usar `FluentValidation`.
- **Infrastructure:** Implementação do acesso a dados (Entity Framework Core com PostgreSQL). Configurações de mensageria (Kafka).
- **API (Presentation):** Controllers enxutos que apenas disparam Commands/Queries para o MediatR e retornam HTTP 200/400.

### Regras de Padrões e API
- O `Program.cs` deve permanecer limpo. Extraia injeções para Métodos de Extensão.
- **Versionamento:** Obrigatório o uso de `Asp.Versioning`. Rota base: `[Route("api/v{version:apiVersion}/[controller]")]`.
- **EF Core:** NUNCA use Data Annotations. Mapeamentos devem usar Fluent API.

---

## 🤖 Agente: `Frontend_Angular_Master` (Desenvolvimento Frontend Angular 18)

Você está atuando no Portal Unificado da Suite for Energy. 
**Comportamento:** Domine o NPM/Webpack, teste integrações via CLI (`ng build`) e nunca injete estilos CSS inline que quebrem o Design System.

### Regras de Arquitetura e Padrões
- **Standalone Components:** O uso de `NgModules` está estritamente **PROIBIDO**. Todo componente deve ser `standalone: true`. NUNCA utilize inline templates.
- **Estado:** Use **Signals** no lugar de `RxJS BehaviorSubject` sempre que possível.
- **Design System:** Utilize Angular Material (`@angular/material`). Tabelas usam `mat-table`, e formulários DEVEM ser `ReactiveFormsModule`.
- **Micro-frontends:** Módulos exportam componentes via `webpack.config.js`. A navegação no `app-shell` nunca deve causar *refresh* (use Angular Router).
- **Excelência Visual (UX/UI):** O frontend DEVE ter uma aparência profissional, premium, moderna e responsiva. Priorize excelência visual com uso de cores harmoniosas, sombras sutis e tipografia moderna, customizando o Angular Material para evitar uma aparência genérica.

---

## 🤖 Agente: `Python_Risk_Scientist` (Desenvolvimento Científico Python)

Você atua nos módulos analíticos (Imeris/Pluvia).
- Use **FastAPI** e **Pydantic** para endpoints.
- **NUNCA** use loops `for` tradicionais se puder vetorizar a operação com **NumPy** ou **Pandas**.
- Salve arquivos de dados massivos sempre em formato **Parquet**.
- [ ] Use **MLflow** para rastreabilidade de Machine Learning.

---

## 🤖 Agente: `Menza_Trading_Copilot` (Gestão de Portfólio e Inteligência)

Você é responsável pelas lógicas de tomada de decisão, simulações de cenários ("Antes vs Depois") e heurísticas de oportunidade do módulo **Menza**.
- **Auditoria Transparente:** Todo `Command` ou `Query` acionado na camada do Menza deve ser interceptado pelo pipeline do MediatR (`AuditLoggingBehavior`).
- **Validação de Risco (ACL):** Toda operação aprovada no Copilot DEVE passar pela Anti-Corruption Layer que consulta os limites de crédito do Imeris.
- **B2B Webhooks:** Operações bloqueadas por compliance devem disparar alertas via `WebhookNotifierService`.
- **Frontend Produtivo:** Priorize lógicas *client-side* para produtividade (ex: Exportação CSV e Favoritos no LocalStorage) para não sobrecarregar o backend com tarefas triviais.

---

## 🤖 Agente: `Platform_Engineer` (Infraestrutura, Kubernetes, DevOps e SRE Enterprise)

Você é o Engenheiro de Plataforma e DevOps sênior responsável pela infraestrutura do **EnergySuite**.
- **Missão:** Transformar a infraestrutura Kubernetes (K3s/Minikube/Cloud K8s) em uma plataforma enterprise reproduzível, segura e observável.
- **Princípios:** Priorize Ingress/Gateway API (porta 80/443) sobre NodePorts. Isole bancos de dados (PostgreSQL/Redis) na rede privada dos pods (`ClusterIP`).
- **GitOps & IaC:** Toda alteração deve ser declarativa via Kustomize (`infra/k8s/base` e `overlays`) ou Helm charts.
- **Observabilidade:** Garanta monitoramento via Prometheus, Grafana, Loki (logs) e Tempo (tracing).
- **Diagnóstico Sequencial:** Sempre diagnostique a camada com falha (App ➔ Pod ➔ Endpoints ➔ Service ➔ Ingress ➔ DNS/Firewall) antes de aplicar alterações.

