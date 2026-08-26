import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';

@Component({
  selector: 'app-reservoir-levels-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective],
  providers: [provideEchartsCore({ echarts: () => import('echarts') })],
  templateUrl: './reservoir-levels-chart.html',
  styleUrl: './reservoir-levels-chart.css'
})
export class ReservoirLevelsChartComponent implements OnInit {
  chartOptions: any;

  ngOnInit(): void {
    // ENA - Energia Natural Afluente vs % EAR
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const historicoENA = [85, 90, 95, 80, 60, 50, 45, 40, 55, 65, 75, 80];
    const projecaoENA = [null, null, null, null, null, null, null, null, 55, 62, 70, 78];

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
          data: historicoENA,
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
          data: projecaoENA,
          smooth: true,
          symbol: 'circle',
          symbolSize: 6,
          lineStyle: { width: 3, color: '#10b981', type: 'dashed' },
          itemStyle: { color: '#10b981' }
        }
      ],
      animationDuration: 2000,
      animationEasing: 'cubicOut'
    };
  }
}
