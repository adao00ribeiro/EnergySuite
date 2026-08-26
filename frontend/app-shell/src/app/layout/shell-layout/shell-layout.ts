import { Component, inject, OnInit } from '@angular/core';
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
export class ShellLayoutComponent implements OnInit {
  private router = inject(Router);

  // Global Contexts
  private moduleMenus: Record<string, any[]> = {
    '/portfolio': [
      { label: 'Balanço Geral', path: '/portfolio', icon: 'account_balance_wallet', color: 'text-blue' },
      { label: 'Exposição', path: '/portfolio/exposure', icon: 'pie_chart', color: 'text-blue' }
    ],
    '/operations': [
      { label: 'Dashboard', path: '/operations', icon: 'dashboard', color: 'text-indigo' },
      { label: 'Contratos', path: '/operations/contracts', icon: 'description', color: 'text-indigo' },
      { label: 'Liquidações', path: '/operations/settlements', icon: 'receipt_long', color: 'text-indigo' }
    ],
    '/pricing': [
      { label: 'Painel de Risco', path: '/pricing', icon: 'trending_up', color: 'text-cyan' },
      { label: 'Curva Forward', path: '/pricing/curves', icon: 'show_chart', color: 'text-cyan' },
      { label: 'Cenários', path: '/pricing/scenarios', icon: 'science', color: 'text-cyan' }
    ],
    '/hydrology': [
      { label: 'Reservatórios', path: '/hydrology', icon: 'water_drop', color: 'text-teal' },
      { label: 'Modelos MLOps', path: '/hydrology/models', icon: 'memory', color: 'text-teal' }
    ]
  };

  // Default menu if somehow we aren't in a recognized module
  defaultMenu = [
    { label: 'Portfólio', path: '/portfolio', icon: 'dashboard', color: 'text-blue' },
    { label: 'Operações', path: '/operations', icon: 'sync_alt', color: 'text-indigo' },
    { label: 'Precificação', path: '/pricing', icon: 'trending_up', color: 'text-cyan' },
    { label: 'Hidrologia', path: '/hydrology', icon: 'water_drop', color: 'text-teal' }
  ];

  navItems = this.defaultMenu;
  currentTheme = '';

  ngOnInit() {
    this.updateMenu(this.router.url);
    this.router.events.subscribe((event: any) => {
      if (event.urlAfterRedirects || event.url) {
        this.updateMenu(event.urlAfterRedirects || event.url);
      }
    });
  }

  private updateMenu(url: string) {
    const baseRoute = Object.keys(this.moduleMenus).find(route => url.startsWith(route));
    this.navItems = baseRoute ? this.moduleMenus[baseRoute] : this.defaultMenu;
    
    // Define active theme
    if (url.startsWith('/portfolio')) this.currentTheme = 'theme-portfolio';
    else if (url.startsWith('/operations')) this.currentTheme = 'theme-operations';
    else if (url.startsWith('/pricing')) this.currentTheme = 'theme-pricing';
    else if (url.startsWith('/hydrology')) this.currentTheme = 'theme-hydrology';
    else this.currentTheme = '';
  }

  goHome() {
    this.router.navigate(['/']);
  }
}
