import { Component, OnInit, signal, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpParams } from '@angular/common/http';
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
import * as echarts from 'echarts';
import { PrecipitationMapDialogComponent } from './precipitation-map-dialog.component';
import { environment } from '../../../../../environments/environment';
import { BRAZIL_GEOJSON } from './brazil-geo';

// Register Brazil Map
try {
  echarts.registerMap('brazil', BRAZIL_GEOJSON as any);
} catch (e) {
  console.warn('Map brazil already registered', e);
}

interface ForecastDay {
  offset: number;
  date: string;
  points: number[][];
}

interface MapResponse {
  model: string;
  date: string;
  horizon_days: number;
  points: number[][];
  days?: ForecastDay[];
}

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
  styleUrl: './precipitation-map.component.scss',
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
  error = signal<string | null>(null);

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
    this.error.set(null);

    let params = new HttpParams()
      .set('model', model.split('-')[0])
      .set('date', date);

    this.http.get<any>(
      `${environment.riskApiUrl}/pluvia/precipitation-map`,
      { params }
    ).subscribe({
      next: (response) => {
        this.buildGrid(response, model, date);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load precipitation map', err);
        this.error.set('Falha ao carregar dados do Lakehouse.');
        this.mapGrid.set([]);
        this.isLoading.set(false);
      }
    });
  }

  buildGrid(response: any, model: string, baseDate: string) {
    const base = new Date(baseDate);
    const grid: any[] = [];

    const days: ForecastDay[] = Array.isArray(response.days) && response.days.length
      ? response.days
      : Array.from({ length: response.horizon_days || 8 }, (_, i) => {
          const d = new Date(base);
          d.setDate(base.getDate() + i + 1);
          return {
            offset: i + 1,
            date: d.toISOString().split('T')[0],
            points: response.points
          };
        });

    days.forEach((day, index) => {
      const forecastDate = new Date(day.date);
      const points = day.points;

      grid.push({
        model: model,
        date: forecastDate,
        dayLabel: `Dia ${index + 1}`,
        points: points,
        chartOption: this.generateMiniChartOption(points)
      });
    });

    this.mapGrid.set(grid);
  }

  generateMiniChartOption(points: any[]): EChartsOption {
    return {
      tooltip: { show: false },
      visualMap: {
        show: false,
        min: 0,
        max: 80,
        inRange: {
          color: [
            '#1e293b00',
            '#70baff',
            '#005bc5',
            '#00b848',
            '#ffd600',
            '#ff8c00',
            '#e62222',
            '#9c27b0'
          ]
        }
      },
      geo: {
        map: 'brazil',
        roam: false,
        zoom: 1.1,
        center: [-54, -14],
        itemStyle: {
          areaColor: '#1e293b',
          borderColor: '#334155',
          borderWidth: 0.8
        }
      },
      series: [
        {
          type: 'heatmap',
          coordinateSystem: 'geo',
          data: points,
          pointSize: 10,
          blurSize: 12
        }
      ]
    };
  }

  openDialog(mapItem: any) {
    this.dialog.open(PrecipitationMapDialogComponent, {
      width: '85vw',
      maxWidth: '900px',
      data: mapItem
    });
  }
}
