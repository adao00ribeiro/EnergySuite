import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReservoirLevelsChartComponent } from '../components/reservoir-levels-chart/reservoir-levels-chart';
import { MlopsStatusComponent } from '../components/mlops-status/mlops-status';

@Component({
  selector: 'app-hydrology-dashboard',
  standalone: true,
  imports: [CommonModule, ReservoirLevelsChartComponent, MlopsStatusComponent],
  templateUrl: './hydrology-dashboard.html',
  styleUrl: './hydrology-dashboard.css'
})
export class HydrologyDashboardComponent {
  currentDate = new Date();
}
