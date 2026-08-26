# Especificação de Desenvolvimento: Módulo de Hidrologia (Clone Pluvia)

## 1. Visão do Produto
O módulo de **Hidrologia e Meteorologia** é o motor de dados físicos do mercado. O preço da energia no Brasil é ditado pela chuva. Este módulo coleta, processa e exibe dados de precipitação, vazões e energia natural afluente (ENA).

## 2. Casos de Uso Principais (Features)
- **Mapas de Precipitação:** Visualização geográfica de chuvas observadas e previstas sobre bacias hidrográficas.
- **Acompanhamento de ENA:** Gráficos comparando ENA esperada vs ENA verificada por submercado.
- **Nível de Reservatórios (EAR):** Dashboard diário mostrando o volume útil dos principais reservatórios do SIN (Sistema Interligado Nacional).
- **Ingestão Automatizada:** Integração com ONS, CCEE, INMET para baixar dados diariamente.

## 3. Arquitetura Frontend (Angular 18)

### Rotas
- `/analytics/hydrology/precipitation`: Mapas climáticos.
- `/analytics/hydrology/reservoirs`: Acompanhamento do nível dos lagos.
- `/analytics/hydrology/ena`: Dashboards de Energia Natural Afluente.

### Componentes Chave
- `BasinMapComponent`: Mapa interativo (Leaflet ou Mapbox GL JS) renderizando polígonos das bacias e heatmaps de chuva.
- `ReservoirGaugeComponent`: Componentes visuais tipo termômetro/tanque mostrando o nível de armazenamento.
- `EnaChartComponent`: Gráfico de séries temporais históricas.

## 4. Integração Backend (.NET 8 ou Python)
- **ETL Diário:** Jobs diários que realizam web scraping ou chamam APIs públicas (ONS) para popular o banco de dados.
- **Time-Series DB:** Fortemente recomendado usar PostgreSQL com extensão TimescaleDB para consultas ultrarrápidas de séries temporais de bacias.

## 5. Modelos de Dados (Entidades)
```csharp
public class Reservoir {
    public int Code { get; set; }
    public string Name { get; set; }
    public string Submarket { get; set; }
}

public class DailyReservoirReading {
    public int ReservoirCode { get; set; }
    public DateTime Date { get; set; }
    public double UsefulVolumePercentage { get; set; }
    public double InflowM3s { get; set; } // Vazão afluente
}
```

## 6. Plano de Execução
1. Configurar Cron Jobs no backend para download automático da rotina diária do ONS.
2. Implementar Leaflet no Angular para renderização espacial.
3. Construir as telas focando em alta performance de renderização de gráficos.
