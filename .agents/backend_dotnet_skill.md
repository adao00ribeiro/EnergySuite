# Diretrizes para o Agente: Desenvolvimento Backend (.NET C#)

Você está atuando no módulo ETRM (Energy Trading and Risk Management) da Suite for Energy.
Este é o coração financeiro do sistema. Siga estas diretrizes estritamente:

## 1. Arquitetura (Clean Architecture)
O código DEVE ser dividido em 4 camadas:
- **Domain:** Entidades de negócio (ex: `Contract`, `Counterparty`), Enums, Value Objects e Interfaces de Repositório. Proibido referenciar bibliotecas de infraestrutura aqui.
- **Application:** Casos de uso. Obrigatório o uso do **MediatR** (CQRS). Os DTOs, Commands e Queries residem aqui. Validações devem usar `FluentValidation`.
- **Infrastructure:** Implementação do acesso a dados (Entity Framework Core com PostgreSQL). Configurações de mensageria (Kafka Producer/Consumer).
- **API (Presentation):** Controllers (ou Minimal APIs) enxutos que apenas disparam Commands/Queries para o MediatR e retornam HTTP 200/400.

## 2. CQRS (Command Query Responsibility Segregation)
- Nunca faça lógica de negócio complexa direto no Controller.
- **Commands:** Alteram o estado. (`CreateContractCommand`, `UpdateContractPriceCommand`).
- **Queries:** Lêm o estado. (`GetContractByIdQuery`). Devem usar `AsNoTracking()` no EF Core para performance.

## 3. Orientação a Eventos (Kafka)
- Sempre que um `Command` for executado com sucesso e alterar o estado de forma relevante para outros módulos, dispare um evento de integração (ex: `ContractCreatedIntegrationEvent`).
- Use bibliotecas sólidas como o `Confluent.Kafka` ou `MassTransit`.

## 4. Observabilidade (OpenTelemetry)
- Nunca capture exceções silenciosamente (`catch(Exception e) { // nada }`).
- O log deve ser estruturado (ex: `_logger.LogInformation("Contrato {ContractId} criado", contract.Id)` em vez de concatenação de strings).
