import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';
import { MatIconModule } from '@angular/material/icon';
import { PortfolioService } from '../../../../core/services/portfolio.service';

@Component({
  selector: 'app-energy-balance-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective, MatIconModule],
  providers: [provideEchartsCore({ echarts: () => import('echarts') })],
  templateUrl: './energy-balance-chart.html',
  styleUrl: './energy-balance-chart.scss'
})
export class EnergyBalanceChartComponent implements OnInit {
  private portfolioService = inject(PortfolioService);

  chartOptions: any;
  isEmpty = signal(false);
  hasError = signal(false);
  isLoading = signal(true);

  ngOnInit(): void {
    this.portfolioService.getDashboardData().subscribe({
      next: (data) => {
        this.isLoading.set(false);

        const monthly = data.monthlyData ?? [];
        if (monthly.length === 0) {
          this.isEmpty.set(true);
          return;
        }
        this.isEmpty.set(false);

        const months = monthly.map((m: any) => m.month);
        const resources = monthly.map((m: any) => m.purchased);
        const requirements = monthly.map((m: any) => m.sold);

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
            data: ['Compras (Recursos)', 'Vendas (Requisitos)'],
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
              name: 'Compras (Recursos)',
              type: 'bar',
              data: resources,
              itemStyle: {
                color: '#0ea5e9',
                borderRadius: [4, 4, 0, 0]
              }
            },
            {
              name: 'Vendas (Requisitos)',
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
      },
      error: (err) => {
        console.error('Erro ao carregar balanço de energia:', err);
        this.isLoading.set(false);
        this.hasError.set(true);
      }
    });
  }
}