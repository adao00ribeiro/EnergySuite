import { Routes } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/native-federation';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/portal/portal-home/portal-home.ts').then(m => m.PortalHomeComponent),
    pathMatch: 'full'
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell-layout/shell-layout.ts').then(m => m.ShellLayoutComponent),
    children: [
      {
        path: 'portfolio',
        loadComponent: () => loadRemoteModule('mf-portfolio', './Component').then(m => m.App)
      },
      {
        path: 'operations',
        loadComponent: () => loadRemoteModule('mf-operations', './Component').then(m => m.App)
      },
      {
        path: 'pricing',
        loadComponent: () => loadRemoteModule('mf-pricing', './Component').then(m => m.App)
      },
      {
        path: 'hydrology',
        loadComponent: () => loadRemoteModule('mf-hydrology', './Component').then(m => m.App)
      },
      // Legacy monolithic routes for fallback
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/executive-dashboard.component').then(m => m.ExecutiveDashboardComponent)
      },
      {
        path: 'contracts',
        loadComponent: () => import('./contracts/features/contract-list/contract-list.component').then(m => m.ContractListComponent)
      },
      {
        path: 'contracts/new',
        loadComponent: () => import('./contracts/features/contract-create/contract-create.component').then(m => m.ContractCreateComponent)
      },
      {
        path: 'risk/counterparty',
        loadComponent: () => import('./features/risk/counterparty-risk/counterparty-risk.component').then(m => m.CounterpartyRiskComponent)
      }
    ]
  }
];
