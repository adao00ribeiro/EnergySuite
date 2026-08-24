# Diretrizes para o Agente: Desenvolvimento Frontend (Angular 18)

Você está atuando no Portal Unificado da Suite for Energy.
O foco aqui é prover uma experiência Enterprise para o usuário final, com extrema modularidade.

## 1. Standalone Components
- O projeto usa Angular 18+. O uso de `NgModules` está estritamente **PROIBIDO**. 
- Todo componente, diretiva ou pipe criado deve ser `standalone: true`.

## 2. Gerenciamento de Estado e Reatividade
- Use **Signals** para reatividade local em componentes no lugar do antigo `RxJS BehaviorSubject` sempre que possível.
- Use a função `inject()` no construtor para injeção de dependência, visando código mais limpo e testes fáceis.

## 3. Design System (Angular Material)
- O padrão visual deve utilizar Angular Material (`@angular/material`).
- Tabelas de dados (`mat-table`) com paginação (`mat-paginator`) e ordenação (`mat-sort`) são mandatórias para listagens de contratos ou séries temporais.
- Utilize formulários reativos (`ReactiveFormsModule`) fortemente tipados. NUNCA use Template-Driven forms.

## 4. Micro-frontends (Webpack Module Federation)
- O `app-shell` é a casca. Ele carrega a navbar e a barra lateral.
- Os módulos de negócio (`mf-backops`, `mf-imeris`) devem exportar seus componentes de rota remotamente no `webpack.config.js`.
- Ao criar links de navegação entre módulos, garanta que a navegação não dê *refresh* na tela (use o router do Angular).

## 5. Visualização de Dados (Gráficos)
- Para dashboards analíticos complexos (ex: Curva de PLD), utilize ECharts (`ngx-echarts`) ou Highcharts (`highcharts-angular`).
