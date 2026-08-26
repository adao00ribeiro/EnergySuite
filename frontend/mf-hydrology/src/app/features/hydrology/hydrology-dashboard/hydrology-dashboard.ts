import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { ReservoirLevelsChartComponent } from '../components/reservoir-levels-chart/reservoir-levels-chart';
import { MlopsStatusComponent } from '../components/mlops-status/mlops-status';
import { PrecipitationMapComponent } from '../components/precipitation-map/precipitation-map.component';
import { CustomScenariosComponent } from '../components/custom-scenarios/custom-scenarios';

@Component({
  selector: 'app-hydrology-dashboard',
  standalone: true,
  imports: [CommonModule, MatTabsModule, ReservoirLevelsChartComponent, MlopsStatusComponent, PrecipitationMapComponent, CustomScenariosComponent],
  templateUrl: './hydrology-dashboard.html',
  styleUrl: './hydrology-dashboard.css'
})
export class HydrologyDashboardComponent {
  currentDate = new Date();
}
