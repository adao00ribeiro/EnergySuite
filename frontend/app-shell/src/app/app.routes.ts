import { Routes } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/native-federation';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/portal/portal-home/portal-home').then(m => m.PortalHomeComponent),
    pathMatch: 'full'
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell-layout/shell-layout').then(m => m.ShellLayoutComponent),
    children: [
      {
        path: 'portfolio',
        loadChildren: () => loadRemoteModule('mf-portfolio', './Routes').then(m => m.routes)
      },
      {
        path: 'operations',
        loadChildren: () => loadRemoteModule('mf-operations', './Routes').then(m => m.routes)
      },
      {
        path: 'pricing',
        loadChildren: () => loadRemoteModule('mf-pricing', './Routes').then(m => m.routes)
      },
      {
        path: 'hydrology',
        loadChildren: () => loadRemoteModule('mf-hydrology', './Routes').then(m => m.routes)
      },
      // Módulos de Gestão Transversais
      {
        path: 'alerts',
        loadComponent: () => import('./features/alerts/alerts-dashboard.component').then(m => m.AlertsDashboardComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings-dashboard.component').then(m => m.SettingsDashboardComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/user-management.component').then(m => m.UserManagementComponent)
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
