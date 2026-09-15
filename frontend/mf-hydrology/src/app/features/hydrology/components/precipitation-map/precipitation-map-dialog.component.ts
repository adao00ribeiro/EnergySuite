import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NgxEchartsModule } from 'ngx-echarts';
import { EChartsOption } from 'echarts';
import * as echarts from 'echarts';
import { BRAZIL_GEOJSON } from './brazil-geo';

// Register Brazil Map with ECharts
try {
  echarts.registerMap('brazil', BRAZIL_GEOJSON as any);
} catch (e) {
  console.warn('Map brazil already registered or registration failed', e);
}

@Component({
  selector: 'app-precipitation-map-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, NgxEchartsModule],
  template: `
    <div class="dialog-header">
      <div class="header-title-container">
        <h2 mat-dialog-title class="dialog-title">
          {{ data.model }} - {{ formattedDate }} - {{ data.dayLabel }}
        </h2>
      </div>
      <div class="header-actions">
        <button mat-icon-button class="action-btn download-btn" (click)="downloadMap()" title="Download do Mapa (PNG)">
          <mat-icon>file_download</mat-icon>
        </button>
        <button mat-icon-button mat-dialog-close class="action-btn close-btn" title="Fechar">
          <mat-icon>close</mat-icon>
        </button>
      </div>
    </div>
    <mat-dialog-content class="dialog-content">
      <div echarts [options]="chartOption()" (chartInit)="onChartInit($event)" class="full-map-chart"></div>
      <div class="color-scale">
        <div class="scale-bar"></div>
        <div class="scale-labels">
          <span>0</span><span>1</span><span>5</span><span>10</span><span>15</span><span>20</span><span>25</span><span>30</span><span>40</span><span>50</span><span>75</span><span>100</span><span>150</span><span>200</span><span>250</span><span>300</span><span>350</span><span>400</span>
        </div>
      </div>
    </mat-dialog-content>
  `,
  styles: [`
    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 20px;
      border-bottom: 1px solid var(--color-border, #1e293b);
      background: #0f172a;
    }
    .dialog-title {
      margin: 0;
      font-size: 1.15rem;
      font-weight: 600;
      color: #f8fafc;
      letter-spacing: -0.01em;
    }
    .header-actions {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .action-btn {
      color: #94a3b8;
      transition: color 0.2s ease, transform 0.2s ease;
      &:hover {
        color: #f8fafc;
        transform: scale(1.05);
      }
    }
    .dialog-content {
      padding: 16px;
      height: 620px;
      display: flex;
      flex-direction: column;
      background: #090d16;
    }
    .full-map-chart {
      flex: 1;
      width: 100%;
      height: 100%;
      border-radius: 8px;
      overflow: hidden;
    }
    .color-scale {
      margin-top: 14px;
      padding: 0 8px;
    }
    .scale-bar {
      height: 14px;
      width: 100%;
      border-radius: 3px;
      background: linear-gradient(to right, 
        #f8fafc 0%, 
        #cbe7ff 5%, 
        #70baff 12%, 
        #2a86e9 20%, 
        #005bc5 30%, 
        #00b848 42%, 
        #2fd238 55%, 
        #ffd600 70%, 
        #ff8c00 82%, 
        #e62222 92%, 
        #9c27b0 100%
      );
      box-shadow: 0 1px 4px rgba(0,0,0,0.4);
    }
    .scale-labels {
      display: flex;
      justify-content: space-between;
      font-size: 10px;
      font-weight: 500;
      margin-top: 6px;
      color: #94a3b8;
    }
  `]
})
export class PrecipitationMapDialogComponent implements OnInit {
  chartOption = signal<EChartsOption>({});
  formattedDate = '';
  private echartsInstance: any = null;

  constructor(
    public dialogRef: MatDialogRef<PrecipitationMapDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { model: string; date: any; dayLabel: string; points: any[] }
  ) {}

  ngOnInit() {
    this.formatDateString();
    this.updateChart();
  }

  onChartInit(ec: any) {
    this.echartsInstance = ec;
  }

  formatDateString() {
    if (this.data.date instanceof Date) {
      const year = this.data.date.getUTCFullYear();
      const month = String(this.data.date.getUTCMonth() + 1).padStart(2, '0');
      const day = String(this.data.date.getUTCDate()).padStart(2, '0');
      this.formattedDate = `${day}/${month}/${year}`;
    } else if (typeof this.data.date === 'string') {
      const parts = this.data.date.split('T')[0].split('-');
      if (parts.length === 3) {
        this.formattedDate = `${parts[2]}/${parts[1]}/${parts[0]}`;
      } else {
        this.formattedDate = this.data.date;
      }
    } else {
      this.formattedDate = String(this.data.date);
    }
  }

  updateChart() {
    const option: EChartsOption = {
      backgroundColor: '#090d16',
      tooltip: {
        trigger: 'item',
        backgroundColor: 'rgba(15, 23, 42, 0.95)',
        borderColor: '#334155',
        borderWidth: 1,
        padding: [10, 14],
        textStyle: { color: '#f8fafc', fontSize: 12 },
        formatter: (params: any) => {
          if (!params.value || !Array.isArray(params.value)) return '';
          const lon = params.value[0];
          const lat = params.value[1];
          const val = params.value[2];
          return `
            <div style="font-weight:600;margin-bottom:6px;color:#38bdf8;font-size:13px;">Precipitação Acumulada</div>
            <div style="display:flex;justify-content:space-between;gap:12px;margin-bottom:2px;">
              <span style="color:#94a3b8;">Longitude:</span> <b>${lon.toFixed(2)}°</b>
            </div>
            <div style="display:flex;justify-content:space-between;gap:12px;margin-bottom:4px;">
              <span style="color:#94a3b8;">Latitude:</span> <b>${lat.toFixed(2)}°</b>
            </div>
            <div style="display:flex;justify-content:space-between;gap:12px;border-top:1px solid #334155;padding-top:4px;">
              <span style="color:#94a3b8;">Volume:</span> <b style="color:#facc15;font-size:13px;">${val.toFixed(1)} mm</b>
            </div>
          `;
        }
      },
      visualMap: {
        show: false,
        min: 0,
        max: 100,
        calculable: true,
        realtime: true,
        inRange: {
          color: [
            '#f8fafc00',
            '#cbe7ff',
            '#70baff',
            '#2a86e9',
            '#005bc5',
            '#00b848',
            '#2fd238',
            '#ffd600',
            '#ff8c00',
            '#e62222',
            '#9c27b0'
          ]
        }
      },
      geo: {
        map: 'brazil',
        roam: true,
        zoom: 1.25,
        center: [-54, -14],
        itemStyle: {
          areaColor: '#1e293b',
          borderColor: '#475569',
          borderWidth: 1.2
        },
        emphasis: {
          itemStyle: {
            areaColor: '#334155'
          }
        }
      },
      series: [
        {
          name: 'Precipitação (mm)',
          type: 'heatmap',
          coordinateSystem: 'geo',
          data: this.data.points,
          pointSize: 18,
          blurSize: 22
        }
      ]
    };
    this.chartOption.set(option);
  }

  downloadMap() {
    if (this.echartsInstance) {
      const url = this.echartsInstance.getDataURL({
        type: 'png',
        pixelRatio: 2,
        backgroundColor: '#090d16'
      });
      const link = document.createElement('a');
      const cleanDate = this.formattedDate.replace(/\//g, '-');
      const cleanLabel = this.data.dayLabel.replace(/\s+/g, '');
      link.download = `${this.data.model}_${cleanDate}_${cleanLabel}.png`;
      link.href = url;
      link.click();
    }
  }
}
