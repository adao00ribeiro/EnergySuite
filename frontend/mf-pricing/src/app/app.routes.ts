import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/pricing/pricing-dashboard/pricing-dashboard').then(m => m.PricingDashboardComponent)
  }
];
