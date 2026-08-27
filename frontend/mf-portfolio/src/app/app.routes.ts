import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./core/layout/layout.component').then(m => m.LayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'strategies', loadComponent: () => import('./features/strategies/strategies.component').then(m => m.StrategiesComponent) },
      { path: 'opportunities', loadComponent: () => import('./features/opportunities/opportunities-book.component').then(m => m.OpportunitiesBookComponent) }
    ]
  }
];
