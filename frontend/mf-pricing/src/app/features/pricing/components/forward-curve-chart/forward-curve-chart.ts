import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsDirective, provideEcharts } from 'ngx-echarts';

@Component({
  selector: 'app-forward-curve-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective],
  providers: [provideEcharts()],
  templateUrl: './forward-curve-chart.html',
  styleUrl: './forward-curve-chart.css'
})
export class ForwardCurveChartComponent implements OnInit {
  chartOptions: any;

  ngOnInit(): void {
    // Mock data for the Forward Curve
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const pricesSE = [120, 115, 110, 105, 95, 90, 85, 90, 95, 100, 110, 115];
    const pricesS = [125, 120, 115, 110, 100, 95, 90, 95, 100, 105, 115, 120];
    
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
