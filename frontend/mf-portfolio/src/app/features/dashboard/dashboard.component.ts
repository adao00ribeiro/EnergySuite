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
      const submarkets = ['SE/CO', 'SUL', 'NE', 'NORTE'];
      
      const monthlyData = [];
      const detailedGaps = [];
      const heatmapPoints = [];

      let totalPurchased = 0;
      let totalSold = 0;

      for (let m = 0; m < months.length; m++) {
        let mPurchased = 0;
        let mSold = 0;

        for (let s = 0; s < submarkets.length; s++) {
          const p = (Math.random() * (isPortfolio1 ? 30 : 20));
          const sld = (Math.random() * (isPortfolio1 ? 35 : 15)); // Potential deficit
          
          mPurchased += p;
          mSold += sld;

          detailedGaps.push({
            month: months[m],
            submarket: submarkets[s],
            energySource: 'Convencional',
            purchased: p,
            sold: sld,
            netGap: p - sld
          });

          heatmapPoints.push({
            xIndex: m,
            yIndex: s,
            gapValue: p - sld
          });
        }

        monthlyData.push({
          month: months[m],
          purchased: mPurchased,
          sold: mSold,
          net: mPurchased - mSold
        });
        
        totalPurchased += mPurchased;
        totalSold += mSold;
      }

      this.portfolioData.set({
        totalPurchased: totalPurchased,
        totalSold: totalSold,
        netPosition: totalPurchased - totalSold,
        estimatedResult: isPortfolio1 ? 450000 : -120000,
        monthlyData: monthlyData,
        detailedGaps: detailedGaps,
        heatmap: {
          xAxisMonths: months,
          yAxisSubmarkets: submarkets,
          points: heatmapPoints
        }
      });
    }, 400);
  }
}
