import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/hydrology/hydrology-dashboard/hydrology-dashboard').then(m => m.HydrologyDashboardComponent)
  }
];
