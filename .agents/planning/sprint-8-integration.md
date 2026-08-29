# Sprint 8: Integração Crítica (Pricing, Portfolio & Base Services)

**Objetivo:** Erradicar os mocks do Módulo de Pricing, integrar os dados vitais do Módulo Menza (Portfolio) com o backend e finalizar os serviços base críticos do ETRM (Webhooks e Sync).

## Tasks (Para o `backend-architect` e `frontend-master`)

### 1. Finalização de Serviços Base (ETRM)
- **Task BK-1: Implementar WebhookService Real**
  - **Contexto:** A notificação de eventos (ex: criação de contrato, alerta de risco) está apenas logando dados ("Mock implementation").
  - **Ação (`backend-architect`):** Modificar `WebhookService.cs` para utilizar `HttpClient` (ou uma Factory) e realizar requisições POST para as URLs de webhook configuradas. Tratar policies de retry (Polly).
- **Task BK-2: Integração no ExternalTradeSyncService**
  - **Contexto:** O serviço em background que sincroniza trades externos (ex: CCEE) está mockado.
  - **Ação (`backend-architect`):** Implementar a lógica real de fetch na API da CCEE ou sistema terceiro no método dentro de `ExternalTradeSyncService.cs`. Se o Contrato/Contraparte não existir, ele deve seguir a lógica de negócio (criar draft ou rejeitar).

### 2. Integração do Módulo Menza (Portfolio)
- **Task P-1 & P-2: Mapeamento de Oportunidades e Dashboard**
  - **Contexto:** Os grids de Oportunidades e do Dashboard no Angular usam arrays estáticos instanciados nos componentes.
  - **Ação (`frontend-master`):** Criar os services correspondentes consumindo `/api/v1/portfolio/opportunities` e injetar nos componentes `opportunities-book.component.ts` e `dashboard.component.ts`.
- **Task P-3: Endpoint SimulateOperationCommand**
  - **Contexto:** O `simulation-dialog.component.ts` simula um delay e devolve "spread de 450.000".
  - **Ação (`backend-architect`):** Criar (caso não exista) o endpoint de simulação em `.NET 8`.
  - **Ação (`frontend-master`):** Conectar o `HttpClient` do MFE do Menza a esse endpoint.

### 3. Integração do Módulo Pricing
- **Task PR-1 & PR-2: Integração Total de Prospect e Curva Forward**
  - **Contexto:** O `prospect.service.ts` e `forward-curve-chart.ts` dependem 100% de mocks.
  - **Ação (`frontend-master`):** Vincular as chamadas aos endpoints correspondentes da API de Preços, possivelmente no `mf-pricing`. Garantir tipagem via DTOs corretos e validação de tela.
