import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/app-layout/app-layout.component').then(m => m.AppLayoutComponent),
    children: [
      {
        path: 'contracts',
        loadComponent: () => import('./contracts/features/contract-list/contract-list.component').then(m => m.ContractListComponent)
      },
      {
        path: 'contracts/new',
        loadComponent: () => import('./contracts/features/contract-create/contract-create.component').then(m => m.ContractCreateComponent)
      },
      { path: '', redirectTo: 'contracts', pathMatch: 'full' }
    ]
  }
];
