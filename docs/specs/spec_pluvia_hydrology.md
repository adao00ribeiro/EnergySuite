# Especificação de Desenvolvimento: Módulo de Hidrologia (Clone Pluvia)

> *Status Atualizado: Produto 100% Implementado (Sprints 1 a 7).*

## 1. Visão do Produto
O módulo de **Hidrologia e Meteorologia** é o motor de dados físicos do mercado. O preço da energia no Brasil é ditado pela chuva. Este módulo coleta dados de precipitação, engatilha modelos hidrológicos robustos e projeta o cenário de Energia Natural Afluente (ENA) exportando tudo no padrão ONS/CCEE.

## 2. Casos de Uso Implementados (Features)
- **Mapas de Precipitação & Cenários:** Visualização geográfica de chuvas (Leaflet) e interface (Angular) para o analista fazer upload e *blend* (ponderação) de mapas Customizados integrados ao MinIO (Object Storage).
- **Acompanhamento de ENA e Reservatórios:** Gráficos dinâmicos interativos (ECharts) consumindo dados reais do back-end para comparar ENA esperada e evolução temporal.
- **Workflow de ML e Automação (MLOps):** Monitoramento em tempo real (painel de status) informando o progresso das simulações numéricas executadas na malha distribuída de Workers Python.
- **Integração B2B (GEVAZP):** Área de exportação setorial que fornece links S3 (Pre-signed URLs) para arquivos txt oficiais (PREVS, ENA, VNA, DADVAZ), com notificações por *Webhooks* (push) para sistemas de terceiros.

## 3. Arquitetura Frontend (Angular 18)
A aplicação está envelopada em arquitetura de Micro-frontends (MFE) com Module Federation (`mf-hydrology`).

### Componentes Chave (Standalone, Signals)
- `PrecipitationMapComponent`: Mapa interativo renderizando imagens de raster sobre polígonos das bacias.
- `ReservoirLevelsChartComponent`: Gráfico de linhas dinâmico consumindo de APIs restritas via `HttpClient`.
- `CustomScenariosComponent`: Ferramenta para upload e ponderação de precipitações usando reatividade do Angular Material.
- `MlopsStatusComponent`: Polling dashboard exibindo dados do Kafka em tempo-real.
- `ExportsDashboardComponent`: Interface material table que converte e disponibiliza relatórios textuais usando RBAC (acesso restrito por Claims).

## 4. Arquitetura Backend (.NET 8 & Python)
O sistema adota o paradigma de Event-Driven Architecture (EDA) fortemente apoiado em Mensageria e Microsserviços.

- **ETL e Agendamento (.NET Quartz):** Trabalhos recorrentes despachados pelo Quartz.NET (cron `0 0 4 * * ?`) garantem o pipeline noturno diário autônomo e sem cliques.
- **API (C#):** Utiliza CQRS (MediatR), conectada ao banco de dados PostgreSQL via Entity Framework Core. Expõe rotas públicas, controladas por *Rate Limiter* severo (`b2b`) e Validação de Token JWT (`[Authorize(Policy="EnaPolicy")]`).
- **Barramento (Kafka / MassTransit):** Tráfego central de inteligência. A API .NET injeta intenções, e o Worker processa pesadamente consumindo esses eventos (`SimulationRequestedIntegrationEvent`).
- **Worker (Python):** Engine de inteligência (`risk-service`). Escuta o Kafka, acessa o MinIO/boto3 para ler *blend* maps, roda simulações matriciais (SMAP) e despacha resultados assíncronos (`EnaCalculatedIntegrationEvent`), além de construir massas de relatórios GEVAZP/txt nativos.

## 5. Modelos de Dados Híbridos
O domínio do sistema cruza entidades físicas com status de máquina.

```csharp
// Exemplo: Armazenamento da Corrida
public class HydrologicalResult : BaseEntity {
    public Guid ExecutionId { get; set; }
    public string Submarket { get; set; }
    public double ValueMwMed { get; set; }
    public double ValuePercentageMlt { get; set; }
    public DateTime TargetDate { get; set; }
}

// Exemplo: Evento de Mensageria
public record EnaCalculatedIntegrationEvent(
    Guid ExecutionId, 
    string Submarket, 
    double ValueMwMed, 
    double ValuePercentageMlt, 
    DateTime TargetDate
);
```

## 6. Pilha Tecnológica Integrada
- **Linguagens:** TypeScript (Angular), C# 12 (.NET 8), Python 3.12.
- **Cache/Mensageria:** Kafka
- **Observabilidade:** OpenTelemetry e Prometheus (Rastreamento injetado nos Consumers).
- **Cloud/Armazenamento:** PostgreSQL (Dados Estruturados) e MinIO/S3 (Datalake de Mapas Rasterizados e Exports de Relatórios Txt).
- **Segurança B2B:** Keycloak JWT (Autenticação) + Claims Policy (Autorização).
