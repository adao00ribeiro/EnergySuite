import { Component, OnInit, signal, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { NgxEchartsModule, NGX_ECHARTS_CONFIG } from 'ngx-echarts';
import { EChartsOption } from 'echarts';

@Component({
  selector: 'app-precipitation-map',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    MatCardModule, 
    MatSelectModule, 
    MatFormFieldModule, 
    MatInputModule,
    NgxEchartsModule
  ],
  templateUrl: './precipitation-map.component.html',
  styleUrls: ['./precipitation-map.component.css'],
  providers: [
    {
      provide: NGX_ECHARTS_CONFIG,
      useFactory: () => ({ echarts: () => import('echarts') })
    }
  ]
})
export class PrecipitationMapComponent implements OnInit {
  private http = inject(HttpClient);

  // Angular Signals for state management
  selectedModel = signal<string>('GEFS');
  selectedDate = signal<string>(new Date().toISOString().split('T')[0]);
  
  // Loading state
  isLoading = signal<boolean>(false);

  // ECharts options
  chartOption = signal<EChartsOption>({});

  constructor() {
    // Effect: React to changes in model or date filters
    effect(() => {
      this.fetchMapData(this.selectedModel(), this.selectedDate());
    });
  }

  ngOnInit(): void {}

  fetchMapData(model: string, date: string) {
    this.isLoading.set(true);
    // Call the Python API for the geospatial precipitation matrix
    this.http.get<any>(`http://localhost:8000/api/v1/pluvia/precipitation-map?model=${model}&date=${date}`)
      .subscribe({
        next: (response) => {
          this.updateChart(response.points, model);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load precipitation map', err);
          this.isLoading.set(false);
        }
      });
  }

  updateChart(points: any[], model: string) {
    // Basic scatter plot to visualize precipitation intensity over grid
    const option: EChartsOption = {
      title: {
        text: `Precipitation Matrix (${model})`,
        left: 'center',
        textStyle: {
          color: '#e2e8f0'
        }
      },
      tooltip: {
        trigger: 'item',
        formatter: (params: any) => {
          return `Lon: ${params.value[0]}<br/>Lat: ${params.value[1]}<br/>Precip: ${params.value[2]} mm`;
        }
      },
      visualMap: {
        min: 0,
        max: 100,
        calculable: true,
        orient: 'vertical',
        right: 10,
        bottom: 20,
        textStyle: { color: '#fff' },
        inRange: {
          color: ['#313695', '#4575b4', '#74add1', '#abd9e9', '#e0f3f8', '#ffffbf', '#fee090', '#fdae61', '#f46d43', '#d73027', '#a50026']
        }
      },
      xAxis: {
        type: 'value',
        scale: true,
        name: 'Longitude',
        splitLine: { show: false },
        axisLabel: { color: '#94a3b8' }
      },
      yAxis: {
        type: 'value',
        scale: true,
        name: 'Latitude',
        splitLine: { show: false },
        axisLabel: { color: '#94a3b8' }
      },
      series: [
        {
          name: 'Precipitation',
          type: 'scatter',
          symbolSize: (val: any) => {
            return val[2] === 0 ? 0 : 10;
          },
          data: points,
          itemStyle: {
            opacity: 0.8
          }
        }
      ]
    };
    
    this.chartOption.set(option);
  }
}
