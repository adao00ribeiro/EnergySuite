import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTabsModule } from '@angular/material/tabs';
import { IndicatorCardsComponent } from './components/indicator-cards/indicator-cards.component';
import { PositionChartComponent } from './components/position-chart/position-chart.component';
import { PositionGridComponent } from './components/position-grid/position-grid.component';
import { HeatmapChartComponent } from './components/heatmap-chart/heatmap-chart.component';
import { PortfolioService } from '../../core/services/portfolio.service';
import { inject } from '@angular/core';

interface PortfolioData {
  totalPurchased: number;
  totalSold: number;
  netPosition: number;
  estimatedResult: number;
  monthlyData: any[];
  detailedGaps: any[];
  heatmap: any;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTabsModule,
    IndicatorCardsComponent,
    PositionChartComponent,
    PositionGridComponent,
    HeatmapChartComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  portfolioData = signal<any | null>(null);
  selectedPortfolio = signal<string>('portfolio_1');
  portfolioService = inject(PortfolioService);

  ngOnInit() {
    this.loadData();
  }

  onPortfolioChange(portfolioId: string) {
    this.selectedPortfolio.set(portfolioId);
    this.loadData();
  }

  private loadData() {
    this.portfolioService.getDashboardData().subscribe({
      next: (data: any) => {
        this.portfolioData.set(data);
      },
      error: (err) => {
        console.error('Error fetching dashboard data:', err);
      }
    });
  }

  onNewOpportunity() {
    alert('Abertura do formulário de "Nova Oportunidade" será implementada em breve!');
  }
}
