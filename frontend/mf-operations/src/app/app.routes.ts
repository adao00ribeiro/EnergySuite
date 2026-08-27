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
  },
  {
    path: 'tickets',
    loadComponent: () => import('./features/operations/tickets-list/tickets-list').then(m => m.TicketsListComponent)
  },
  {
    path: 'approvals',
    loadComponent: () => import('./features/operations/approval-center/approval-center').then(m => m.ApprovalCenterComponent)
  },
  {
    path: 'contracts/:id',
    loadComponent: () => import('./features/operations/contract-details/contract-details').then(m => m.ContractDetailsComponent)
  },
  {
    path: 'finance',
    loadComponent: () => import('./features/finance/financial-dashboard/financial-dashboard').then(m => m.FinancialDashboardComponent)
  },
  {
    path: 'ccee',
    loadComponent: () => import('./features/ccee/ccee-dashboard/ccee-dashboard').then(m => m.CceeDashboardComponent)
  }
];
