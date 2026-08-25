import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';
import type { EChartsOption } from 'echarts';
import { RiskService, CounterpartyRisk } from '../../../core/services/risk.service';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-counterparty-risk',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective, MatCardModule, MatIconModule, MatProgressSpinnerModule],
  providers: [
    provideEchartsCore({ echarts: () => import('echarts') }),
  ],
  templateUrl: './counterparty-risk.component.html',
  styleUrls: ['./counterparty-risk.component.scss']
})
export class CounterpartyRiskComponent implements OnInit {
  private riskService = inject(RiskService);
  
  public isLoading = true;
  public chartOption: EChartsOption = {};
  public portfolioData: CounterpartyRisk[] = [];

  ngOnInit(): void {
    this.riskService.getPortfolioRisk().subscribe({
      next: (data) => {
        this.portfolioData = data;
        this.setupChart(data);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar risco do portfólio', err);
        this.isLoading = false;
      }
    });
  }

  private setupChart(data: CounterpartyRisk[]) {
    const counterparties = data.map(d => d.counterparty_name);
    const exposures = data.map(d => d.financial_exposure);
    const mtms = data.map(d => d.mark_to_market);

    this.chartOption = {
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' }
      },
      legend: {
        data: ['Exposição Financeira (R$)', 'Mark-to-Market (R$)']
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
      },
      xAxis: {
        type: 'value'
      },
      yAxis: {
        type: 'category',
        data: counterparties
      },
      series: [
        {
          name: 'Exposição Financeira (R$)',
          type: 'bar',
          data: exposures,
          itemStyle: { color: '#3f51b5' }
        },
        {
          name: 'Mark-to-Market (R$)',
          type: 'bar',
          data: mtms,
          itemStyle: {
            color: (params: any) => {
              // Verde para MtM positivo, vermelho para negativo
              return params.value >= 0 ? '#4caf50' : '#f44336';
            }
          }
        }
      ]
    };
  }
}
