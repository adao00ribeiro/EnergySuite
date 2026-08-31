import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NgxEchartsModule } from 'ngx-echarts';
import { EChartsOption } from 'echarts';
import { environment } from '../../../../../environments/environment';
import { token } from '../../../../core/theme-token';

export interface EnaResult {
  targetDate: string;
  valueMwMed: number;
  valuePercentageMlt: number;
}

@Component({
  selector: 'app-ena-analytics',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTabsModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
    MatIconModule,
    MatProgressSpinnerModule,
    FormsModule,
    NgxEchartsModule
  ],
  templateUrl: './ena-analytics.html',
  styleUrl: './ena-analytics.scss'
})
export class EnaAnalyticsComponent implements OnInit {
  private http = inject(HttpClient);

  chartOption = signal<EChartsOption>({});
  points = signal<EnaResult[]>([]);
  isLoading = signal(false);
  hasError = signal(false);

  // Form State
  tableName = signal('Gráfico Exemplo');
  selectedResult = signal('oficial');
  selectedInfoBase = signal('Ambas');
  selectedPrevsDeck = signal('Combinado');
  selectedCalcBase = signal('Semanal/Mensal');
  selectedMeasure = signal('%MLT');
  groupBy = signal('Submercados');
  tempAnalysis = signal('D-0');
  diffMode = signal(false);

  private readonly submarkets = ['SE/CO', 'S', 'NE', 'N'];

  ngOnInit() {
    this.loadEnaData();
  }

  loadEnaData() {
    this.isLoading.set(true);
    this.hasError.set(false);

    this.http.get<EnaResult[]>(`${environment.apiUrl}/pluvia/ena`).subscribe({
      next: (data) => {
        const results: EnaResult[] = Array.isArray(data) ? data : [];
        this.points.set(results);
        this.isLoading.set(false);
        this.buildChart(results);
      },
      error: (err) => {
        console.error('Erro ao carregar resultados de ENA:', err);
        this.points.set([]);
        this.isLoading.set(false);
        this.hasError.set(true);
      }
    });
  }

  buildChart(results: EnaResult[]) {
    if (results.length === 0) {
      this.chartOption.set({});
      return;
    }

    const labels = results.map(r => new Date(r.targetDate).toISOString().slice(0, 7));
    const measure = this.selectedMeasure() === '%MLT';
    const values = results.map(r => measure ? Number(r.valuePercentageMlt) : Number(r.valueMwMed));

    this.chartOption.set({
      tooltip: {
        trigger: 'axis'
      },
      legend: {
        data: ['ENA'],
        bottom: 0,
        textStyle: { color: token('--chart-tick') }
      },
      grid: {
        left: '5%',
        right: '5%',
        bottom: '15%',
        top: '5%',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        data: labels,
        axisLine: { lineStyle: { color: token('--chart-grid') } },
        axisLabel: { color: token('--chart-tick') }
      },
      yAxis: {
        type: 'value',
        min: 0,
        splitLine: { lineStyle: { color: token('--chart-grid'), type: 'solid' } },
        axisLabel: { color: token('--chart-tick') }
      },
      series: [
        {
          name: 'ENA',
          type: 'line',
          data: values,
          itemStyle: { color: token('--color-accent') },
          lineStyle: { width: 3 },
          areaStyle: {
            color: {
              type: 'linear', x: 0, y: 0, x2: 0, y2: 1,
              colorStops: [
                { offset: 0, color: 'rgba(3, 105, 161, 0.25)' },
                { offset: 1, color: 'rgba(3, 105, 161, 0)' }
              ]
            }
          }
        }
      ]
    });
  }
}