import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { NgxEchartsModule } from 'ngx-echarts';
import { EChartsOption } from 'echarts';

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
    FormsModule,
    NgxEchartsModule
  ],
  templateUrl: './ena-analytics.html',
  styleUrls: ['./ena-analytics.scss']
})
export class EnaAnalyticsComponent implements OnInit {
  chartOption = signal<EChartsOption>({});

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

  ngOnInit() {
    this.initChart();
  }

  initChart() {
    this.chartOption.set({
      tooltip: {
        trigger: 'axis'
      },
      legend: {
        data: ['Oficial (Sólida)', 'Projeção (Tracejada)'],
        bottom: 0,
        textStyle: { color: '#64748B' }
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
        data: ['DC202601-sem1', 'DC202601-sem2', 'DC202601-sem3', 'DC202601-sem4', 'DC202601-sem5', 'DC202601-sem6'],
        axisLine: { lineStyle: { color: '#CBD5E1' } },
        axisLabel: { color: '#64748B' }
      },
      yAxis: {
        type: 'value',
        min: 0,
        max: 300,
        splitLine: { lineStyle: { color: '#F1F5F9', type: 'solid' } },
        axisLabel: { color: '#64748B' }
      },
      series: [
        {
          name: 'Oficial (Sólida)',
          type: 'line',
          data: [150, 100, 150, 180, 200, 150, 50, 100],
          itemStyle: { color: '#0369A1' },
          lineStyle: { width: 3 }
        },
        {
          name: 'Projeção (Tracejada)',
          type: 'line',
          data: [100, 150, 200, 150, 150, 200, 200, 150],
          itemStyle: { color: '#0F172A' },
          lineStyle: { width: 3, type: 'dashed' }
        }
      ]
    });
  }
}
