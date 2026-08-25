import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsModule } from 'ngx-echarts';
import type { EChartsOption } from 'echarts';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { Subscription } from 'rxjs';
import { MlopsService, PriceForecastPoint, RiskMetricsSummary } from '../../core/services/mlops.service';
import { RiskSignalrService, RiskCalculatedEvent } from '../../core/services/risk-signalr.service';

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
  styleUrls: ['./executive-dashboard.component.scss']
})
export class ExecutiveDashboardComponent implements OnInit, OnDestroy {
  public forecastData: PriceForecastPoint[] = [];
  public riskSummary!: RiskMetricsSummary;
  public realTimeEvents: RiskCalculatedEvent[] = [];
  public selectedSubmarket: 'pldSE' | 'pldS' | 'pldNE' | 'pldN' = 'pldSE';
  
  public chartOptions: EChartsOption = {};
  
  private signalrSub!: Subscription;

  constructor(
    private mlopsService: MlopsService,
    private riskSignalrService: RiskSignalrService
  ) {}

  ngOnInit(): void {
    this.mlopsService.getPriceForecasts().subscribe(data => {
      this.forecastData = data;
      this.updateChartOptions();
    });

    this.mlopsService.getRiskSummary().subscribe(data => {
      this.riskSummary = data;
    });

    this.riskSignalrService.startConnection();
    this.signalrSub = this.riskSignalrService.riskCalculated$.subscribe(event => {
      this.realTimeEvents.unshift(event);
      if (this.realTimeEvents.length > 10) {
        this.realTimeEvents.pop();
      }
      // Dynamically update total exposure
      if (this.riskSummary) {
        this.riskSummary.totalExposure += event.financialExposure;
        this.riskSummary.lastUpdated = new Date().toLocaleTimeString('pt-BR');
      }
    });
  }

  ngOnDestroy(): void {
    if (this.signalrSub) {
      this.signalrSub.unsubscribe();
    }
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
        backgroundColor: '#1e293b',
        borderColor: 'rgba(255, 255, 255, 0.15)',
        textStyle: { color: '#f8fafc' }
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
        axisLine: { lineStyle: { color: '#64748b' } },
        axisLabel: { color: '#94a3b8' }
      },
      yAxis: {
        type: 'value',
        splitLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.05)' } },
        axisLabel: {
          color: '#94a3b8',
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
                { offset: 0, color: '#38bdf8' },
                { offset: 1, color: '#1e40af' }
              ]
            }
          },
          label: {
            show: true,
            position: 'top',
            color: '#38bdf8',
            formatter: 'R$ {@value}'
          },
          animationEasing: 'elasticOut',
          animationDelay: (idx) => idx * 50
        }
      ]
    };
  }
}
