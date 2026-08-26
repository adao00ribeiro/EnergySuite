import { Routes } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/native-federation';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/app-layout/app-layout.component').then(m => m.AppLayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/executive-dashboard.component').then(m => m.ExecutiveDashboardComponent)
      },
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
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];
