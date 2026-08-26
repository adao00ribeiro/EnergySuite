import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsDirective, provideEcharts } from 'ngx-echarts';

@Component({
  selector: 'app-energy-balance-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective],
  providers: [provideEcharts()],
  templateUrl: './energy-balance-chart.html',
  styleUrl: './energy-balance-chart.scss'
})
export class EnergyBalanceChartComponent implements OnInit {
  chartOptions: any;

  ngOnInit(): void {
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const resources = [200, 210, 190, 195, 205, 220, 230, 215, 210, 200, 205, 210]; // Geração + Compras
    const requirements = [180, 190, 200, 210, 195, 185, 180, 190, 205, 215, 220, 195]; // Consumo + Vendas

    this.chartOptions = {
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' },
        backgroundColor: 'rgba(30, 41, 59, 0.9)',
        borderColor: 'rgba(255,255,255,0.1)',
        textStyle: { color: '#f8fafc' }
      },
      legend: {
        data: ['Recursos (Ativos)', 'Requisitos (Passivos)'],
        textStyle: { color: '#94a3b8' },
        top: 0
      },
      grid: {
        left: '3%', right: '4%', bottom: '3%', containLabel: true
      },
      xAxis: {
        type: 'category',
        data: months,
        axisLine: { lineStyle: { color: '#334155' } },
        axisLabel: { color: '#94a3b8' }
      },
      yAxis: {
        type: 'value',
        axisLine: { show: false },
        axisLabel: { color: '#94a3b8', formatter: '{value} MWm' },
        splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)' } }
      },
      series: [
        {
          name: 'Recursos (Ativos)',
          type: 'bar',
          data: resources,
          itemStyle: { 
            color: '#0ea5e9',
            borderRadius: [4, 4, 0, 0]
          }
        },
        {
          name: 'Requisitos (Passivos)',
          type: 'bar',
          data: requirements,
          itemStyle: { 
            color: '#f43f5e',
            borderRadius: [4, 4, 0, 0]
          }
        }
      ],
      animationDuration: 2000,
      animationEasing: 'cubicOut'
    };
  }
}
