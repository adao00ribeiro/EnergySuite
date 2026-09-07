# 📋 Sprint 17 — Ajuste Global de Tema Escuro e Correção de Sobreposição de Labels

> **Status:** COMPLETED 🟢  
> **Modo de Execução:** AUTOMATIC (End-to-End)  
> **Data:** 2026-09-07  
> **Agente Responsável Principais:** `frontend-master`, `qa-test-master`, `code-reviewer`, `security-engineer`, `devops-engineer`

---

## 🎯 Objetivos da Sprint

1. Ajustar e padronizar o tema escuro (`body:not(.light-theme)`) em todos os micro-frontends do **EnergySuite** (`app-shell`, `mf-operations`, `mf-portfolio`, `mf-hydrology`, `mf-pricing`), eliminando texto escuro/opaco em fundo escuro.
2. **Correção do Bug de Sobreposição de Letras**: Corrigir a colisão/sobreposição visual entre os rótulos do Angular Material (`<mat-label>`) e o atributo `placeholder="..."` que ocorria nos campos não focados e vazios.

---

## 📋 Tabela de Tarefas da Sprint

| ID | Disciplina | Agente Atribuído | Descrição | Status |
| :--- | :--- | :--- | :--- | :---: |
| **TASK-17-01** | Frontend | 🎨 `frontend-master` | Sobrescrever variáveis CSS e MDC form fields com texto claro `#f8fafc` e tom legível `#94a3b8` nos 5 micro-frontends. | **DONE** |
| **TASK-17-02** | QA & Acessibilidade | 🧪 `qa-test-master` | Identificar e corrigir o bug de sobreposição de `mat-label` x `placeholder` aplicando `opacity: 0` no estado inativo e `opacity: 1` apenas no estado focado (`.mat-focused`). | **DONE** |
| **TASK-17-03** | Segurança | 🔐 `security-engineer` | Garantir sanitização de entradas em formulários e proteção de opacidade em elementos de segurança. | **DONE** |
| **TASK-17-04** | Code Review | 🔍 `code-reviewer` | Refatorar seletores CSS/SCSS garantindo transição suave de opacidade nos placeholders (`transition: opacity 150ms ease`). | **DONE** |
| **TASK-17-05** | DevOps & Build | ☁️ `devops-engineer` | Compilar os pacotes de produção/desenvolvimento de todos os 5 projetos (`app-shell`, `mf-operations`, `mf-portfolio`, `mf-hydrology`, `mf-pricing`) garantindo 0 erros. | **DONE** |

---

## 🚀 Resultados da Execução

1. **Correção da Sobreposição de Rótulos (Sem Letras Misturadas)**:
   - **Campo Vazio / Inativo**: O `placeholder` fica oculto (`opacity: 0 !important`), exibindo exclusivamente o `<mat-label>` ("CNPJ*", "Razão Social*", "Nome Fantasia*") de forma limpa e centralizada.
   - **Campo Focado**: Ao clicar no campo, o `<mat-label>` desliza suavemente para o entalhe superior da borda, e o `placeholder` ("00.000.000/0000-00", "Razão social completa") surge em `#94a3b8` (`opacity: 1 !important`).

2. **Validação de Build (0 Erros)**:
   - `app-shell`: **SUCCESS** (Exit 0)
   - `mf-operations`: **SUCCESS** (Exit 0)
   - `mf-portfolio`: **SUCCESS** (Exit 0)
   - `mf-hydrology`: **SUCCESS** (Exit 0)
   - `mf-pricing`: **SUCCESS** (Exit 0)
