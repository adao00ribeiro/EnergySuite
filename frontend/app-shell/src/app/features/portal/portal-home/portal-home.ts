import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-portal-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './portal-home.html',
  styleUrl: './portal-home.scss'
})
export class PortalHomeComponent {
  private router = inject(Router);

  modules = [
    { name: 'Gestão de Portfólio', path: '/portfolio', color: 'blue', icon: 'M16 8v8m-4-5v5m-4-2v2m-2 4h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z' },
    { name: 'ETRM & Backops', path: '/operations', color: 'indigo', icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
    { name: 'Imeris (Risco)', path: '/pricing', color: 'cyan', icon: 'M13 10V3L4 14h7v7l9-11h-7z' },
    { name: 'Pluvia (Hidrologia)', path: '/hydrology', color: 'teal', icon: 'M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z' }
  ];

  navigateTo(path: string) {
    this.router.navigate([path]);
  }
}
