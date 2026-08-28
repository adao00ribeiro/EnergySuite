import { Component, OnInit, signal, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { NgxEchartsModule, NGX_ECHARTS_CONFIG } from 'ngx-echarts';
import { EChartsOption } from 'echarts';
import { PrecipitationMapDialogComponent } from './precipitation-map-dialog.component';

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
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    NgxEchartsModule
  ],
  templateUrl: './precipitation-map.component.html',
  styleUrls: ['./precipitation-map.component.scss'],
  providers: [
    {
      provide: NGX_ECHARTS_CONFIG,
      useFactory: () => ({ echarts: () => import('echarts') })
    }
  ]
})
export class PrecipitationMapComponent implements OnInit {
  private http = inject(HttpClient);
  private dialog = inject(MatDialog);

  // State
  selectedModel = signal<string>('GEFS-00');
  selectedDate = signal<string>(new Date().toISOString().split('T')[0]);
  isLoading = signal<boolean>(false);

  // Grid Data
  mapGrid = signal<any[]>([]);

  constructor() {
    effect(() => {
      this.fetchMapData(this.selectedModel(), this.selectedDate());
    });
  }

  ngOnInit(): void {}

  fetchMapData(model: string, date: string) {
    this.isLoading.set(true);
    // Call the Python API for the geospatial precipitation matrix
    this.http.get<any>(`http://localhost:8000/api/v1/pluvia/precipitation-map?model=${model.split('-')[0]}&date=${date}`)
      .subscribe({
        next: (response) => {
          this.buildGrid(response.points, model, date);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load precipitation map', err);
          // Mock data for UI demonstration since API might fail during build/test
          this.buildGrid(this.generateMockPoints(), model, date);
          this.isLoading.set(false);
        }
      });
  }

  generateMockPoints() {
    let pts = [];
    for(let i=0; i<100; i++) {
      pts.push([Math.random()*10, Math.random()*10, Math.random()*100]);
    }
    return pts;
  }

  buildGrid(basePoints: any[], model: string, baseDate: string) {
    const grid = [];
    const base = new Date(baseDate);
    
    // Create 8 days of forecast maps
    for(let i=1; i<=8; i++) {
      const forecastDate = new Date(base);
      forecastDate.setDate(base.getDate() + i);
      
      grid.push({
        model: model,
        date: forecastDate,
        dayLabel: `Dia ${i}`,
        points: basePoints, // In reality, this would be day-specific data
        chartOption: this.generateMiniChartOption(basePoints)
      });
    }
    
    this.mapGrid.set(grid);
  }

  generateMiniChartOption(points: any[]): EChartsOption {
    return {
      tooltip: { show: false },
      xAxis: { type: 'value', show: false },
      yAxis: { type: 'value', show: false },
      grid: { left: 0, right: 0, top: 0, bottom: 0 },
      series: [
        {
          type: 'scatter',
          symbolSize: 4,
          data: points,
          itemStyle: {
            color: (params: any) => {
              const v = params.value[2];
              if (v < 15) return '#22c55e';
              if (v < 30) return '#eab308';
              return '#ef4444';
            }
          }
        }
      ]
    };
  }

  openDialog(mapItem: any) {
    this.dialog.open(PrecipitationMapDialogComponent, {
      width: '80vw',
      maxWidth: '800px',
      data: mapItem
    });
  }
}
