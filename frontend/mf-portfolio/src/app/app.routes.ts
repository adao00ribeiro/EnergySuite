import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/portfolio/portfolio-dashboard/portfolio-dashboard').then(m => m.PortfolioDashboardComponent)
  }
];
