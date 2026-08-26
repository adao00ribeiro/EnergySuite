import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/operations/operations-dashboard/operations-dashboard.ts').then(m => m.OperationsDashboardComponent)
  }
];
