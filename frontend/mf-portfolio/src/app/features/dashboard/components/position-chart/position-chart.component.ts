import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsModule, provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts';
import type { EChartsOption } from 'echarts';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-position-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsModule, MatCardModule],
  providers: [
    provideEchartsCore({ echarts })
  ],
  template: `
    <mat-card class="chart-card">
      <mat-card-header>
        <mat-card-title>Posição Mensal</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <div echarts [options]="chartOptions" class="chart-container"></div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .chart-card {
      border-radius: 12px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
      padding-top: 1rem;
    }
    .chart-container {
      height: 400px;
      width: 100%;
    }
  `]
})
export class PositionChartComponent implements OnChanges {
  @Input() data: any[] = [];
  
  chartOptions: EChartsOption = {};

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.updateChart();
    }
  }

  updateChart() {
    const months = this.data.map(d => d.month);
    const purchased = this.data.map(d => d.purchased);
    const sold = this.data.map(d => d.sold);
    const net = this.data.map(d => d.net);

    this.chartOptions = {
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' }
      },
      legend: {
        data: ['Comprado', 'Vendido', 'Posição Líquida']
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        data: months
      },
      yAxis: {
        type: 'value',
        name: 'MWmédio'
      },
      series: [
        {
          name: 'Comprado',
          type: 'bar',
          stack: 'total',
          itemStyle: { color: '#3b82f6' },
          data: purchased
        },
        {
          name: 'Vendido',
          type: 'bar',
          stack: 'total',
          itemStyle: { color: '#f97316' },
          data: sold.map(v => -v)
        },
        {
          name: 'Posição Líquida',
          type: 'line',
          itemStyle: { color: '#8b5cf6' },
          lineStyle: { width: 3 },
          data: net
        }
      ]
    };
  }
}
