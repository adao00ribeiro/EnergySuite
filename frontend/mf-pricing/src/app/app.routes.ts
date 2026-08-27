import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/pricing/pricing-dashboard/pricing-dashboard').then(m => m.PricingDashboardComponent)
  },
  {
    path: 'prospect',
    loadComponent: () => import('./features/prospect/prospect-dashboard/prospect-dashboard').then(m => m.ProspectDashboardComponent)
  },
  {
    path: 'prospect/:id',
    loadComponent: () => import('./features/prospect/prospect-detail/prospect-detail').then(m => m.ProspectDetailComponent)
  }
];
