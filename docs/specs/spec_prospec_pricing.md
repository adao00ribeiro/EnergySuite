# Especificação de Desenvolvimento: Módulo de Estudos de Preço (Clone Prospec)

## 1. Visão do Produto
O módulo de **Pricing & Analytics** é focado em inteligência de mercado e simulações. Ele automatiza a execução e visualização de resultados de modelos oficiais do setor elétrico (NEWAVE, DECOMP, DESSEM) para prever o PLD (Preço de Liquidação das Diferenças).

## 2. Casos de Uso Principais (Features)
- **Execução em Nuvem:** Iniciar rodadas do DECOMP/NEWAVE na nuvem (AWS/Azure) através da interface.
- **Comparação de Cenários:** Comparar resultados de preços entre a rodada "Oficial da CCEE" vs "Sensibilidade da Casa".
- **Visualização de PLD:** Gráficos interativos com trajetórias de preço esperadas (boxplot de cenários).
- **Gestão de Decks:** Upload, edição e validação dos arquivos de entrada (Decks) dos modelos.

## 3. Arquitetura Frontend (Angular 18)

### Rotas
- `/analytics/pricing/studies`: Lista de estudos/rodadas (Status: Rodando, Concluído, Erro).
- `/analytics/pricing/studies/{id}`: Dashboard de resultados de um estudo específico.
- `/analytics/pricing/decks`: Repositório de arquivos de configuração (Decks).

### Componentes Chave
- `StudyRunnerComponent`: Formulário para disparar uma nova simulação (escolher modelo, deck base, parâmetros).
- `PldTrajectoryChartComponent`: Gráfico complexo (Highcharts) mostrando séries temporais de 2000 cenários de preço (fancart/cone de incerteza).
- `ExecutionStatusBadgeComponent`: Polling via WebSocket/SignalR para atualizar o progresso do processamento em tempo real.

## 4. Integração Backend (.NET 8 + Python/FastAPI)
- **Arquitetura Assíncrona:** O .NET não roda o modelo. Ele recebe o request, cria uma mensagem num Tópico Kafka `StartStudyCommand`. Um Worker (Python) lê, baixa o deck, processa em cluster, e grava resultados no PostgreSQL/MinIO.
- **WebSocket (SignalR):** Para enviar feedback de progresso da simulação (ex: "Processando mês 5 de 60") para o Angular.

## 5. Modelos de Dados (Entidades)
```csharp
public class PricingStudy {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ModelType { get; set; } // NEWAVE, DECOMP
    public string Status { get; set; } // Queued, Running, Finished
    public DateTime CreatedAt { get; set; }
    public string DeckStoragePath { get; set; } // Path no MinIO
}
```

## 6. Plano de Execução
1. Criar microserviço/worker em Python responsável apenas por executar modelos.
2. Construir pipeline Kafka para orquestração.
3. Desenvolver a UI de visualização de resultados com SignalR para real-time.
