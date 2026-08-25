# Diretrizes para o Agente: Desenvolvimento Backend (.NET C#)

Você está atuando no módulo ETRM (Energy Trading and Risk Management) da Suite for Energy.
Este é o coração financeiro do sistema. Siga estas diretrizes estritamente:

## 1. Arquitetura (Clean Architecture)
O código DEVE ser dividido em 4 camadas:
- **Domain:** Entidades de negócio (ex: `Contract`, `Counterparty`), Enums, Value Objects e Interfaces de Repositório. Proibido referenciar bibliotecas de infraestrutura aqui.
- **Application:** Casos de uso. Obrigatório o uso do **MediatR** (CQRS). Os DTOs, Commands e Queries residem aqui. Validações devem usar `FluentValidation`.
- **Infrastructure:** Implementação do acesso a dados (Entity Framework Core com PostgreSQL). Configurações de mensageria (Kafka Producer/Consumer).
- **API (Presentation):** Controllers (ou Minimal APIs) enxutos que apenas disparam Commands/Queries para o MediatR e retornam HTTP 200/400.

## 2. Padrões de Injeção de Dependência (IoC) e Program.cs Enxuto
- O `Program.cs` deve permanecer **limpo e focado no pipeline HTTP**. 
- Todo registro de serviços (`builder.Services.Add...`) deve ser extraído para **Métodos de Extensão (Extension Methods)** (ex: `AddVersioning()`, `AddSwagger()`, `RegisterServices()`, `AddRateLimiter()`).
- Agrupe esses métodos de extensão em pastas específicas como `Extensions` ou `IoC`.

## 3. Configurações de API e Serialização
- **Versionamento de API:** Obrigatório o uso do pacote `Asp.Versioning`. Defina a versão padrão (ex: `1.0`), assuma a versão padrão quando não especificada e substitua a versão na URL.
- **Respostas de Erro:** Utilize o padrão do `ProblemDetails` globalmente (`builder.Services.AddApiProblemDetails()`).
- **Serialização JSON:** Configure o `JsonSerializerOptions` para:
  - Usar CamelCase (`JsonNamingPolicy.CamelCase`).
  - Ignorar valores nulos (`JsonIgnoreCondition.WhenWritingNull`).
  - Ignorar ciclos de referência (`ReferenceHandler.IgnoreCycles`).
  - Converter Enums para String (`JsonStringEnumConverter`).

## 4. Segurança, CORS e Rate Limiting
- **Segurança de Cabeçalhos:** Sempre aplique middlewares de segurança customizados para injetar cabeçalhos importantes como `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`, e `Content-Security-Policy`.
- **CORS:** Configure políticas estritas definindo origens específicas (`WithOrigins`). Evite `AllowAnyOrigin` em produção.
- **Rate Limiting:** Implemente limitação de taxa (ex: `FixedWindowRateLimiter`) usando a identidade do usuário ou o IP (`HttpContext.Connection.RemoteIpAddress`), com mensagens de `429 Too Many Requests` personalizadas.

## 5. Mapeamento de Entidades (EF Core Fluent API)
- **NUNCA use Data Annotations** (como `[Table]`, `[Column]`, `[Required]`) nas entidades de domínio. O domínio deve ser agnóstico de infraestrutura.
- Todos os mapeamentos devem ser feitos através do **Fluent API** implementando `IEntityTypeConfiguration<T>` na camada de *Infrastructure* (ex: `ContractMap`).
- Especifique explicitamente o nome das tabelas e colunas (padrão snake_case ou o estabelecido pelo banco de dados) usando `ToTable()` e `HasColumnName()`.
- Defina tipos de dados complexos explicitamente, como decimais (`HasColumnType("decimal(18,2)")`).
- Utilize `HasDefaultValueSql("CURRENT_TIMESTAMP")` para campos de data automáticos como `created_at` e `updated_at`.
- Ao definir relacionamentos, seja explícito sobre as chaves estrangeiras (`HasForeignKey`), nomes de restrição (`HasConstraintName`) e comportamento de deleção (`OnDelete(DeleteBehavior.Cascade)` ou `Restrict`).

## 6. Migrações de Banco de Dados (EF Core)
- Crie um **MigrationManager** como método de extensão para o `IHost` ou `WebApplication` (ex: `MigrateDatabase<TContext>`). 
- Isso permite rodar as migrações automaticamente no startup da aplicação, garantindo que o banco está sincronizado antes do app aceitar requisições. Capture e logue qualquer falha de migração estruturadamente.

## 7. Injeção de Dependências Avançada (IoC)
- Centralize a injeção em um arquivo como `NativeInjectorConfig.cs` usando `RegisterServices`.
- **PostgreSQL avançado:** Ao registrar o `DbContext` com Npgsql, utilize o `NpgsqlDataSourceBuilder` se precisar habilitar funcionalidades específicas do Postgres.
- O registro de rotinas de mensageria (Kafka via MassTransit) e casos de uso (MediatR) também deve ficar isolado nesta camada, garantindo que o `Program.cs` permaneça limpo.

## 8. CQRS (Command Query Responsibility Segregation)
- Nunca faça lógica de negócio complexa direto no Controller.
- **Commands:** Alteram o estado. (`CreateContractCommand`, `UpdateContractPriceCommand`).
- **Queries:** Lêm o estado. (`GetContractByIdQuery`). Devem usar `AsNoTracking()` no EF Core para performance.

## 9. Orientação a Eventos (Kafka)
- Sempre que um `Command` for executado com sucesso e alterar o estado de forma relevante para outros módulos, dispare um evento de integração (ex: `ContractCreatedIntegrationEvent`).
- Use bibliotecas sólidas como o `Confluent.Kafka` ou `MassTransit`.

## 10. Observabilidade (OpenTelemetry)
- Nunca capture exceções silenciosamente (`catch(Exception e) { // nada }`).
- O log deve ser estruturado (ex: `_logger.LogInformation("Contrato {ContractId} criado", contract.Id)` em vez de concatenação de strings).
