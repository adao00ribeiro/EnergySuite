import { Component, signal, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';
import { HttpClient } from '@angular/common/http';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../../../environments/environment';

interface EnaPoint {
  targetDate: string;
  valueMwMed: number;
  valuePercentageMlt: number;
}

@Component({
  selector: 'app-reservoir-levels-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective, MatButtonToggleModule, MatIconModule, FormsModule],
  providers: [provideEchartsCore({ echarts: () => import('echarts') })],
  templateUrl: './reservoir-levels-chart.html',
  styleUrl: './reservoir-levels-chart.css'
})
export class ReservoirLevelsChartComponent {
  private http = inject(HttpClient);

  chartOptions = signal<any>({});
  selectedOffset = signal<number>(0);
  hasError = signal(false);
  isEmpty = signal(false);
  isLoading = signal(false);

  constructor() {
    effect(() => {
      this.loadEnaData(this.selectedOffset());
    });
  }

  loadEnaData(offsetDays: number) {
    this.isLoading.set(true);
    this.hasError.set(false);

    this.http.get<EnaPoint[]>(`${environment.apiUrl}/pluvia/ena?offsetDays=${offsetDays}`).subscribe({
      next: (data) => {
        const points: EnaPoint[] = Array.isArray(data) ? data : [];
        this.isLoading.set(false);

        if (points.length === 0) {
          this.isEmpty.set(true);
          this.chartOptions.set({});
          return;
        }
        this.isEmpty.set(false);

        const months = points.map(d => {
          const date = new Date(d.targetDate);
          return date.toLocaleDateString('pt-BR', { month: 'short', year: '2-digit' });
        });

        const mwmed = points.map(d => Number(d.valueMwMed));
        const mlt = points.map(d => Number(d.valuePercentageMlt));

        this.chartOptions.set({
          backgroundColor: 'transparent',
          tooltip: {
            trigger: 'axis',
            axisPointer: { type: 'cross' },
            backgroundColor: 'rgba(30, 41, 59, 0.9)',
            borderColor: 'rgba(255,255,255,0.1)',
            textStyle: { color: '#f8fafc' }
          },
          legend: {
            data: ['ENA (MWmed)', 'ENA (%MLT)'],
            textStyle: { color: '#94a3b8' },
            top: 0
          },
          grid: {
            left: '3%', right: '4%', bottom: '3%', containLabel: true
          },
          xAxis: {
            type: 'category',
            boundaryGap: false,
            data: months,
            axisLine: { lineStyle: { color: '#334155' } },
            axisLabel: { color: '#94a3b8' }
          },
          yAxis: [
            {
              type: 'value',
              name: 'MWmed',
              axisLine: { show: false },
              axisLabel: { color: '#94a3b8' },
              splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)' } }
            },
            {
              type: 'value',
              name: '%MLT',
              axisLine: { show: false },
              axisLabel: { color: '#94a3b8', formatter: '{value}%' },
              splitLine: { show: false }
            }
          ],
          series: [
            {
              name: 'ENA (MWmed)',
              type: 'line',
              yAxisIndex: 0,
              data: mwmed,
              smooth: true,
              symbol: 'circle',
              symbolSize: 6,
              lineStyle: { width: 3, color: '#0ea5e9' },
              itemStyle: { color: '#0ea5e9' },
              areaStyle: {
                color: {
                  type: 'linear', x: 0, y: 0, x2: 0, y2: 1,
                  colorStops: [{ offset: 0, color: 'rgba(14, 165, 233, 0.4)' }, { offset: 1, color: 'rgba(14, 165, 233, 0)' }]
                }
              }
            },
            {
              name: 'ENA (%MLT)',
              type: 'line',
              yAxisIndex: 1,
              data: mlt,
              smooth: true,
              symbol: 'circle',
              symbolSize: 6,
              lineStyle: { width: 3, color: '#10b981', type: 'dashed' },
              itemStyle: { color: '#10b981' }
            }
          ],
          animationDuration: 1000,
          animationEasing: 'cubicOut'
        });
      },
      error: (err) => {
        console.error('Failed to load ENA data', err);
        this.isLoading.set(false);
        this.hasError.set(true);
        this.chartOptions.set({});
      }
    });
  }
}