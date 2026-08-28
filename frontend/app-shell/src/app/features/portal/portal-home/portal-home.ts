import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { KeycloakService } from 'keycloak-angular';

@Component({
  selector: 'app-portal-home',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatMenuModule],
  templateUrl: './portal-home.html',
  styleUrl: './portal-home.scss'
})
export class PortalHomeComponent implements OnInit {
  private router = inject(Router);
  private keycloak = inject(KeycloakService);

  userProfile: any = null;
  userInitials: string = 'AD';
  
  allModules = [
    { 
      name: 'VoltTrade', 
      category: 'PORTFÓLIO & MERCADO',
      description: 'Gestão de ativos, posições e exposição energética.',
      path: '/portfolio', 
      color: 'portfolio-color', 
      icon: 'bar_chart',
      roles: ['Trader', 'Executive']
    },
    { 
      name: 'OpsCore', 
      category: 'OPERAÇÕES',
      description: 'Operações, contratos e processos de backoffice.',
      path: '/operations', 
      color: 'operations-color', 
      icon: 'layers',
      roles: ['Trader', 'Executive']
    },
    { 
      name: 'RiskVisor', 
      category: 'RISCO',
      description: 'Monitoramento e análise de risco energético.',
      path: '/pricing', 
      color: 'risk-color', 
      icon: 'security',
      roles: ['RiskAnalyst', 'Executive']
    },
    { 
      name: 'Fluvius', 
      category: 'HIDROLOGIA',
      description: 'Dados, indicadores e análises hidrológicas.',
      path: '/hydrology', 
      color: 'hydrology-color', 
      icon: 'water_drop',
      roles: ['Trader', 'RiskAnalyst', 'Executive']
    }
  ];

  modules: any[] = [];

  recentActivities = [
    { module: 'VoltTrade', time: 'Último acesso hoje às 14:32', color: 'portfolio-color', icon: 'bar_chart' },
    { module: 'RiskVisor', time: 'Último acesso ontem às 18:10', color: 'risk-color', icon: 'security' },
    { module: 'Fluvius', time: 'Último acesso 25/08/2026', color: 'hydrology-color', icon: 'water_drop' }
  ];

  async ngOnInit() {
    if (await this.keycloak.isLoggedIn()) {
      this.userProfile = await this.keycloak.loadUserProfile();
      
      const first = this.userProfile?.firstName?.charAt(0) || '';
      const last = this.userProfile?.lastName?.charAt(0) || '';
      this.userInitials = (first + last).toUpperCase() || 'U';

      const userRoles = this.keycloak.getUserRoles();
      
      // Filter modules based on user roles
      this.modules = this.allModules.filter(mod => 
        mod.roles.some(role => userRoles.includes(role))
      );
    }
  }

  navigateTo(path: string) {
    this.router.navigate([path]);
  }

  logout() {
    this.keycloak.logout(window.location.origin);
  }
}
