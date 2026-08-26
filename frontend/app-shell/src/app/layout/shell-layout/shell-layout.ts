import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-shell-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, MatSidenavModule, MatIconModule, MatTooltipModule],
  templateUrl: './shell-layout.html',
  styleUrl: './shell-layout.scss'
})
export class ShellLayoutComponent {
  private router = inject(Router);

  navItems = [
    { label: 'Portfólio', path: '/portfolio', icon: 'dashboard', color: 'text-blue' },
    { label: 'Operações', path: '/operations', icon: 'sync_alt', color: 'text-indigo' },
    { label: 'Precificação', path: '/pricing', icon: 'trending_up', color: 'text-cyan' },
    { label: 'Hidrologia', path: '/hydrology', icon: 'water_drop', color: 'text-teal' }
  ];

  goHome() {
    this.router.navigate(['/']);
  }
}
