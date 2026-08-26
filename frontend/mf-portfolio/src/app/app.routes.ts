import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/portfolio/portfolio-dashboard/portfolio-dashboard.ts').then(m => m.PortfolioDashboardComponent)
  }
];
