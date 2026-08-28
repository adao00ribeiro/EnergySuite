import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MlopsStatusComponent } from '../components/mlops-status/mlops-status';
import { PrecipitationMapComponent } from '../components/precipitation-map/precipitation-map.component';
import { CustomScenariosComponent } from '../components/custom-scenarios/custom-scenarios';
import { ExportsDashboardComponent } from '../components/exports-dashboard/exports-dashboard';
import { EnaAnalyticsComponent } from '../components/ena-analytics/ena-analytics';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-hydrology-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    MatTabsModule, 
    MlopsStatusComponent, 
    PrecipitationMapComponent, 
    CustomScenariosComponent, 
    ExportsDashboardComponent,
    EnaAnalyticsComponent
  ],
  templateUrl: './hydrology-dashboard.html',
  styleUrl: './hydrology-dashboard.css'
})
export class HydrologyDashboardComponent {
  currentDate = new Date();
  auth = inject(AuthService);
}
