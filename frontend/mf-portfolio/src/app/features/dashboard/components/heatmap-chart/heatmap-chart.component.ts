import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsModule, provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts';
import type { EChartsOption } from 'echarts';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-heatmap-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsModule, MatCardModule],
  providers: [
    provideEchartsCore({ echarts })
  ],
  template: `
    <mat-card class="chart-card">
      <mat-card-header>
        <mat-card-title>Heatmap de Gaps (Submercado x Mês)</mat-card-title>
        <mat-card-subtitle>Vermelho indica necessidade de compra (Déficit). Azul indica sobra (Excedente).</mat-card-subtitle>
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
      margin-top: 16px;
    }
    .chart-container {
      height: 450px;
      width: 100%;
    }
  `]
})
export class HeatmapChartComponent implements OnChanges {
  @Input() heatmapData: any;
  
  chartOptions: EChartsOption = {};

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['heatmapData'] && this.heatmapData) {
      this.updateChart();
    }
  }

  updateChart() {
    if (!this.heatmapData?.xAxisMonths || !this.heatmapData?.yAxisSubmarkets) return;

    // Convert Points object [{XIndex, YIndex, GapValue}] to Echarts Heatmap array [XIndex, YIndex, GapValue]
    const dataPoints = this.heatmapData.points.map((p: any) => [p.xIndex, p.yIndex, p.gapValue]);

    this.chartOptions = {
      tooltip: {
        position: 'top'
      },
      grid: {
        height: '60%',
        top: '10%'
      },
      xAxis: {
        type: 'category',
        data: this.heatmapData.xAxisMonths,
        splitArea: {
          show: true
        }
      },
      yAxis: {
        type: 'category',
        data: this.heatmapData.yAxisSubmarkets,
        splitArea: {
          show: true
        }
      },
      visualMap: {
        min: -30,
        max: 30,
        calculable: true,
        orient: 'horizontal',
        left: 'center',
        bottom: '5%',
        inRange: {
          color: ['#ef4444', '#f8fafc', '#3b82f6'] // Red (Deficit), White/Gray (Neutral), Blue (Surplus)
        }
      },
      series: [
        {
          name: 'Gap (MWm)',
          type: 'heatmap',
          data: dataPoints,
          label: {
            show: true
          },
          emphasis: {
            itemStyle: {
              shadowBlur: 10,
              shadowColor: 'rgba(0, 0, 0, 0.5)'
            }
          }
        }
      ]
    };
  }
}
