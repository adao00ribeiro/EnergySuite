# Especificação de Desenvolvimento: Módulo de Portfólio (Clone Menza)

## 1. Visão do Produto
O módulo de **Portfólio** será responsável pela consolidação e visualização da posição física e financeira da comercializadora ou consumidor livre. Ele consolida contratos de compra, venda, geração e consumo para apurar o balanço energético mensal.

## 2. Casos de Uso Principais (Features)
- **Balanço Energético (Posição):** Visualização mês a mês da energia comprada vs. vendida.
- **Gestão de Submercados:** Apuração da posição por submercado (SE/CO, S, NE, N) considerando perdas.
- **Marcação a Mercado (MtM):** Cálculo do valor do portfólio comparando o preço do contrato com o PLD/Preço Forward atual.
- **Simulação de Cenários:** "O que acontece com minha margem se o PLD subir 20%?"

## 3. Arquitetura Frontend (Angular 18)

### Rotas
- `/portfolio`: Dashboard geral com a posição consolidada.
- `/portfolio/books`: Gestão de "Books" de negociação (agrupamento de contratos).
- `/portfolio/mtm`: Relatórios de Marcação a Mercado.

### Componentes Chave
- `PortfolioDashboardComponent`: Layout principal com cards de resumo (Posição Líquida, Exposição, MtM).
- `EnergyBalanceChartComponent`: Gráfico de barras empilhadas (Highcharts ou ECharts) mostrando Compras (verde), Vendas (vermelho) e Sobra/Déficit (linha).
- `BookSelectorComponent`: Dropdown no header para filtrar dados por book de energia.

### Gerenciamento de Estado (NgRx / Signals)
- Armazenar o `activeBookId`.
- Cache da curva de preços (Preço Forward) para evitar requisições constantes.

## 4. Integração Backend (.NET 8 - ETRM Service)
- **Endpoint:** `GET /api/v1/portfolios/{id}/balance` (Retorna a soma agregada dos contratos por mês).
- **Endpoint:** `GET /api/v1/portfolios/{id}/mtm` (Retorna o valor presente líquido da carteira).

## 5. Modelos de Dados (Entidades)
```csharp
public class Portfolio {
    public Guid Id { get; set; }
    public string Name { get; set; } // ex: "Book Especulação"
    public ICollection<Contract> Contracts { get; set; }
}

public class MonthlyBalance {
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalBuyMwm { get; set; }
    public decimal TotalSellMwm { get; set; }
    public decimal NetPositionMwm => TotalBuyMwm - TotalSellMwm;
}
```

## 6. Plano de Execução
1. Criar entidade `Portfolio` no backend e relacionar com `Contract`.
2. Expor endpoint de consolidação `GET /balance`.
3. Criar rota `/portfolio` no Angular.
4. Integrar biblioteca de gráficos (ex: Apache ECharts) para renderizar a posição mensal.
