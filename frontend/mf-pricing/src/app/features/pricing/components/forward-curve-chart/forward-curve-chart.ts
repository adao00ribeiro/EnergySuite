import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { environment } from '../../../../../environments/environment';

interface ForwardCurvePoint {
  month: string;
  pldSE: number;
  pldS: number;
  pldNE: number;
  pldN: number;
}

@Component({
  selector: 'app-forward-curve-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective, MatIconModule],
  providers: [provideEchartsCore({ echarts: () => import('echarts') })],
  templateUrl: './forward-curve-chart.html',
  styleUrl: './forward-curve-chart.css'
})
export class ForwardCurveChartComponent implements OnInit {
  chartOptions: any = null;
  hasError = signal<boolean>(false);
  isEmpty = signal<boolean>(false);
  private http = inject(HttpClient);

  ngOnInit(): void {
    this.http.get<ForwardCurvePoint[]>(`${environment.apiUrl}/pricing/forward-curve`).subscribe({
      next: (data) => {
        const points: ForwardCurvePoint[] = Array.isArray(data) ? data : [];

        if (points.length === 0) {
          this.isEmpty.set(true);
          this.chartOptions = null;
          return;
        }

        this.buildChart(points);
      },
      error: (err) => {
        this.hasError.set(true);
        this.chartOptions = null;
        console.error('Error fetching forward curve:', err);
      }
    });
  }

  private buildChart(points: ForwardCurvePoint[]) {
    const months = points.map(p => p.month);
    const pricesSE = points.map(p => p.pldSE);
    const pricesS = points.map(p => p.pldS);

    this.chartOptions = {
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'cross' },
        backgroundColor: 'rgba(30, 41, 59, 0.9)',
        borderColor: 'rgba(255,255,255,0.1)',
        textStyle: { color: '#f8fafc' }
      },
      legend: {
        data: ['Submercado SE/CO', 'Submercado S'],
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
      yAxis: {
        type: 'value',
        axisLine: { show: false },
        axisLabel: { color: '#94a3b8', formatter: 'R$ {value}' },
        splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)' } }
      },
      series: [
        {
          name: 'Submercado SE/CO',
          type: 'line',
          data: pricesSE,
          smooth: true,
          symbol: 'none',
          lineStyle: { width: 3, color: '#06b6d4', shadowColor: 'rgba(6, 182, 212, 0.5)', shadowBlur: 10 },
          areaStyle: {
            color: {
              type: 'linear', x: 0, y: 0, x2: 0, y2: 1,
              colorStops: [{ offset: 0, color: 'rgba(6, 182, 212, 0.3)' }, { offset: 1, color: 'rgba(6, 182, 212, 0)' }]
            }
          }
        },
        {
          name: 'Submercado S',
          type: 'line',
          data: pricesS,
          smooth: true,
          symbol: 'none',
          lineStyle: { width: 3, color: '#8b5cf6', shadowColor: 'rgba(139, 92, 246, 0.5)', shadowBlur: 10 },
        }
      ],
      animationDuration: 2000,
      animationEasing: 'cubicOut'
    };
  }
}