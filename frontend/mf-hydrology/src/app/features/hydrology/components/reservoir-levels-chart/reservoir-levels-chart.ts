import { Component, OnInit, signal, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';
import { HttpClient } from '@angular/common/http';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-reservoir-levels-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective, MatButtonToggleModule, FormsModule],
  providers: [provideEchartsCore({ echarts: () => import('echarts') })],
  templateUrl: './reservoir-levels-chart.html',
  styleUrl: './reservoir-levels-chart.css'
})
export class ReservoirLevelsChartComponent implements OnInit {
  private http = inject(HttpClient);
  
  chartOptions = signal<any>({});
  selectedOffset = signal<number>(0);

  constructor() {
    effect(() => {
      this.loadEnaData(this.selectedOffset());
    });
  }

  ngOnInit(): void {
    // Initial load will be triggered by effect when signal is initialized
  }

  loadEnaData(offsetDays: number) {
    this.http.get<any[]>(`/api/v1/pluvia/ena?offsetDays=${offsetDays}`).subscribe({
      next: (data) => {
        const months = data.map(d => {
          const date = new Date(d.targetDate);
          return date.toLocaleDateString('pt-BR', { month: 'short' });
        });
        
        const projecaoMLT = data.map(d => d.valuePercentageMlt);
        
        // Simular um histórico ligeiramente alterado com base no MLT para visualização
        const historicoMLT = projecaoMLT.map(v => v * 0.9 + (Math.random() * 5));

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
            data: ['ENA Histórico', 'ENA Projeção (ML)'],
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
            axisLabel: { color: '#94a3b8', formatter: '{value}%' },
            splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)' } }
          },
          series: [
            {
              name: 'ENA Histórico',
              type: 'line',
              data: historicoMLT,
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
              name: 'ENA Projeção (ML)',
              type: 'line',
              data: projecaoMLT,
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
      error: (err) => console.error("Failed to load ENA data", err)
    });
  }
}
