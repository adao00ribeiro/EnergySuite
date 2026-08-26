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

  navGroups: NavGroup[] = [];
  currentTheme = '';

  // Global Contexts mapped to the new NavGroup structure
  private moduleMenus: Record<string, NavGroup[]> = {
    '/portfolio': [
      {
        label: 'Dashboard',
        icon: 'dashboard',
        path: '/portfolio'
      },
      {
        label: 'Gestão de Portfólio',
        icon: 'work',
        children: [
          { label: 'Visão Geral', path: '/portfolio' },
          { label: 'Ativos', path: '/portfolio/assets' },
          { label: 'Contratos', path: '/portfolio/contracts' },
          { label: 'Alocação', path: '/portfolio/allocation' }
        ]
      },
      {
        label: 'Relatórios',
        icon: 'assessment',
        children: [
          { label: 'Performance', path: '/portfolio/reports/performance' },
          { label: 'Extratos', path: '/portfolio/reports/extracts' }
        ]
      }
    ],
    '/operations': [
      {
        label: 'Dashboard',
        icon: 'dashboard',
        path: '/operations'
      },
      {
        label: 'Cadastro Comercial',
        icon: 'domain',
        children: [
          { label: 'Grupos Econômicos', path: '/operations/commercial/groups' },
          { label: 'Empresas', path: '/operations/commercial/companies' },
          { label: 'Pessoas', path: '/operations/commercial/people' }
        ]
      },
      {
        label: 'Operações',
        icon: 'swap_horiz',
        children: [
          { label: 'Boletas e Operações', path: '/operations/tickets' },
          { label: 'Vínculos (SWAP/Inter)', path: '/operations/links' },
          { label: 'Portfólios', path: '/operations/portfolios' }
        ]
      },
      {
        label: 'Contratos',
        icon: 'description',
        children: [
          { label: 'Gestão de Contratos', path: '/operations/contracts/manage' },
          { label: 'Aditivos', path: '/operations/contracts/amendments' },
          { label: 'Reajustes', path: '/operations/contracts/readjustments' }
        ]
      },
      {
        label: 'Central de Aprovação',
        icon: 'verified',
        path: '/operations/approvals'
      },
      {
        label: 'Financeiro',
        icon: 'attach_money',
        children: [
          { label: 'Contas a Pagar', path: '/operations/finance/payables' },
          { label: 'Contas a Receber', path: '/operations/finance/receivables' },
          { label: 'Faturamento', path: '/operations/finance/billing' },
          { label: 'Encontro de Contas', path: '/operations/finance/offset' }
        ]
      },
      {
        label: 'Integração CCEE',
        icon: 'electric_bolt',
        children: [
          { label: 'Comparador CCEE', path: '/operations/ccee/compare' },
          { label: 'Exportação XML', path: '/operations/ccee/export' },
          { label: 'Ajustes e Validação', path: '/operations/ccee/adjustments' }
        ]
      }
    ],
    '/hydrology': [
      {
        label: 'Dashboard',
        icon: 'dashboard',
        path: '/hydrology'
      },
      {
        label: 'Recursos Hídricos',
        icon: 'water_drop',
        children: [
          { label: 'Geração', path: '/hydrology/generation' },
          { label: 'Consumo', path: '/hydrology/consumption' },
          { label: 'Balanço Energético', path: '/hydrology' }
        ]
      },
      {
        label: 'Modelos',
        icon: 'memory',
        children: [
          { label: 'MLOps', path: '/hydrology/models' }
        ]
      }
    ],
    '/pricing': [
      {
        label: 'Dashboard',
        icon: 'dashboard',
        path: '/pricing'
      },
      {
        label: 'Mercado & Risco',
        icon: 'trending_up',
        children: [
          { label: 'Preços', path: '/pricing' },
          { label: 'Exposição', path: '/pricing/exposure' },
          { label: 'Posição de Mercado', path: '/pricing/position' },
          { label: 'Curva Forward', path: '/pricing/curves' },
          { label: 'Cenários', path: '/pricing/scenarios' }
        ]
      }
    ]
  };

  // Fallback se estiver na home ou rota não reconhecida
  private defaultMenu: NavGroup[] = [
    {
      label: 'Home',
      icon: 'home',
      path: '/'
    }
  ];

  managementGroups: NavGroup[] = [
    {
      label: 'Alertas',
      icon: 'notifications',
      path: '/alerts',
      badge: 3
    },
    {
      label: 'Configurações',
      icon: 'settings',
      path: '/settings'
    },
    {
      label: 'Usuários',
      icon: 'people',
      path: '/users'
    }
  ];

  ngOnInit() {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.updateMenu(event.urlAfterRedirects);
        this.autoExpandActiveGroup(event.urlAfterRedirects);
      }
    });
    
    // Initial load
    setTimeout(() => {
      this.updateMenu(this.router.url);
      this.autoExpandActiveGroup(this.router.url);
    }, 100);
  }

  private updateMenu(url: string) {
    const baseRoute = Object.keys(this.moduleMenus).find(route => url.startsWith(route));
    this.navGroups = baseRoute ? this.moduleMenus[baseRoute] : this.defaultMenu;
    
    // Define active theme
    if (url.startsWith('/portfolio')) this.currentTheme = 'theme-portfolio';
    else if (url.startsWith('/operations')) this.currentTheme = 'theme-operations';
    else if (url.startsWith('/pricing')) this.currentTheme = 'theme-pricing';
    else if (url.startsWith('/hydrology')) this.currentTheme = 'theme-hydrology';
    else this.currentTheme = '';
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
