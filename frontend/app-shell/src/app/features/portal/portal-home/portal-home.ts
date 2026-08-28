import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { KeycloakService } from 'keycloak-angular';

@Component({
  selector: 'app-portal-home',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatMenuModule, MatDividerModule],
  templateUrl: './portal-home.html',
  styleUrl: './portal-home.scss'
})
export class PortalHomeComponent implements OnInit {
  private router = inject(Router);
  private keycloak = inject(KeycloakService);
  private cdr = inject(ChangeDetectorRef);

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
        this.userInitials = (first + last).toUpperCase() || 'AD';
      } catch (err) {
        console.error('Failed to parse user info', err);
        this.userInitials = 'AD';
      }

      const userRoles = this.keycloak.getUserRoles();
      
      // Admin bypass or role filtering
      const isAdmin = userRoles.includes('admin') || userRoles.includes('default-roles-energysuite');
      
      this.modules = this.allModules.filter(mod => 
        isAdmin || mod.roles.some(role => userRoles.includes(role))
      );
      
      this.cdr.detectChanges();
    }
  }

  navigateTo(path: string) {
    this.router.navigate([path]);
  }

  logout() {
    this.keycloak.logout(window.location.origin);
  }
}
