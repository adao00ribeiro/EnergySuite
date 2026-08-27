import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-portal-home',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './portal-home.html',
  styleUrl: './portal-home.scss'
})
export class PortalHomeComponent {
  private router = inject(Router);

  modules = [
    { 
      name: 'Gestão de Portfólio', 
      category: 'PORTFÓLIO & MERCADO',
      description: 'Gestão de ativos, posições e exposição energética.',
      path: '/portfolio', 
      color: 'portfolio-color', 
      icon: 'bar_chart' 
    },
    { 
      name: 'ETRM & Backops', 
      category: 'OPERAÇÕES',
      description: 'Operações, contratos e processos de backoffice.',
      path: '/operations', 
      color: 'operations-color', 
      icon: 'layers' 
    },
    { 
      name: 'Imeris', 
      category: 'RISCO',
      description: 'Monitoramento e análise de risco energético.',
      path: '/pricing', 
      color: 'risk-color', 
      icon: 'security' 
    },
    { 
      name: 'Pluvia', 
      category: 'HIDROLOGIA',
      description: 'Dados, indicadores e análises hidrológicas.',
      path: '/hydrology', 
      color: 'hydrology-color', 
      icon: 'water_drop' 
    }
  ];

  recentActivities = [
    { module: 'Gestão de Portfólio', time: 'Último acesso hoje às 14:32', color: 'portfolio-color', icon: 'bar_chart' },
    { module: 'Imeris', time: 'Último acesso ontem às 18:10', color: 'risk-color', icon: 'security' },
    { module: 'Pluvia', time: 'Último acesso 25/08/2026', color: 'hydrology-color', icon: 'water_drop' }
  ];

  navigateTo(path: string) {
    this.router.navigate([path]);
  }
}
