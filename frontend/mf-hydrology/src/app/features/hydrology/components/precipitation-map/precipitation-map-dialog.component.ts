import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NgxEchartsModule } from 'ngx-echarts';
import { EChartsOption } from 'echarts';

@Component({
  selector: 'app-precipitation-map-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, NgxEchartsModule],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>{{ data.model }} - {{ data.date }} - {{ data.dayLabel }}</h2>
      <button mat-icon-button mat-dialog-close>
        <mat-icon>close</mat-icon>
      </button>
    </div>
    <mat-dialog-content class="dialog-content">
      <div echarts [options]="chartOption()" class="full-map-chart"></div>
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
      padding: 0 16px;
      border-bottom: 1px solid var(--color-border);
    }
    .dialog-content {
      padding: 16px;
      height: 600px;
      display: flex;
      flex-direction: column;
    }
    .full-map-chart {
      flex: 1;
      width: 100%;
    }
    .color-scale {
      margin-top: 16px;
    }
    .scale-bar {
      height: 12px;
      width: 100%;
      background: linear-gradient(to right, #ffffff, #dcfce7, #86efac, #22c55e, #16a34a, #eab308, #f59e0b, #ef4444, #b91c1c, #d946ef, #701a75, #64748B);
    }
    .scale-labels {
      display: flex;
      justify-content: space-between;
      font-size: 10px;
      margin-top: 4px;
      color: var(--color-muted-foreground);
    }
  `]
})
export class PrecipitationMapDialogComponent implements OnInit {
  chartOption = signal<EChartsOption>({});

  constructor(
    public dialogRef: MatDialogRef<PrecipitationMapDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { model: string; date: string; dayLabel: string; points: any[] }
  ) {}

  ngOnInit() {
    this.updateChart();
  }

  updateChart() {
    const option: EChartsOption = {
      visualMap: { show: false }, // Using custom CSS scale bar instead
      tooltip: {
        trigger: 'item',
        formatter: (params: any) => {
          return `Lon: ${params.value[0]}<br/>Lat: ${params.value[1]}<br/>Precip: ${params.value[2]} mm`;
        }
      },
      xAxis: { type: 'value', scale: true, splitLine: { show: false }, axisLabel: { show: false } },
      yAxis: { type: 'value', scale: true, splitLine: { show: false }, axisLabel: { show: false } },
      series: [
        {
          type: 'scatter',
          symbolSize: (val: any) => val[2] === 0 ? 0 : 8,
          data: this.data.points,
          itemStyle: {
            color: (params: any) => {
              const v = params.value[2];
              if (v < 5) return '#86efac';
              if (v < 15) return '#22c55e';
              if (v < 30) return '#eab308';
              if (v < 75) return '#ef4444';
              return '#d946ef';
            }
          }
        }
      ]
    };
    this.chartOption.set(option);
  }
}
