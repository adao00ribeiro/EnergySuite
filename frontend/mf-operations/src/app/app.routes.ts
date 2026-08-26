import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/operations/operations-dashboard/operations-dashboard').then(m => m.OperationsDashboardComponent)
  },
  {
    path: 'commercial/companies',
    loadComponent: () => import('./features/commercial-registry/company-list/company-list').then(m => m.CompanyListComponent)
  },
  {
    path: 'portfolios',
    loadComponent: () => import('./features/portfolios/portfolio-list/portfolio-list').then(m => m.PortfolioListComponent)
  }
];
