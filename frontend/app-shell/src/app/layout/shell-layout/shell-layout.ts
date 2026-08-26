import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface NavItem {
  label: string;
  path: string;
}

export interface NavGroup {
  label: string;
  icon: string;
  path?: string; // Se não tiver path, funciona apenas como um grupo expansível (mas o usuário pediu para Dashboard ter path, e Portfólio ter path E submenus)
  badge?: number;
  children?: NavItem[];
}

@Component({
  selector: 'app-shell-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, MatSidenavModule, MatIconModule, MatTooltipModule],
  templateUrl: './shell-layout.html',
  styleUrl: './shell-layout.scss'
})
export class ShellLayoutComponent implements OnInit {
  private router = inject(Router);

  isCollapsed = false;
  expandedGroups: Record<string, boolean> = {};

  navGroups: NavGroup[] = [
    {
      label: 'Dashboard',
      icon: 'dashboard',
      path: '/'
    },
    {
      label: 'Portfólio',
      icon: 'work',
      children: [
        { label: 'Visão Geral', path: '/portfolio' },
        { label: 'Ativos', path: '/portfolio/assets' },
        { label: 'Contratos', path: '/portfolio/contracts' },
        { label: 'Alocação', path: '/portfolio/allocation' }
      ]
    },
    {
      label: 'Energia',
      icon: 'bolt',
      children: [
        { label: 'Geração', path: '/hydrology/generation' },
        { label: 'Consumo', path: '/hydrology/consumption' },
        { label: 'Balanço Energético', path: '/hydrology' }
      ]
    },
    {
      label: 'Mercado',
      icon: 'bar_chart',
      children: [
        { label: 'Preços', path: '/pricing' },
        { label: 'Exposição', path: '/pricing/exposure' },
        { label: 'Posição de Mercado', path: '/pricing/position' }
      ]
    },
    {
      label: 'Contratos',
      icon: 'description',
      children: [
        { label: 'Bilaterais', path: '/operations/contracts' },
        { label: 'PPA', path: '/operations/ppa' },
        { label: 'Obrigações', path: '/operations/obligations' }
      ]
    },
    {
      label: 'Relatórios',
      icon: 'assessment',
      children: [
        { label: 'Relatórios Executivos', path: '/reports/executive' },
        { label: 'Performance', path: '/reports/performance' },
        { label: 'Exportações', path: '/reports/exports' }
      ]
    },
    {
      label: 'Alertas',
      icon: 'notifications',
      path: '/alerts',
      badge: 3
    }
  ];

  managementGroups: NavGroup[] = [
    {
      label: 'Configurações',
      icon: 'settings',
      path: '/settings'
    },
    {
      label: 'Usuários e Permissões',
      icon: 'people',
      path: '/users'
    }
  ];

  ngOnInit() {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.autoExpandActiveGroup(event.urlAfterRedirects);
      }
    });
    
    // Initial expansion check
    setTimeout(() => this.autoExpandActiveGroup(this.router.url), 100);
  }

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }

  toggleGroup(groupLabel: string) {
    if (this.isCollapsed) {
      this.isCollapsed = false; // Expande o sidebar se clicar no grupo estando fechado
    }
    this.expandedGroups[groupLabel] = !this.expandedGroups[groupLabel];
  }

  isGroupActive(group: NavGroup): boolean {
    if (group.path && this.router.url === group.path) return true;
    if (group.children) {
      return group.children.some(child => this.router.url.startsWith(child.path));
    }
    return false;
  }

  private autoExpandActiveGroup(url: string) {
    for (const group of this.navGroups) {
      if (group.children && group.children.some(c => url.startsWith(c.path))) {
        this.expandedGroups[group.label] = true;
      }
    }
    for (const group of this.managementGroups) {
      if (group.children && group.children.some(c => url.startsWith(c.path))) {
        this.expandedGroups[group.label] = true;
      }
    }
  }

  handleGroupClick(group: NavGroup) {
    if (group.children && group.children.length > 0) {
      this.toggleGroup(group.label);
    } else if (group.path) {
      this.router.navigate([group.path]);
    }
  }

  goHome() {
    this.router.navigate(['/']);
  }
}
