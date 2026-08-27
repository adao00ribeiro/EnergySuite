import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { IndicatorCardsComponent } from './components/indicator-cards/indicator-cards.component';
import { PositionChartComponent } from './components/position-chart/position-chart.component';

interface PortfolioData {
  totalPurchased: number;
  totalSold: number;
  netPosition: number;
  estimatedResult: number;
  monthlyData: any[];
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
    IndicatorCardsComponent,
    PositionChartComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  portfolioData = signal<PortfolioData | null>(null);
  selectedPortfolio = signal<string>('portfolio_1');

  ngOnInit() {
    this.loadMockData();
  }

  onPortfolioChange(portfolioId: string) {
    this.selectedPortfolio.set(portfolioId);
    this.loadMockData();
  }

  private loadMockData() {
    setTimeout(() => {
      const isPortfolio1 = this.selectedPortfolio() === 'portfolio_1';
      
      const months = ['2026-01', '2026-02', '2026-03', '2026-04', '2026-05', '2026-06', '2026-07', '2026-08', '2026-09', '2026-10', '2026-11', '2026-12'];
      const monthlyData = months.map(m => {
        const p = isPortfolio1 ? 120 + Math.random() * 30 : 80 + Math.random() * 20;
        const s = isPortfolio1 ? 100 + Math.random() * 40 : 90 + Math.random() * 10;
        return {
          month: m,
          purchased: p,
          sold: s,
          net: p - s
        };
      });

      this.portfolioData.set({
        totalPurchased: isPortfolio1 ? 150.5 : 85.0,
        totalSold: isPortfolio1 ? 120.0 : 95.0,
        netPosition: isPortfolio1 ? 30.5 : -10.0,
        estimatedResult: isPortfolio1 ? 450000 : -120000,
        monthlyData: monthlyData
      });
    }, 400);
  }
}
