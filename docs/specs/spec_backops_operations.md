# Especificação de Desenvolvimento: Módulo de Backoffice (Clone BackOps)

## 1. Visão do Produto
O módulo de **Operações e Backoffice** centraliza a gestão do ciclo de vida dos contratos após a negociação. É onde a energia se transforma em dinheiro. Cuida de validação, alocação CCEE, faturamento e integração com ERP financeiro.

## 2. Casos de Uso Principais (Features)
- **Gestão de Medição:** Coleta de dados de consumo/geração real.
- **Alocação de Energia:** Definição de quanto de um contrato será alocado em cada contraparte na CCEE.
- **Faturamento:** Geração de espelhos de faturamento, cálculo de impostos (PIS/COFINS, ICMS) e emissão de notas.
- **Controle de Inadimplência:** Acompanhamento de liquidação financeira.

## 3. Arquitetura Frontend (Angular 18)

### Rotas
- `/operations/allocations`: Tela para realizar a alocação (sazonalização e modulação) de contratos.
- `/operations/billing`: Painel de faturamento (Espelhos, NFs pendentes).
- `/operations/measurements`: Upload e gestão de dados de medição (XML CCEE).

### Componentes Chave
- `AllocationGridComponent`: Uma tabela editável (Ag-Grid ou Mat-Table) onde o operador digita os montantes mensais alocados.
- `BillingPipelineComponent`: Um Kanban visual (Pendente, Faturado, Pago).
- `XmlUploaderComponent`: Componente drag-and-drop para arquivos `.xml` da CCEE.

## 4. Integração Backend (.NET 8 - ETRM Service)
- **Sistema de Filas (Kafka/RabbitMQ):** Quando uma NF é gerada, disparar evento `InvoiceGeneratedEvent` para o sistema financeiro (Protheus/SAP).
- **Endpoint:** `POST /api/v1/operations/allocations` (Salva a alocação mensal).
- **Background Job (Hangfire/Quartz):** Robô que verifica atrasos no pagamento e atualiza status.

## 5. Modelos de Dados (Entidades)
```csharp
public class Allocation {
    public Guid ContractId { get; set; }
    public DateTime CompetenceMonth { get; set; }
    public decimal AllocatedMw { get; set; }
    public string Status { get; set; } // Pending, Validated, SentToCCEE
}

public class Invoice {
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal TaxesAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
}
```

## 6. Plano de Execução
1. Construir tabelas de `Invoice` e `Allocation`.
2. Desenvolver a tela de faturamento com fluxo de status (Angular).
3. Criar rotina de integração assíncrona (Event Bus) para mensageria de notas fiscais.
