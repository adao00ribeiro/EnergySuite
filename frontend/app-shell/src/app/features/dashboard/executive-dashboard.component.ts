import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { NgxEchartsModule } from 'ngx-echarts';
import type { EChartsOption } from 'echarts';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MlopsService, PriceForecastPoint, RiskMetricsSummary } from '../../core/services/mlops.service';
import { RiskSignalrService, RiskCalculatedEvent } from '../../core/services/risk-signalr.service';
import { token } from '../../core/theme-token';

@Component({
  selector: 'app-executive-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTableModule,
    MatTabsModule,
    NgxEchartsModule
  ],
  templateUrl: './executive-dashboard.component.html',
  styleUrl: './executive-dashboard.component.scss'
})
export class ExecutiveDashboardComponent implements OnInit {
  public forecastData: PriceForecastPoint[] = [];
  public riskSummary!: RiskMetricsSummary;
  public realTimeEvents: RiskCalculatedEvent[] = [];
  public selectedSubmarket: 'pldSE' | 'pldS' | 'pldNE' | 'pldN' = 'pldSE';

  public chartOptions: EChartsOption = {};

  private destroyRef = inject(DestroyRef);
  private mlopsService = inject(MlopsService);
  private riskSignalrService = inject(RiskSignalrService);

  ngOnInit(): void {
    this.mlopsService.getPriceForecasts().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => {
      this.forecastData = data;
      this.updateChartOptions();
    });

    this.mlopsService.getRiskSummary().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => {
      this.riskSummary = data;
    });

    this.riskSignalrService.startConnection();
    this.riskSignalrService.riskCalculated$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(event => {
      this.realTimeEvents.unshift(event);
      if (this.realTimeEvents.length > 10) {
        this.realTimeEvents.pop();
      }
      if (this.riskSummary) {
        this.riskSummary.totalExposure += event.financialExposure;
        this.riskSummary.lastUpdated = new Date().toLocaleTimeString('pt-BR');
      }
    });
  }

  public setSubmarket(market: 'pldSE' | 'pldS' | 'pldNE' | 'pldN'): void {
    this.selectedSubmarket = market;
    this.updateChartOptions();
  }

  private updateChartOptions(): void {
    if (!this.forecastData || this.forecastData.length === 0) return;

    const dates = this.forecastData.map(d => d.date);
    const values = this.forecastData.map(d => d[this.selectedSubmarket]);

    this.chartOptions = {
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' },
        backgroundColor: token('--color-card'),
        borderColor: 'rgba(255, 255, 255, 0.15)',
        textStyle: { color: token('--color-card-foreground') }
      },
      grid: {
        left: '2%',
        right: '2%',
        bottom: '5%',
        top: '15%',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        data: dates,
        axisLine: { lineStyle: { color: token('--chart-slate') } },
        axisLabel: { color: token('--chart-tick') }
      },
      yAxis: {
        type: 'value',
        splitLine: { lineStyle: { color: token('--chart-grid') } },
        axisLabel: {
          color: token('--chart-tick'),
          formatter: 'R$ {value}'
        }
      },
      series: [
        {
          name: 'PLD (R$/MWh)',
          type: 'bar',
          data: values,
          barMaxWidth: 40,
          itemStyle: {
            borderRadius: [6, 6, 0, 0],
            color: {
              type: 'linear',
              x: 0, y: 0, x2: 0, y2: 1,
              colorStops: [
                { offset: 0, color: token('--color-info') },
                { offset: 1, color: token('--chart-navy') }
              ]
            }
          },
          label: {
            show: true,
            position: 'top',
            color: token('--color-info'),
            formatter: 'R$ {@value}'
          },
          animationEasing: 'elasticOut',
          animationDelay: (idx) => idx * 50
        }
      ]
    };
  }
}
