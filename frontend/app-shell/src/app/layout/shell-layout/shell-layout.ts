import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { KeycloakService } from 'keycloak-angular';

export interface NavItem {
  label: string;
  path: string;
}

export interface NavGroup {
  label: string;
  icon: string;
  path?: string; // Se não tiver path, funciona apenas como um grupo expansível
  badge?: number;
  children?: NavItem[];
}

@Component({
  selector: 'app-shell-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, MatSidenavModule, MatIconModule, MatTooltipModule, MatMenuModule, MatDividerModule],
  templateUrl: './shell-layout.html',
  styleUrl: './shell-layout.scss'
})
export class ShellLayoutComponent implements OnInit {
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private keycloak = inject(KeycloakService);

  isCollapsed = false;
  isDarkTheme = true;
  expandedGroups: Record<string, boolean> = {};

  userProfile: any = null;
  userRoles: string[] = [];
  userInitials: string = 'US';

  toggleTheme() {
    this.isDarkTheme = !this.isDarkTheme;
    if (this.isDarkTheme) {
      document.body.classList.remove('light-theme');
    } else {
      document.body.classList.add('light-theme');
    }
  }

  navGroups: NavGroup[] = [];
  currentTheme = '';

  private moduleMenus: Record<string, NavGroup[]> = {
    '/portfolio': [
      { label: 'Dashboard', icon: 'dashboard', path: '/portfolio/dashboard' },
      { label: 'Estratégias', icon: 'analytics', path: '/portfolio/strategies' },
      { label: 'Oportunidades', icon: 'lightbulb', path: '/portfolio/opportunities' }
    ],
    '/operations': [
      { label: 'Dashboard', icon: 'dashboard', path: '/operations' },
      { label: 'Cadastro Comercial', icon: 'domain', children: [{ label: 'Empresas', path: '/operations/commercial/companies' }] },
      { label: 'Operações', icon: 'swap_horiz', children: [{ label: 'Boletas e Operações', path: '/operations/tickets' }, { label: 'Portfólios', path: '/operations/portfolios' }] },
      { label: 'Central de Aprovação', icon: 'verified', path: '/operations/approvals' },
      { label: 'Financeiro', icon: 'attach_money', path: '/operations/finance' },
      { label: 'Integração CCEE', icon: 'electric_bolt', path: '/operations/ccee' }
    ],
    '/hydrology': [
      { label: 'Pluvia Dashboard', icon: 'water_drop', path: '/hydrology' }
    ],
    '/pricing': [
      { label: 'Dashboard', icon: 'dashboard', path: '/pricing' },
      { label: 'Prospecção (Energy Prospect)', icon: 'explore', path: '/pricing/prospect' }
    ]
  };

  private defaultMenu: NavGroup[] = [
    { label: 'Home', icon: 'home', path: '/' }
  ];

  managementGroups: NavGroup[] = [
    { label: 'Alertas', icon: 'notifications', path: '/alerts', badge: 3 },
    { label: 'Configurações', icon: 'settings', path: '/settings' },
    { label: 'Usuários', icon: 'people', path: '/users' }
  ];

  async ngOnInit() {
    if (await this.keycloak.isLoggedIn()) {
      try {
        const kcInstance = this.keycloak.getKeycloakInstance();
        const tokenParsed = kcInstance.tokenParsed;
        
        if (tokenParsed) {
          this.userProfile = {
            firstName: tokenParsed['given_name'] || tokenParsed['preferred_username'] || 'Usuário',
            lastName: tokenParsed['family_name'] || '',
            email: tokenParsed['email'] || ''
          };
        }

        const first = this.userProfile?.firstName?.charAt(0) || '';
        const last = this.userProfile?.lastName?.charAt(0) || '';
        this.userInitials = (first + last).toUpperCase() || 'US';
      } catch (err) {
        console.error('Failed to parse user info', err);
        this.userInitials = 'US';
      }
      this.userRoles = this.keycloak.getUserRoles();
    }

    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.updateMenu(event.urlAfterRedirects);
        this.autoExpandActiveGroup(event.urlAfterRedirects);
        this.cdr.detectChanges();
      }
    });
    
    setTimeout(() => {
      this.updateMenu(this.router.url);
      this.autoExpandActiveGroup(this.router.url);
      this.cdr.detectChanges();
    }, 100);
  }

  private updateMenu(url: string) {
    const baseRoute = Object.keys(this.moduleMenus).find(route => url.startsWith(route));
    this.navGroups = baseRoute ? this.moduleMenus[baseRoute] : this.defaultMenu;
    
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
      this.isCollapsed = false;
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

  logout() {
    this.keycloak.logout(window.location.origin);
  }
}
