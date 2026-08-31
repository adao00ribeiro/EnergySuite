# Sprints & Tasks — Plataforma BackOffice de Operações de Energia (BackOps)

Este documento detalha o roteiro de implementação para o produto **Energy BackOffice** na plataforma EnergySuite, dividindo as especificações do sistema de Transaction Lifecycle Management em Sprints, e designando as tarefas para os nossos agentes especialistas (`@backend_dotnet` e `@frontend_angular`).

---

## Sprint 1: Core de Cadastros, Dashboard e Configuração
**Objetivo:** Estabelecer a fundação do BackOps, criando a base de usuários, grupos econômicos e contrapartes, além de configurar a interface principal e portfólios.

### Tarefas
- [x] Atualizar Sidebar no Angular (app-shell) para acomodar os novos menus do BackOps (Cadastro Comercial, Operações, Contratos, Financeiro, CCEE, Aprovações). *(Agente: frontend_angular)*
- [x] Implementar entidades de domínio e banco de dados para **Cadastro Comercial** (`Company`, `Person`, `EconomicGroup`, `Contact`, `Address`). *(Agente: backend_dotnet)*
- [x] Desenvolver as APIs CRUD para o Cadastro Comercial. *(Agente: backend_dotnet)*
- [x] Criar entidade e APIs para **Portfólios** (`Portfolio`). *(Agente: backend_dotnet)*
- [x] Desenvolver as telas de Cadastro Comercial e Portfólios no frontend (dentro do `mf-operations`). *(Agente: frontend_angular)*
- [x] Desenvolver o **Dashboard Operacional** com KPIs provisórios (boletas pendentes, operações inativas). *(Agentes: backend_dotnet, frontend_angular)*

---

## Sprint 2: Motor de Operações, Boletas e Workflow de Aprovação
**Objetivo:** Implementar o núcleo do sistema transacional, permitindo a criação de boletas, operações de compra e venda, e o ciclo de vida (rascunho -> oficial).

### Tarefas
- [x] Implementar as entidades `Ticket` (Boleta) e `Operation` (Compra, Venda), conectando-as a partes e contrapartes. *(Agente: backend_dotnet)*
- [x] Implementar a **Máquina de Estados (State Machine)** da operação (Rascunho, Validação, Aguardando Aprovação, Aprovada, Publicada, Oficial). *(Agente: backend_dotnet)*
- [x] Desenvolver a entidade e lógica de log de auditoria `AuditLog` para rastrear qualquer mudança em operações publicadas. *(Agente: backend_dotnet)*
- [x] Implementar motor de regras para aprovação (Rápida, Restrita, via Aprovação). *(Agente: backend_dotnet)*
- [x] Desenvolver telas de listagem e criação de Boletas e Operações no frontend. *(Agente: frontend_angular)*
- [x] Desenvolver tela da **Central de Aprovação** para usuários com perfil gerencial. *(Agente: frontend_angular)*

---

## Sprint 3: Gestão de Contratos e Operações Avançadas
**Objetivo:** Permitir versionamento de contratos, lógica avançada de reajustes, além de operações intercompany e SWAP.

### Tarefas
- [x] Refatorar a entidade existente de `Contract` para o novo modelo completo (Vigência, Limites de Volume, Indexadores). *(Agente: backend_dotnet)*
- [x] Implementar entidades para `ContractAmendment` (Aditivos), `PriceIndex` e histórico de versionamento. *(Agente: backend_dotnet)*
- [x] Criar o Motor de **Reajustes** automáticos baseado em indexadores (IPCA, IGP-M). *(Agente: backend_dotnet)*
- [x] Implementar a inteligência de **SWAP** (criação automática e sincronizada de Compra/Venda inter-submercado) e **Intercompany** (geração de operação espelho). *(Agente: backend_dotnet)*
- [x] Integrar armazenamento MinIO para upload/download de documentos (anexos) em operações e contratos. *(Agente: backend_dotnet)*
- [x] Desenvolver interface visual para Vínculos (Links) de Operações e visualização de Aditivos no frontend. *(Agente: frontend_angular)*

---

## Sprint 4: BackOffice Financeiro
**Objetivo:** Conectar a operação transacional ao ciclo de faturamento e liquidação.

### Tarefas
- [x] Implementar entidades para `AccountPayable`, `AccountReceivable` e `Billing` (Faturamento). *(Agente: backend_dotnet)*
- [x] Desenvolver a inteligência de cálculo de faturamento (Volume × Preço + Reajustes + Impostos). *(Agente: backend_dotnet)*
- [x] Implementar a funcionalidade de **Encontro de Contas** (`AccountOffset`) buscando contrapartes com saldo devedor e credor no mesmo mês. *(Agente: backend_dotnet)*
- [x] Desenvolver o grid de Acompanhamento Financeiro e Liberação para Faturamento no frontend. *(Agente: frontend_angular)*

---

## Sprint 5: Integração CCEE
**Objetivo:** Fazer o registro, exportação e conciliação automática com a Câmara de Comercialização de Energia Elétrica (CCEE).

### Tarefas
- [x] Criar serviços de exportação XML para a CCEE (CCEAL Simplificado, Firme Mensal, Firme Período). *(Agente: backend_dotnet)*
- [x] Desenvolver o processador de arquivos de retorno (CSV) da CliqCCEE. *(Agente: backend_dotnet)*
- [x] Criar o Motor de Comparação (`CCEComparison`) cruzando dados BackOps × CCEE (Status: OK, Ajustado, Pendente). *(Agente: backend_dotnet)*
- [x] Desenvolver rotinas geradoras de XML de ajuste de volume e modulação. *(Agente: backend_dotnet)*
- [x] Desenvolver interface para visualização do Comparador CCEE no frontend. *(Agente: frontend_angular)*

---

## Sprint 6: APIs B2B e Sincronização Externa (BBCE, N5X)
**Objetivo:** Conectar o BackOps a plataformas de negociação externas e viabilizar automação programática por parte dos clientes.

### Tarefas
- [x] Desenvolver camada `Integration` com sincronização automática e periódica de boletas originadas na BBCE e N5X. *(Agente: backend_dotnet)*
- [x] Criar API Gateway endpoints REST (`POST /operations`, `POST /operations/{id}/publish`) com autenticação M2M. *(Agente: backend_dotnet)*
- [x] Implementar sistema de envio de **Webhooks** informando os clientes sobre status de suas operações e contas a pagar/receber. *(Agente: backend_dotnet)*
